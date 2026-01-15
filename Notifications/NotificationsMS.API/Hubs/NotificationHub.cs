using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using System;

namespace NotificationsMS.Hubs
{
    [Authorize] 
    public class NotificationHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }

        public async Task JoinEventRoom(string eventId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, eventId);
        }

        public async Task SendChatMessage(string eventId, string message)
        {
            var userName = Context.User.Identity?.Name ?? "Anónimo";
            await Clients.Group(eventId).SendAsync("ReceiveChatMessage", new {
                User = userName,
                Message = message,
                Timestamp = DateTime.UtcNow
            });
        }
    }
}