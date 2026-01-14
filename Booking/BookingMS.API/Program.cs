using Microsoft.EntityFrameworkCore;
using BookingMS.Infrastructure.Persistence.Configuration;
using BookingMS.Infrastructure.Repositories;
using BookingMS.Domain.Interfaces;
using MassTransit;
using BookingMS.Infrastructure.Services;
using BookingMS.Shared.Events;
using BookingMS.Infrastructure.Consumers;
using BookingMS.Application.Commands.CreateBooking;
using BookingMS.Infrastructure.BackgroundJobs;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using MongoDB.Driver;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson;
using MediatR;
using BookingMS.Application.Behaviors;
using BookingMS.Application.Interfaces;
using Hangfire;
using Hangfire.PostgreSql;
using BookingMS.API;

namespace BookingMS.API
{
    [ExcludeFromCodeCoverage]
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
            }
            catch (BsonSerializationException) { }

            var builder = WebApplication.CreateBuilder(args);

            var keycloakUrl = builder.Configuration["Keycloak:AuthServerUrl"] ?? "http://localhost:8080";
            var realm = builder.Configuration["Keycloak:Realm"] ?? "Users-Ms";

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Authority = $"{keycloakUrl}/realms/{realm}";
                    options.RequireHttpsMetadata = false; 
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = false, 
                        ValidateAudience = false,
                        NameClaimType = "preferred_username"
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnTokenValidated = context =>
                        {
                            var claimsIdentity = context.Principal?.Identity as ClaimsIdentity;
                            if (claimsIdentity != null)
                            {
                                var resourceAccess = context.Principal?.FindFirst("resource_access")?.Value;
                                if (!string.IsNullOrEmpty(resourceAccess))
                                {
                                    using var doc = JsonDocument.Parse(resourceAccess);
                                    if (doc.RootElement.TryGetProperty("publi-client", out var clientElement) &&
                                        clientElement.TryGetProperty("roles", out var rolesElement))
                                    {
                                        foreach (var role in rolesElement.EnumerateArray())
                                        {
                                            claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, role.GetString() ?? ""));
                                        }
                                    }
                                }
                            }
                            return Task.CompletedTask;
                        }
                    };
                });

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            var dataSourceBuilder = new Npgsql.NpgsqlDataSourceBuilder(connectionString);
            dataSourceBuilder.EnableDynamicJson();
            var dataSource = dataSourceBuilder.Build();

            builder.Services.AddDbContext<BookingDbContext>(options =>
                options.UseNpgsql(dataSource));

            builder.Services.AddScoped<IBookingRepository, BookingRepository>();
            builder.Services.AddScoped<IEventPublisher, RabbitMqEventPublisher>();
            
            builder.Services.AddHttpClient<IMarketingService, MarketingService>();
            builder.Services.AddHttpClient<ISeatingService, SeatingService>(c => c.BaseAddress = new Uri("http://seating-ms:8080"));
            builder.Services.AddHttpClient<IServicesService, ServicesService>(c => c.BaseAddress = new Uri("http://services-ms:8080"));

            builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateBookingCommand).Assembly));

            builder.Services.AddMassTransit(x =>
            {
                x.AddConsumer<PaymentCapturedConsumer>();
                x.AddConsumer<PaymentFailedConsumer>();
                x.AddConsumer<SeatUnlockedConsumer>();
                x.AddConsumer<EventCancelledConsumer>();
                
                x.UsingRabbitMq((context, cfg) =>
                {
                    var rabbitMqHost = builder.Configuration.GetConnectionString("RabbitMQ");
                    if (!string.IsNullOrEmpty(rabbitMqHost))
                    {
                         cfg.Host(rabbitMqHost);
                    }
                    else
                    {
                         cfg.Host("localhost", "/", h => {
                             h.Username("guest");
                             h.Password("guest");
                         });
                    }
                    
                    cfg.ConfigureEndpoints(context);
                });

            });

            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });
            builder.Services.AddAuthorization();
            builder.Services.AddHttpContextAccessor();
            
            builder.Services.AddSingleton<IMongoClient>(sp => 
                new MongoClient(builder.Configuration["MongoDb:ConnectionString"]));
            builder.Services.AddScoped<IAuditService, MongoAuditService>();
            builder.Services.AddScoped<IAuditService, MongoAuditService>();
            builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AuditBehavior<,>));

            builder.Services.AddHostedService<BookingMS.Infrastructure.Workers.BookingCleanupWorker>();

            builder.Services.AddScoped<BookingJobs>();
            builder.Services.AddHangfire(config => config
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UsePostgreSqlStorage(c => c.UseNpgsqlConnection(connectionString)));

            builder.Services.AddHangfireServer();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<BookingDbContext>();
                db.Database.Migrate();
            }

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseHangfireDashboard("/hangfire/booking", new DashboardOptions
            {
                Authorization = new[] { new HangfireDashboardAuthorizationFilter() }
            });

            using (var scope = app.Services.CreateScope())
            {
                 var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
                 recurringJobManager.AddOrUpdate<BookingJobs>(
                     "CleanupExpiredBookings", 
                     job => job.ProcessExpiredBookingsAsync(), 
                     Cron.Minutely());

                 recurringJobManager.AddOrUpdate<BookingJobs>(
                     "SendPaymentReminders", 
                     job => job.ProcessPaymentRemindersAsync(), 
                     Cron.Minutely());
            }

            app.MapControllers();
            app.Run();
        }
    }
}