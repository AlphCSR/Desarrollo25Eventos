using Microsoft.AspNetCore.SignalR;
using NotificationsMS.Domain.Interfaces;
using NotificationsMS.Hubs;
using System;
using System.Threading.Tasks;

namespace NotificationsMS.API.Services
{
    public class SignalRNotifier : INotifier
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public SignalRNotifier(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task SendNotificationAsync(string userId, string message)
        {
            await _hubContext.Clients.User(userId).SendAsync("ReceiveNotification", message);
        }

        public async Task BroadcastSeatUpdateAsync(object update)
        {
            await _hubContext.Clients.All.SendAsync("ReceiveSeatUpdate", update);
        }
    }
}
