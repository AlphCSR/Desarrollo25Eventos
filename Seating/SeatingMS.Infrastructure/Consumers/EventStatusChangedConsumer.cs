using MassTransit;
using Microsoft.Extensions.Logging;
using EventsMS.Shared.Events;
using SeatingMS.Domain.Interfaces;
using SeatingMS.Domain.Entities;
using System.Threading.Tasks;
using System.Linq;
using EventsMS.Shared.Enums;

namespace SeatingMS.Infrastructure.Consumers
{
    public class EventStatusChangedConsumer : IConsumer<EventStatusChangedEvent>
    {
        private readonly IEventSeatRepository _repository;
        private readonly ILogger<EventStatusChangedConsumer> _logger;

        public EventStatusChangedConsumer(IEventSeatRepository repository, ILogger<EventStatusChangedConsumer> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<EventStatusChangedEvent> context)
        {
            var msg = context.Message;

            if (msg.NewStatus != EventStatus.Published && msg.NewStatus != EventStatus.Live)
                return;

            if (msg.Sections == null || !msg.Sections.Any())
            {
                _logger.LogWarning($"[SeatingMS] Recibido cambio de estado para {msg.EventId} a {msg.NewStatus}, pero no trae secciones para validar asientos.");
                return;
            }

            var existingSeats = await _repository.GetByEventIdAsync(msg.EventId, context.CancellationToken);
            if (existingSeats.Any())
            {
                _logger.LogInformation($"[SeatingMS] El evento {msg.EventId} ya tiene {existingSeats.Count()} asientos. Omitiendo generación.");
                return;
            }

            _logger.LogInformation($"[SeatingMS] El evento {msg.EventId} se publicó/inició y NO tiene asientos. Generando ahora...");

            var seatsToCreate = new System.Collections.Generic.List<EventSeat>();

            foreach (var section in msg.Sections)
            {
                if (section.IsNumbered)
                {
                    int rows = (int)System.Math.Ceiling((double)section.Capacity / 10);
                    int createdForSection = 0;
                    for (int r = 1; r <= rows; r++)
                    {
                        for (int c = 1; c <= 10; c++)
                        {
                            if (createdForSection >= section.Capacity) break;
                            string rowChar = ((char)('A' + r - 1)).ToString();
                            seatsToCreate.Add(new EventSeat(msg.EventId, section.SectionId, rowChar, c));
                            createdForSection++;
                        }
                    }
                }
                else
                {
                    for (int i = 1; i <= section.Capacity; i++)
                    {
                        seatsToCreate.Add(new EventSeat(msg.EventId, section.SectionId, "GEN", i));
                    }
                }
            }

            if (seatsToCreate.Count > 0)
            {
                await _repository.AddRangeAsync(seatsToCreate, context.CancellationToken);
                await _repository.SaveChangesAsync(context.CancellationToken);
                _logger.LogInformation($"[SeatingMS] Se generaron {seatsToCreate.Count} asientos (Self-Healing) para el evento {msg.EventId}.");
            }
        }
    }
}
