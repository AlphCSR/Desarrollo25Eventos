using System;
using System.Threading.Tasks;

namespace NotificationsMS.Domain.Interfaces
{
    public interface INotifier
    {
        Task SendNotificationAsync(string userId, string message);
        Task BroadcastSeatUpdateAsync(object update);
    }
}
