using Microsoft.EntityFrameworkCore;
using ReportsMS.Infrastructure.Persistence;
using ReportsMS.Infrastructure.Repositories;
using ReportsMS.Domain.Interfaces;
using ReportsMS.Infrastructure.Consumers;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReportsMS.Application.Queries;
using MongoDB.Driver;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson;
using ReportsMS.Application.Behaviors;
using ReportsMS.Application.Interfaces;
using MediatR;

try
{
    BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
}
catch (BsonSerializationException) { }

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ReportsDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<IReportsRepository, ReportsRepository>();
builder.Services.AddScoped<ReportsMS.Infrastructure.Services.CsvExportService>();

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetSalesReportQuery).Assembly));

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<SalesRecordingConsumer>();
    x.AddConsumer<EventCreatedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitMqConnectionString = builder.Configuration.GetConnectionString("RabbitMQ");
        if (string.IsNullOrEmpty(rabbitMqConnectionString)) rabbitMqConnectionString = "rabbitmq://rabbitmq-server/";

        cfg.Host(rabbitMqConnectionString);
        
        cfg.ConfigureEndpoints(context);
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IMongoClient>(sp => 
    new MongoClient(builder.Configuration["MongoDb:ConnectionString"]));
builder.Services.AddScoped<IAuditService, ReportsMS.Infrastructure.Services.MongoAuditService>();
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AuditBehavior<,>));
builder.Services.AddHttpContextAccessor();

builder.Services.AddHostedService<ReportsMS.Infrastructure.BackgroundJobs.MetricsAggregationWorker>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ReportsDbContext>();
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