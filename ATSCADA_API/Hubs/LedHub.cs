using Microsoft.AspNetCore.SignalR;

namespace ATSCADA_API.Hubs
{
    public class LedHub : Hub
    {
        public async Task UpdateLedDisplay(string queueNumber, string counterName)
        {
            await Clients.All.SendAsync("ReceiveLedUpdate", queueNumber, counterName);
        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
}
