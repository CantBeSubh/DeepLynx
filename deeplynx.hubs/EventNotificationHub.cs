using Microsoft.AspNet.SignalR;

namespace deeplynx.hubs
{
    public class EventNotificationHub : Hub
    {
        public async Task SendNotification(string message)
        {
            await Clients.All.SendAsync("ReceiveNotification", message);
        }
    }
}