using EventsMS.Infrastructure.Persistence;
using EventsMS.Infrastructure.Repository;
using MassTransit;
using EventsMS.Domain.Interfaces;
using EventsMS.Application;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace EventsMS.API
{
    [ExcludeFromCodeCoverage]
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 1. DB Context
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddDbContext<EventsDbContext>(options =>
                options.UseNpgsql(connectionString));

            // 2. Repositorios
            builder.Services.AddScoped<IEventRepository, EventRepository>();

            // 3. MediatR
            builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(EventsMS.Application.Commands.CreateEvent.CreateEventCommand).Assembly));

            // 4. MassTransit
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

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Migración automática al inicio (solo dev)
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

            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}