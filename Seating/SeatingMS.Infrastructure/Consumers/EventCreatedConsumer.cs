using MassTransit;
using Microsoft.Extensions.Logging;
using EventsMS.Shared.Events;
using SeatingMS.Domain.Interfaces;
using SeatingMS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using System.Diagnostics.CodeAnalysis;

namespace SeatingMS.Infrastructure.Consumers
{
    [ExcludeFromCodeCoverage]
    public class EventCreatedConsumer : IConsumer<EventCreatedEvent>
    {
        private readonly IEventSeatRepository _repository;
        private readonly ILogger<EventCreatedConsumer> _logger;

        public EventCreatedConsumer(IEventSeatRepository repository, ILogger<EventCreatedConsumer> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<EventCreatedEvent> context)
        {
            var message = context.Message;
            _logger.LogInformation($"Generando asientos para el evento: {message.Title}");

            var seatsToCreate = new List<EventSeat>();

            foreach (var section in message.Sections)
            {
                if (section.IsNumbered)
                {
                   
                    int rows = (int)Math.Ceiling((double)section.Capacity / 10);
                    
                    for (int r = 1; r <= rows; r++)
                    {
                        for (int c = 1; c <= 10; c++)
                        {
                            if (seatsToCreate.Count >= section.Capacity) break; // Parar si llenamos la capacidad
                            
                            string rowChar = ((char)('A' + r - 1)).ToString();
                            seatsToCreate.Add(new EventSeat(message.EventId, section.SectionId, rowChar, c));
                        }
                    }
                }
                else
                {
                    for (int i = 1; i <= section.Capacity; i++)
                    {
                        seatsToCreate.Add(new EventSeat(message.EventId, section.SectionId, "GEN", i));
                    }
                }
            }

            if (seatsToCreate.Count > 0)
            {
                await _repository.AddRangeAsync(seatsToCreate, context.CancellationToken);
                await _repository.SaveChangesAsync(context.CancellationToken);
                _logger.LogInformation($"Se generaron {seatsToCreate.Count} asientos exitosamente.");
            }
        }
    }
}