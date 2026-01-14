using MassTransit;
using Microsoft.Extensions.Logging;
using EventsMS.Shared.Events;
using SeatingMS.Domain.Interfaces;
using SeatingMS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using System.Linq;
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
            _logger.LogInformation($"Recibido EventCreatedEvent. EventId: {message.EventId}, Title: {message.Title}, Sections: {message.Sections?.Count ?? 0}");

            if (message.Sections == null || message.Sections.Count == 0)
            {
                 _logger.LogWarning($"El evento {message.EventId} no tiene secciones.");
                 return;
            }

            var existingSeats = await _repository.GetByEventIdAsync(message.EventId, context.CancellationToken);
            if (existingSeats != null && existingSeats.Any())
            {
                _logger.LogInformation($"El evento {message.EventId} ya tiene {existingSeats.Count()} asientos. Omitiendo generación de duplicados.");
                return;
            }

            var seatsToCreate = new List<EventSeat>();

            foreach (var section in message.Sections)
            {
                _logger.LogInformation($"Procesando Sección: {section.Name}, ID: {section.SectionId}, Numerado: {section.IsNumbered}, Capacidad: {section.Capacity}");

                if (section.IsNumbered)
                {
                    int rows = (int)Math.Ceiling((double)section.Capacity / 10);
                    int createdForSection = 0;
                    
                    for (int r = 1; r <= rows; r++)
                    {
                        for (int c = 1; c <= 10; c++)
                        {
                            if (createdForSection >= section.Capacity) break;
                            
                            string rowChar = ((char)('A' + r - 1)).ToString();
                            seatsToCreate.Add(new EventSeat(message.EventId, section.SectionId, rowChar, c));
                            createdForSection++;
                        }
                    }
                    _logger.LogInformation($"Generados {createdForSection} asientos para sección numerada {section.Name}");
                }
                else
                {
                    for (int i = 1; i <= section.Capacity; i++)
                    {
                        seatsToCreate.Add(new EventSeat(message.EventId, section.SectionId, "GEN", i));
                    }
                    _logger.LogInformation($"Generados {section.Capacity} asientos para sección general {section.Name}");
                }
            }

            if (seatsToCreate.Count > 0)
            {
                _logger.LogInformation($"Guardando {seatsToCreate.Count} asientos en BD...");
                await _repository.AddRangeAsync(seatsToCreate, context.CancellationToken);
                await _repository.SaveChangesAsync(context.CancellationToken);
                _logger.LogInformation($"Se generaron {seatsToCreate.Count} asientos exitosamente en BD.");
            }
            else 
            {
                _logger.LogWarning("No se generaron asientos (seatsToCreate es 0).");
            }
        }
    }
}