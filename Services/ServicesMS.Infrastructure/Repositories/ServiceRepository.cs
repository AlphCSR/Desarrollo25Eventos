using Microsoft.EntityFrameworkCore;
using ServicesMS.Domain.Entities;
using ServicesMS.Domain.Interfaces;
using ServicesMS.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ServicesMS.Infrastructure.Repositories
{
    public class ServiceRepository : IServiceRepository
    {
        private readonly ServicesDbContext _context;

        public ServiceRepository(ServicesDbContext context)
        {
            _context = context;
        }

        public async Task<ServiceDefinition?> GetDefinitionByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _context.ServiceDefinitions.FindAsync(new object[] { id }, cancellationToken);
        }

        public async Task<IEnumerable<ServiceDefinition>> GetDefinitionsByEventAsync(Guid eventId, CancellationToken cancellationToken)
        {
            return await _context.ServiceDefinitions
                .Where(x => x.EventId == eventId)
                .ToListAsync(cancellationToken);
        }

        public async Task AddBookingAsync(ServiceBooking booking, CancellationToken cancellationToken)
        {
            await _context.ServiceBookings.AddAsync(booking, cancellationToken);
        }

        public async Task AddDefinitionAsync(ServiceDefinition definition, CancellationToken cancellationToken)
        {
            await _context.ServiceDefinitions.AddAsync(definition, cancellationToken);
        }

        public async Task UpdateDefinitionAsync(ServiceDefinition definition, CancellationToken cancellationToken)
        {
            _context.ServiceDefinitions.Update(definition);
            await Task.CompletedTask;
        }
        
        public async Task<IEnumerable<ServiceBooking>> GetBookingsByBookingIdAsync(Guid bookingId, CancellationToken cancellationToken)
        {
            return await _context.ServiceBookings
                .Where(x => x.BookingId == bookingId)
                .ToListAsync(cancellationToken);
        }

        public async Task UpdateBookingAsync(ServiceBooking booking, CancellationToken cancellationToken)
        {
            _context.ServiceBookings.Update(booking);
            await Task.CompletedTask;
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
