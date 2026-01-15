using EventsMS.Infrastructure.Persistence;
using EventsMS.Infrastructure.Repository;
using MassTransit;
using EventsMS.Domain.Interfaces;
using EventsMS.Application;
using EventsMS.Application.Interfaces;
using EventsMS.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
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
using EventsMS.Application.Behaviors;
using Hangfire;
using Hangfire.PostgreSql;
using EventsMS.Infrastructure.BackgroundJobs;
using EventsMS.API;

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
            ValidIssuers = new[] { $"{keycloakUrl}/realms/{realm}", "http://localhost:8080/realms/Users-Ms" },
            ValidateAudience = false,
            ValidateLifetime = true,
            NameClaimType = "preferred_username"
        };
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"[EventsMS] Authentication failed: {context.Exception.Message}");
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                var claimsIdentity = context.Principal?.Identity as ClaimsIdentity;
                if (claimsIdentity != null)
                {
                    var resourceAccess = context.Principal?.FindFirst("resource_access")?.Value;
                    if (!string.IsNullOrEmpty(resourceAccess))
                    {
                        using var doc = JsonDocument.Parse(resourceAccess);
                        foreach (var client in doc.RootElement.EnumerateObject())
                        {
                            if (client.Value.TryGetProperty("roles", out var rolesElement))
                            {
                                foreach (var role in rolesElement.EnumerateArray())
                                {
                                    var roleName = role.GetString();
                                    if (roleName != null) {
                                        Console.WriteLine($"[EventsMS] Role from resource_access ({client.Name}): {roleName}");
                                        claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, roleName));
                                    }
                                }
                            }
                        }
                    }

                    var realmAccess = context.Principal?.FindFirst("realm_access")?.Value;
                    if (!string.IsNullOrEmpty(realmAccess))
                    {
                        using var doc = JsonDocument.Parse(realmAccess);
                        if (doc.RootElement.TryGetProperty("roles", out var rolesElement))
                        {
                            foreach (var role in rolesElement.EnumerateArray())
                            {
                                var roleName = role.GetString();
                                if (roleName != null) {
                                    Console.WriteLine($"[EventsMS] Role from realm_access: {roleName}");
                                    claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, roleName));
                                }
                            }
                        }
                    }
                }
                return Task.CompletedTask;
            }
        };
    });

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<EventsDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<IEventRepository, EventRepository>();

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(EventsMS.Application.Commands.CreateEvent.CreateEventCommand).Assembly));

builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitMqConnectionString = builder.Configuration.GetConnectionString("RabbitMQ");
        if (!string.IsNullOrEmpty(rabbitMqConnectionString))
        {
            cfg.Host(rabbitMqConnectionString);
        }
        else
        {
            cfg.Host("localhost", "/", h => {
                h.Username("guest");
                h.Password("guest");
            });
        }
    });
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();

builder.Services.AddSingleton<IMongoClient>(sp => 
    new MongoClient(builder.Configuration["MongoDb:ConnectionString"]));
builder.Services.AddScoped<IAuditService, MongoAuditService>();
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AuditBehavior<,>));
builder.Services.AddScoped<EventJobs>();

builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(c => c.UseNpgsqlConnection(connectionString)));

builder.Services.AddHangfireServer();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<EventsDbContext>();
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

app.UseHangfireDashboard("/hangfire/events", new DashboardOptions
{
    Authorization = new[] { new HangfireDashboardAuthorizationFilter() }
});

using (var scope = app.Services.CreateScope())
{
    var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
    recurringJobManager.AddOrUpdate<EventJobs>(
        "ProcessEventTransitions", 
        job => job.ProcessEventTransitionsAsync(), 
        Cron.Minutely());
}

app.MapControllers();

app.Run();