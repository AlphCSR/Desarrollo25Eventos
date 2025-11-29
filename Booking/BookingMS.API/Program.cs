using Microsoft.EntityFrameworkCore;
using BookingMS.Infrastructure.Persistence.Configuration;
using BookingMS.Infrastructure.Repositories;
using BookingMS.Domain.Interfaces;
using MassTransit;
using BookingMS.Infrastructure.Services;
using BookingMS.Shared.Events;
using BookingMS.Infrastructure.Consumers;
using BookingMS.Application.Commands.CreateBooking;
using System.Diagnostics.CodeAnalysis;

namespace BookingMS.API
{
    [ExcludeFromCodeCoverage]
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 1. DB Context
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            var dataSourceBuilder = new Npgsql.NpgsqlDataSourceBuilder(connectionString);
            dataSourceBuilder.EnableDynamicJson();
            var dataSource = dataSourceBuilder.Build();

            builder.Services.AddDbContext<BookingDbContext>(options =>
                options.UseNpgsql(dataSource));

            // 2. Repositorios
            builder.Services.AddScoped<IBookingRepository, BookingRepository>();
            builder.Services.AddScoped<IEventPublisher, RabbitMqEventPublisher>();

            // 3. MediatR
            builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateBookingCommand).Assembly));

            // 4. MassTransit
            builder.Services.AddMassTransit(x =>
            {
                x.AddConsumer<PaymentCapturedConsumer>();
                x.AddConsumer<PaymentFailedConsumer>();
                x.AddConsumer<SeatUnlockedConsumer>();
                
                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host("localhost", "/", h => {
                        h.Username("guest");
                        h.Password("guest");
                    });
                    
                    cfg.ReceiveEndpoint("booking-payment-captured", e =>
                    {
                        e.ConfigureConsumer<PaymentCapturedConsumer>(context);
                    });
                    
                    cfg.ReceiveEndpoint("booking-payment-failed", e =>
                    {
                        e.ConfigureConsumer<PaymentFailedConsumer>(context);
                    });

                    cfg.ReceiveEndpoint("booking-seat-unlocked", e =>
                    {
                        e.ConfigureConsumer<SeatUnlockedConsumer>(context);
                    });
                });

            });

            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
                });
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Migraciones automáticas
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

            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}