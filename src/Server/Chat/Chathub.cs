using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace signalRtest
{
    [Authorize]
    public class ChatHub : Hub
    {
        ChatRepository Repo { get; }

        public ChatHub(
            ChatRepository repo
        )
        {
            Repo = repo;
        }

        public override async Task OnConnectedAsync()
        {
            var username = Context.User?.Identity.Name;
            if (username == null) return;

            var ts = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            var msg = new ChattMessage
            {
                UnixMsTimestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                Sender = username,
                Payload = "Came online."
            };
            Repo.AddUsername(username, Context.ConnectionId);

            await Clients.Others.SendAsync("ClientConnected", username);
            await Clients.All.SendAsync("NewChatmessage", msg);

            var groups = Repo.OpenChannels(username);
            foreach (var g in groups)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, g);
                var groupAnnounce = new ChattMessage
                {
                    UnixMsTimestamp = ts,
                    Channel = g,
                    Sender = username,
                    Payload = "Came online."
                };

                Repo.RecordChannel(g, groupAnnounce);
                await Clients.Group(g).SendAsync("NewChatmessage", groupAnnounce);
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(System.Exception exception)
        {
            var username = Context.User?.Identity.Name;
            if (username == null) return;

            var ts = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            var broadCastAnnounce = new ChattMessage
            {
                UnixMsTimestamp = ts,
                Sender = username,
                Payload = "Went offline."
            };
            Repo.RemoveUserName(username);
            Repo.Broadcast(broadCastAnnounce);
            await Clients.Others.SendAsync("ClientDisconnected", username);
            await Clients.All.SendAsync("NewChatmessage", broadCastAnnounce);

            var groups = Repo.OpenChannels(username);
            foreach (var g in groups)
            {
                var groupAnnounce = new ChattMessage
                {
                    UnixMsTimestamp = ts,
                    Channel = g,
                    Sender = username,
                    Payload = "Went offline."
                };

                Repo.RecordChannel(g, groupAnnounce);
                await Clients.Group(g).SendAsync("NewChatmessage", groupAnnounce);
            }

            await base.OnDisconnectedAsync(exception);
        }

        static string channelId(string subj, string obj) =>
            obj == ""
                ? ""
                : string.Join(".",
                    new[] { subj, obj }
                    .OrderBy(x => x)
                );

        public async Task SendMessage(string payload, string cId = "")
        {
            var subject = Context.User?.Identity.Name ?? "no name";
            var msg = new ChattMessage
            {
                UnixMsTimestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                Channel = cId,
                Sender = subject,
                Payload = payload
            };

            if (cId == "")
            {
                Repo.Broadcast(msg);
                await Clients.All.SendAsync("NewChatmessage", msg);
            }
            else
            {
                Repo.RecordChannel(cId, msg);
                await Clients.Group(cId).SendAsync("NewChatmessage", msg);
            }
        }

        public async Task OpenChannel(string counterpart, string greet)
        {
            var subject = Context.User?.Identity.Name ?? "no name";
            var cId = channelId(subject, counterpart);

            var payload = new ChattMessage
            {
                UnixMsTimestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                Channel = cId,
                Sender = subject,
                Payload = greet
            };

            var counterPartConnectionId = Repo.GetConnectionId(counterpart);
            await Groups.AddToGroupAsync(Context.ConnectionId, cId);
            await Groups.AddToGroupAsync(counterPartConnectionId, cId);

            Repo.RecordChannel(cId, payload);
            await Clients.Group(cId).SendAsync("ChannelOpened", cId);
            await Clients.Group(cId).SendAsync("NewChatmessage", payload);
        }

        public async Task CloseChannel(string cId)
        {
            var subject = Context.User?.Identity.Name ?? "no name";
            Repo.CloseChannel(cId);
            await Clients.Group(cId).SendAsync("ChannelClosed", cId);
            var counterpart = cId.Split(".").Single(x => x != subject);
            var counterPartConnectionId = Repo.GetConnectionId(counterpart);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, cId);
            await Groups.RemoveFromGroupAsync(counterPartConnectionId, cId);
        }
    }
}