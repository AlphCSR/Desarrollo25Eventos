using MassTransit;
using UsersMS.Shared.Events;
using MarketingMS.Domain.Interfaces;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace MarketingMS.Infrastructure.Consumers
{
    public class UserProfileUpdatedConsumer : IConsumer<UserProfileUpdatedEvent>
    {
        private readonly IUserInterestRepository _interestRepository;
        private readonly ILogger<UserProfileUpdatedConsumer> _logger;

        public UserProfileUpdatedConsumer(IUserInterestRepository interestRepository, ILogger<UserProfileUpdatedConsumer> logger)
        {
            _interestRepository = interestRepository;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<UserProfileUpdatedEvent> context)
        {
            var evt = context.Message;
            if (evt.Preferences != null && evt.Preferences.Count > 0)
            {
                await _interestRepository.SetInterestsAsync(evt.UserId, evt.Preferences);
                _logger.LogInformation("Sincronizando {Count} preferences para el usuario {UserId}", evt.Preferences.Count, evt.UserId);
            }
        }
    }
}
