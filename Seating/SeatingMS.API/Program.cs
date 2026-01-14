using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using SeatingMS.Infrastructure.Persistence;
using SeatingMS.Infrastructure.Repositories;
using SeatingMS.Domain.Interfaces;
using MassTransit;
using SeatingMS.Infrastructure.Consumers;

using MongoDB.Driver;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson;
using MediatR;
using SeatingMS.Application.Behaviors;
using SeatingMS.Application.Interfaces;

namespace SeatingMS.API
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

            builder.Services.AddDbContext<SeatingDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddScoped<IEventSeatRepository, EventSeatRepository>();

            builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(SeatingMS.Application.Commands.LockSeat.LockSeatCommand).Assembly));

            builder.Services.AddMassTransit(x =>
            {
                x.AddConsumer<EventCreatedConsumer>();
                x.AddConsumer<BookingConfirmedConsumer>();
                x.AddConsumer<BookingCancelledConsumer>();
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

                    cfg.ConfigureEndpoints(context);
                });
            });
            
            builder.Services.AddHostedService<SeatingMS.Infrastructure.BackgroundJobs.SeatLockExpirationWorker>();

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            
            builder.Services.AddSingleton<IMongoClient>(sp => 
                new MongoClient(builder.Configuration["MongoDb:ConnectionString"]));
            builder.Services.AddScoped<IAuditService, MongoAuditService>();
            builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AuditBehavior<,>));
            builder.Services.AddHttpContextAccessor();

            var app = builder.Build();

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