using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using UsersMS.Application;
using UsersMS.Domain.Interfaces;
using UsersMS.Infrastructure.Persistence;
using UsersMS.Infrastructure.Repositories;
using UsersMS.Application.Interfaces;
using UsersMS.Infrastructure.Services;
using UsersMS.Application.Behaviors;
using MediatR;
using MongoDB.Driver;
using MassTransit;
using UsersMS.Infrastructure.Consumers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text.Json;

using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson;

[assembly: ExcludeFromCodeCoverage]

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

builder.Services.AddAuthorization(); 
builder.Services.AddApplication(); 
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<UsersDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddSingleton<IMongoClient>(sp => 
    new MongoClient(builder.Configuration["MongoDb:ConnectionString"]));
builder.Services.AddScoped<IAuditService, MongoAuditService>();
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AuditBehavior<,>));
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<UserHistoryCreatedConsumer>();
    x.AddConsumer<EventCreatedConsumer>();
    x.AddConsumer<EventUpdatedConsumer>();
    x.AddConsumer<ServiceBookedConsumer>();
    x.AddConsumer<BookingCreatedConsumer>();
    x.AddConsumer<BookingConfirmedConsumer>();
    x.AddConsumer<BookingCancelledConsumer>();
    
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

        cfg.ReceiveEndpoint("user-history-created", e =>
        {
            e.ConfigureConsumer<UserHistoryCreatedConsumer>(context);
        });

        cfg.ReceiveEndpoint("user-event-created-history", e =>
        {
            e.ConfigureConsumer<EventCreatedConsumer>(context);
        });

        cfg.ReceiveEndpoint("user-event-updated-history", e =>
        {
            e.ConfigureConsumer<EventUpdatedConsumer>(context);
        });

        cfg.ReceiveEndpoint("user-service-booked-history", e =>
        {
            e.ConfigureConsumer<ServiceBookedConsumer>(context);
        });

        cfg.ReceiveEndpoint("user-booking-created-history", e =>
        {
            e.ConfigureConsumer<BookingCreatedConsumer>(context);
        });

        cfg.ReceiveEndpoint("user-booking-confirmed-history", e =>
        {
            e.ConfigureConsumer<BookingConfirmedConsumer>(context);
        });

        cfg.ReceiveEndpoint("user-booking-cancelled-history", e =>
        {
            e.ConfigureConsumer<BookingCancelledConsumer>(context);
        });
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient<IKeycloakService, KeycloakService>();

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<UsersDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection(); 
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();