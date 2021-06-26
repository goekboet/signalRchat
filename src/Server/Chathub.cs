using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace signalRtest
{
    public record Message
    {
        public string From { get; init; }
        public string Payload { get; init; }
    }
    public class ChatHub : Hub
    {
        public async Task SendMessage(Message m)
        {
            await Clients.All.SendAsync("ReceiveMessage", m);
        }
    }
}