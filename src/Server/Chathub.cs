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
        public async Task SendMessage(string payload)
        {
            var message = new Message { From = Context.User.Identity.Name, Payload = payload };
            await Clients.All.SendAsync("ReceiveMessage", message);
        }
    }
}