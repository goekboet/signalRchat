using System;
using System.Linq;
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
            Repo.AddUsername(username, Context.ConnectionId);
            var json = JsonSerializer.Serialize(msg);
            await Clients.Others.SendAsync("ClientConnected", username);
            await Clients.All.SendAsync("NewChatmessage", msg);
            
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
            await Clients.All.SendAsync("NewChatmessage", msg);

            await base.OnDisconnectedAsync(exception);
        }

        static string channelId(string subj, string obj) => 
            obj == "" 
                ? "" 
                : string.Join(".", 
                    new [] { subj, obj}
                    .OrderBy(x => x)
                );

        public async Task SendMessage(string payload, string counterPart = "")
        {
            var subject = Context.User?.Identity.Name ?? "no name";
            var cId = channelId(subject, counterPart);
            var msg = new BroadcastMessage
            {
                UnixMsTimestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                Channel = cId,
                Sender = subject,
                Payload = payload
            };
            var json = JsonSerializer.Serialize(msg);
            if (counterPart == "") 
            {
                Repo.Broadcast(json);
                await Clients.All.SendAsync("NewChatmessage", msg);
            }
            else
            {
                Repo.RecordChannel(subject, cId, msg);
                await Clients.Group(cId).SendAsync("NewChatmessage", msg);
            }
        }

        public async Task OpenChannel(string counterpart, string greet)
        {
            var subject = Context.User?.Identity.Name ?? "no name";
            var cId = channelId(subject, counterpart);
            
            var payload = new BroadcastMessage 
            { 
                UnixMsTimestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                Channel = cId,
                Sender = subject,
                Payload = greet  
            };

            var counterPartConnectionId = Repo.GetConnectionId(counterpart);
            await Groups.AddToGroupAsync(Context.ConnectionId, cId);
            await Groups.AddToGroupAsync(counterPartConnectionId, cId);
            Repo.RecordChannel(subject, cId, payload);
            await Clients.Group(cId).SendAsync("ChannelOpened", cId);
            await Clients.Group(cId).SendAsync("ChannelMessage", payload);
        }

        public async Task CloseChannel(string cId)
        {
            var subject = Context.User?.Identity.Name ?? "no name";
            Repo.CloseChannel(subject, cId);
            await Clients.Group(cId).SendAsync("ChannelClosed", cId);
            var counterpart = cId.Split(".").Single(x => x != subject);
            var counterPartConnectionId = Repo.GetConnectionId(counterpart);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, cId);
            await Groups.RemoveFromGroupAsync(counterPartConnectionId, cId);
        }
    }
}