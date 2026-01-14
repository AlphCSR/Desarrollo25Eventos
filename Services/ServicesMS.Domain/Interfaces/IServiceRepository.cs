using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ServicesMS.Domain.Entities;

namespace ServicesMS.Domain.Interfaces
{
    public interface IServiceRepository
    {
        Task<ServiceDefinition?> GetDefinitionByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<IEnumerable<ServiceDefinition>> GetDefinitionsByEventAsync(Guid eventId, CancellationToken cancellationToken);
        Task AddBookingAsync(ServiceBooking booking, CancellationToken cancellationToken);
        Task AddDefinitionAsync(ServiceDefinition definition, CancellationToken cancellationToken);
        Task UpdateDefinitionAsync(ServiceDefinition definition, CancellationToken cancellationToken);
        Task<IEnumerable<ServiceBooking>> GetBookingsByBookingIdAsync(Guid bookingId, CancellationToken cancellationToken);
        Task UpdateBookingAsync(ServiceBooking booking, CancellationToken cancellationToken);
        Task SaveChangesAsync(CancellationToken cancellationToken);
    }
}
