using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;

namespace signalRtest
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddAuthentication(options => {
                    options.DefaultScheme = "Cookies";
                })
                .AddCookie("Cookies", options => 
                {
                    options.Cookie.Name = "chatclient-auth";
                    options.AccessDeniedPath = new PathString("/accessdenied");
                    options.Cookie.SameSite = SameSiteMode.Strict;
                });
            services.AddRazorPages();
            services.AddSignalR();
            services.AddSingleton<ConnectionMultiplexer>(sp => 
                ConnectionMultiplexer.Connect("localhost")
            );
            services.AddScoped<ChatRepository>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapPost("login", Auth.LoginHandler);
                endpoints.MapPost("logout", Auth.LogoutHandler);
                endpoints.MapGet("api/participants", ChatHttp.GetConnectedClients);
                endpoints.MapGet("api/broadcast", ChatHttp.GetBroadCast);
                endpoints.MapRazorPages();
                endpoints.MapHub<ChatHub>("/chatHub");
            });
        }
    }
}
