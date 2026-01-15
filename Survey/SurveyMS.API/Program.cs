
using Microsoft.EntityFrameworkCore;
using SurveyMS.Infrastructure.Persistence;
using SurveyMS.Domain.Interfaces;
using SurveyMS.Infrastructure.Repositories;
using SurveyMS.Application.Commands.SubmitFeedback;
using MediatR;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<SurveyDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<IFeedbackRepository, FeedbackRepository>();

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(SubmitFeedbackCommand).Assembly));

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
builder.Services.AddOpenApi();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SurveyDbContext>();
    db.Database.Migrate();
}

app.MapControllers();
app.MapOpenApi();

app.Run();
