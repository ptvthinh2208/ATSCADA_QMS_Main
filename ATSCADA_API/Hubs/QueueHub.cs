using Microsoft.AspNetCore.SignalR;
using MySqlX.XDevAPI;

namespace ATSCADA_API.Hubs
{
    public class QueueHub : Hub
    {
        public async Task SendMessage()
        {
            await Clients.All.SendAsync("ReceiveMessage");
        }
    }
}
