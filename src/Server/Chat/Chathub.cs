using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using StackExchange.Redis;

namespace signalRtest
{
    [Authorize]
    public class ChatHub : Hub
    {
        ChatRepository Repo {get;}
        
        public ChatHub(
            ChatRepository repo
        )
        {
            Repo = repo;
        }

        public override async Task OnConnectedAsync()
        {
            var username = Context.User?.Identity.Name ?? "no name";
            var msg = new BroadcastMessage
            {
                UnixMsTimestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                Sender = username,
                Payload = "Entered the chat"
            };
            Repo.RemoveUserName(username);
            var json = JsonSerializer.Serialize(msg);
            await Clients.Others.SendAsync("ClientConnected", username);
            await Clients.All.SendAsync("ReceiveMessage", msg);
            
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(System.Exception exception)
        {
            var username = Context.User?.Identity.Name ?? "no name";
            var msg = new BroadcastMessage
            {
                UnixMsTimestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                Sender = username,
                Payload = "Left the chat"
            };
            Repo.RemoveUserName(username);
            var json = JsonSerializer.Serialize(msg);
            Repo.Broadcast(json);
            await Clients.Others.SendAsync("ClientDisconnected", username);
            await Clients.All.SendAsync("ReceiveMessage", msg);

            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(string payload)
        {
            var username = Context.User?.Identity.Name ?? "no name";
            var msg = new BroadcastMessage
            {
                UnixMsTimestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                Sender = username,
                Payload = payload
            };
            var json = JsonSerializer.Serialize(msg);
            Repo.Broadcast(json);
            await Clients.All.SendAsync("ReceiveMessage", msg);
        }
    }
}