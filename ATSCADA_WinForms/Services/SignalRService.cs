using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

namespace ATSCADA_WinForms.Services
{
    public class SignalRService
    {
        private HubConnection _connection;
        public event Action OnMessageReceived;

        public SignalRService(string hubUrl)
        {
            _connection = new HubConnectionBuilder()
                .WithUrl(new Uri(hubUrl))
                .WithAutomaticReconnect()
                .Build();

            _connection.On("ReceiveMessage", () =>
            {
                OnMessageReceived?.Invoke();
            });
        }

        public async Task StartAsync()
        {
            if (_connection.State == HubConnectionState.Disconnected)
            {
                await _connection.StartAsync();
            }
        }

        public async Task StopAsync()
        {
            if (_connection.State != HubConnectionState.Disconnected)
            {
                await _connection.StopAsync();
            }
        }
    }
}
