using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace signalRtest
{
    public record LoginModel
    {
        public string Name {get; init; }
    }

    public static class Auth
    {
        public static async Task LoginHandler(
            HttpContext ctx)
        {
            var form = await ctx.Request.ReadFormAsync();
            var username = form["Username"].ToString();

            var name = new Claim(ClaimTypes.Name, username);
            var identity = new ClaimsIdentity(new [] { name }, "Local" );
            var principal = new ClaimsPrincipal(identity);
            
            await ctx.SignInAsync(principal);
            ctx.Response.Redirect("/");
        }

        public static async Task LogoutHandler(
            HttpContext ctx)
        {
            await ctx.SignOutAsync();
            ctx.Response.Redirect("/");
        }
    }
    
}