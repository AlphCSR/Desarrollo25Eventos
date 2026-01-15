using Microsoft.EntityFrameworkCore;
using MarketingMS.Infrastructure.Persistence;
using MarketingMS.Infrastructure.Repositories;
using MarketingMS.Domain.Interfaces;
using MarketingMS.Domain.Entities;
using MarketingMS.Application.Commands.CreateCoupon;
using MediatR;
using MassTransit;
using System.Text.Json.Serialization;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text.Json;

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
    });

builder.Services.AddAuthorization();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<MarketingDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<ICouponRepository, CouponRepository>();
builder.Services.AddScoped<IUserInterestRepository, UserInterestRepository>();
builder.Services.AddScoped<MarketingMS.Application.Interfaces.IEventsService, MarketingMS.Infrastructure.Services.EventsService>();

builder.Services.AddHttpClient<MarketingMS.Application.Interfaces.IEventsService, MarketingMS.Infrastructure.Services.EventsService>(client =>
{
    client.BaseAddress = new Uri("http://events-ms:8080/");
});

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateCouponCommand).Assembly));

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<MarketingMS.Infrastructure.Consumers.BookingConfirmedConsumer>();
    x.AddConsumer<MarketingMS.Infrastructure.Consumers.UserProfileUpdatedConsumer>();

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

        cfg.ReceiveEndpoint("marketing-booking-confirmed", e =>
        {
            e.ConfigureConsumer<MarketingMS.Infrastructure.Consumers.BookingConfirmedConsumer>(context);
        });

        cfg.ReceiveEndpoint("marketing-user-profile-updated", e =>
        {
            e.ConfigureConsumer<MarketingMS.Infrastructure.Consumers.UserProfileUpdatedConsumer>(context);
        });
    });
});

builder.Services.AddControllers()
    .AddJsonOptions(options => {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi(); 

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MarketingDbContext>();
    db.Database.Migrate();

    if (!db.Coupons.Any())
    {
        db.Coupons.AddRange(
            new Coupon("BIENVENIDA", DiscountType.Percentage, 10, null, 100, 0),
            new Coupon("PROMO50", DiscountType.FixedAmount, 50, DateTime.UtcNow.AddMonths(1), 50, 200),
            new Coupon("FLASH25", DiscountType.Percentage, 25, DateTime.UtcNow.AddDays(7), 20, 0)
        );
        db.SaveChanges();
    }
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapOpenApi();

app.Run();
