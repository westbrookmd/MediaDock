using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace MediaDockBlazorServer.Data
{
    public class MediaControlsHub : Hub
    {
        public const string HubUrl = "/media";

        // trusts the client to send the proper username
        // TODO: fix this - not secure
        public async Task Broadcast(string username, string message)
        {
            await Clients.All.SendAsync("Broadcast", username, message);
        }

        public override Task OnConnectedAsync()
        {
            Console.WriteLine($"{Context.ConnectionId} connected");
            return base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception e)
        {
            Console.WriteLine($"Disconnected {e?.Message} {Context.ConnectionId}");
            await base.OnDisconnectedAsync(e);
        }
    }
}
