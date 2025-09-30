using Microsoft.AspNetCore.SignalR;

namespace deeplynx.hubs;

public class EventNotificationHub : Hub
{
    public async Task SendNotification(string user, string message)
    {
        await Clients.All.SendAsync("ReceiveNotification", user, message);
    }
}