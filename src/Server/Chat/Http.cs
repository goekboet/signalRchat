using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace signalRtest
{
    public static class ChatHttp
    {
        public static async Task GetConnectedClients(
            HttpContext ctx)
        {
            var repo = ctx.RequestServices.GetRequiredService<ChatRepository>();
            var clients = repo.GetUsers();

            await ctx.Response.WriteAsJsonAsync(clients);
        }

        public static async Task GetBroadCast(
            HttpContext ctx)
        {
            var repo = ctx.RequestServices.GetRequiredService<ChatRepository>();
            var broadcast = repo.GetBroadcast().Select(x => JsonSerializer.Deserialize<BroadcastMessage>(x));
            await ctx.Response.WriteAsJsonAsync(broadcast);
        }

        public static async Task OpenChannels(
            HttpContext ctx)
        {
            var repo = ctx.RequestServices.GetRequiredService<ChatRepository>();
            var username = ctx.User.Identity.Name;
            var openChannels = repo.OpenChannels(username);

            await ctx.Response.WriteAsJsonAsync(openChannels);
        }

        public static async Task ClosedChannels(
            HttpContext ctx)
        {
            var repo = ctx.RequestServices.GetRequiredService<ChatRepository>();
            var username = ctx.User.Identity.Name;
            var closed = repo.ClosedChannels(username);

            await ctx.Response.WriteAsJsonAsync(closed);
        }

    }
    
}