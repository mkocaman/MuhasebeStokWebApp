using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace MuhasebeStokWebApp.Hubs
{
    public class NotificationHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");
                await Clients.Caller.SendAsync("ReceiveNotification", "Bağlantı başarılı", "success");
            }
            
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"User_{userId}");
            }
            
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendNotification(string message, string type = "info")
        {
            await Clients.All.SendAsync("ReceiveNotification", message, type);
        }

        public async Task SendNotificationToUser(string userId, string message, string type = "info")
        {
            await Clients.User(userId).SendAsync("ReceiveNotification", message, type);
        }

        public async Task SendNotificationToGroup(string groupName, string message, string type = "info")
        {
            await Clients.Group(groupName).SendAsync("ReceiveNotification", message, type);
        }

        public async Task JoinGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            await Clients.Caller.SendAsync("ReceiveNotification", $"{groupName} grubuna katıldınız", "info");
        }

        public async Task LeaveGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            await Clients.Caller.SendAsync("ReceiveNotification", $"{groupName} grubundan ayrıldınız", "info");
        }
    }
} 