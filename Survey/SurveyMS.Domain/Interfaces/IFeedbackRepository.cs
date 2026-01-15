
using SurveyMS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SurveyMS.Domain.Interfaces
{
    public interface IFeedbackRepository
    {
        Task AddAsync(Feedback feedback, CancellationToken cancellationToken = default);
        Task<IEnumerable<Feedback>> GetByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default);
        Task<bool> HasUserRatedBookingAsync(Guid userId, Guid bookingId, CancellationToken cancellationToken = default);
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
