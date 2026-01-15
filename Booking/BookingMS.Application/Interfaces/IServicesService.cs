using BookingMS.Application.DTOs;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BookingMS.Application.Interfaces
{
    public interface IServicesService
    {
        Task<bool> BookServiceAsync(Guid serviceId, Guid userId, Guid bookingId, int quantity, CancellationToken cancellationToken);
        Task<ServiceDetailDto> GetServiceDetailAsync(Guid serviceId, CancellationToken cancellationToken);
    }
}
