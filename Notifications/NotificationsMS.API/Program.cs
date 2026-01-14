using NotificationsMS.Infrastructure.Consumers;
using NotificationsMS.Infrastructure.Services;
using NotificationsMS.Domain.Interfaces;
using NotificationsMS.API.Services;
using NotificationsMS.Application.Interfaces;
using NotificationsMS.Hubs;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var keycloakUrl = builder.Configuration["Keycloak:AuthServerUrl"];
var realm = builder.Configuration["Keycloak:Realm"];

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = $"{keycloakUrl}/realms/{realm}";
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false, 
            NameClaimType = "preferred_username"
        };
        
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) &&
                    (path.StartsWithSegments("/hubs/notifications")))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddSingleton<IEmailService, SendGridEmailService>();
builder.Services.AddSingleton<IEmailTemplateService, EmailTemplateService>();
builder.Services.AddScoped<INotifier, SignalRNotifier>();
builder.Services.AddSingleton<IUserIdProvider, CustomUserIdProvider>();
builder.Services.AddSignalR();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<BookingConfirmedConsumer>();
    x.AddConsumer<PaymentFailedConsumer>();
    x.AddConsumer<ServiceBookedConsumer>();
    x.AddConsumer<BookingCancelledConsumer>();
    x.AddConsumer<EventCancelledConsumer>();
    x.AddConsumer<PaymentExpiringSoonConsumer>();
    x.AddConsumer<SeatStatusUpdatedConsumer>();
    x.AddConsumer<EventStatusChangedConsumer>();

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
        
        
        cfg.ReceiveEndpoint("notifications-booking-confirmed", e =>
        {
            e.ConfigureConsumer<BookingConfirmedConsumer>(context);
        });

        cfg.ReceiveEndpoint("notifications-payment-failed", e =>
        {
            e.ConfigureConsumer<PaymentFailedConsumer>(context);
        });

        cfg.ReceiveEndpoint("notifications-service-booked", e =>
        {
            e.ConfigureConsumer<ServiceBookedConsumer>(context);
        });
        
        cfg.ReceiveEndpoint("notifications-booking-cancelled", e =>
        {
            e.ConfigureConsumer<BookingCancelledConsumer>(context);
        });

        cfg.ReceiveEndpoint("notifications-event-cancelled", e =>
        {
            e.ConfigureConsumer<EventCancelledConsumer>(context);
        });

        cfg.ReceiveEndpoint("notifications-payment-expiring", e =>
        {
            e.ConfigureConsumer<PaymentExpiringSoonConsumer>(context);
        });

        cfg.ReceiveEndpoint("notifications-seat-status-updated", e =>
        {
            e.ConfigureConsumer<SeatStatusUpdatedConsumer>(context);
        });

        cfg.ReceiveEndpoint("notifications-event-status-changed", e =>
        {
            e.ConfigureConsumer<EventStatusChangedConsumer>(context);
        });

    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("CorsPolicy"); 
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<NotificationHub>("/hubs/notifications");

app.Run();