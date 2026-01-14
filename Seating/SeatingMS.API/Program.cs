using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using SeatingMS.Infrastructure.Persistence;
using SeatingMS.Infrastructure.Repositories;
using SeatingMS.Domain.Interfaces;
using MassTransit;
using SeatingMS.Infrastructure.Consumers;

namespace SeatingMS.API
{
    [ExcludeFromCodeCoverage]
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 1. DB Context
            builder.Services.AddDbContext<SeatingDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            // 2. Repositorios
            builder.Services.AddScoped<IEventSeatRepository, EventSeatRepository>();

            // 3. MediatR
            builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(SeatingMS.Application.Commands.LockSeat.LockSeatCommand).Assembly));

            // 4. MassTransit (RabbitMQ)
            builder.Services.AddMassTransit(x =>
            {
                x.AddConsumer<EventCreatedConsumer>();
                
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

                    cfg.ReceiveEndpoint("seating-event-created", e =>
                    {
                        e.ConfigureConsumer<EventCreatedConsumer>(context);
                    });
                });
            });

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Migraciones automáticas
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<SeatingDbContext>();
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