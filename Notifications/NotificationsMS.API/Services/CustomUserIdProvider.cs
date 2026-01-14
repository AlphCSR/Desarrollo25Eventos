using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace NotificationsMS.API.Services
{
    public class CustomUserIdProvider : IUserIdProvider
    {
        public string? GetUserId(HubConnectionContext connection)
        {
            return connection.User?.FindFirst("email")?.Value 
                   ?? connection.User?.FindFirst("preferred_username")?.Value 
                   ?? connection.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
}
