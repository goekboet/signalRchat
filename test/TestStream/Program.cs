using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

namespace TestStream
{
    class Program
    {
        static HttpClient _client = null;
        static HttpClient Http { get {
                if (_client == null) 
                {
                    var handler = new HttpClientHandler();
                    handler.CookieContainer = Cookies;
                    _client = new HttpClient(handler);
                }

                return _client;
            } 
        }

        static HubConnectionBuilder SignalR => new HubConnectionBuilder();

        static CookieContainer Cookies = new CookieContainer();

        const string Host = "https://localhost:5001";
        static async Task  Main(string[] args)
        {
            Http.BaseAddress = new Uri($"{Host}/login");
            
            var formdata = new []
            {
                ("Username", "Testuser")
            }.ToDictionary(x => x.Item1, x => x.Item2);
            
            var login = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = new FormUrlEncodedContent(formdata)
            
            };

            var r = await Http.SendAsync(login);
            
            Console.WriteLine(r.StatusCode);
            if (r.IsSuccessStatusCode)
            {
                var cookies = string.Join("\n", Cookies.GetCookies(new Uri(Host)).Select(x => $"{x.Name}{x.Value}"));
                Console.WriteLine(cookies);

                Console.WriteLine("Connect to SignalR");
                var connection = SignalR.WithUrl(
                    $"{Host}/chatHub", opts => 
                    {
                        opts.Cookies = Cookies;
                    }).Build();
                await connection.StartAsync();
            }
            Console.ReadKey();
            Console.WriteLine("Exit");
        }
    }
}
