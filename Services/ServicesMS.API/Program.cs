using Microsoft.EntityFrameworkCore;
using ServicesMS.Infrastructure.Persistence;
using ServicesMS.Infrastructure.Repositories;
using ServicesMS.Domain.Interfaces;
using ServicesMS.Infrastructure.Consumers;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ServicesMS.Application.Commands;

using MongoDB.Driver;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson;
using ServicesMS.Application.Behaviors;
using ServicesMS.Application.Interfaces;
using MediatR;

try
{
    BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
}
catch (BsonSerializationException) { }

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ServicesDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<IServiceRepository, ServiceRepository>();

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(BookServiceCommand).Assembly));

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<BookingConfirmedConsumer>();
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

builder.Services.AddSingleton<IMongoClient>(sp => new MongoClient(builder.Configuration["MongoDb:ConnectionString"]));
builder.Services.AddScoped<IAuditService, MongoAuditService>();
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AuditBehavior<,>));
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ServicesDbContext>();
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
