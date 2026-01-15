
using MassTransit;
using InvoicingMS.Infrastructure.Services;
using InvoicingMS.Infrastructure.Consumers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IPdfGenerator, QuestPdfGenerator>();

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

        cfg.ReceiveEndpoint("invoicing-booking-confirmed", e =>
        {
            e.ConfigureConsumer<BookingConfirmedConsumer>(context);
        });
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

var app = builder.Build();

app.MapControllers();
app.MapOpenApi();

app.Run();
