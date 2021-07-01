using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

namespace TestStream
{
    class Program
    {
        static Random Rng {get;} = new Random();
        static HubConnectionBuilder SignalR => new HubConnectionBuilder();

        const string Host = "https://localhost:5001";

        static string[] TestClients { get; } = new []
        {
            "Mikhail Bakunin",
            "Peter Kropotkin",
            "Pierre-Joseph Proudhon",
            "Emma Goldman",
            "Rudolf Rocker",
            "Gustav Landauer",
            "Alexander Berkman",
            "Errico Malatesta",
            "Johann Most",
            "Elisée Reclus"
        };

        static string[] Quotes { get; } = new []
        {
            "You cannot buy the revolution. You cannot make the revolution. You can only be the revolution. It is in your spirit, or it is nowhere.",
            "People have only as much liberty as they have the intelligence to want and the courage to take.",
            "That is what I have always understood to be the essence of anarchism: the conviction that the burden of proof has to be placed on authority, and that it should be dismantled if that burden cannot be met.",
            "Our masters have not heard the people's voice for generations and it is much, much louder than they care to remember.",
            "Ask for work. If they don't give you work, ask for bread. If they do not give you work or bread, then take bread.",
            "To be GOVERNED is to be watched, inspected, spied upon, directed, law-driven, numbered, regulated, enrolled, indoctrinated, preached at, controlled, checked, estimated, valued, censured, commanded, by creatures who have neither the right nor the wisdom nor the virtue to do so. To be GOVERNED is to be at every operation, at every transaction noted, registered, counted, taxed, stamped, measured, numbered, assessed, licensed, authorized, admonished, prevented, forbidden, reformed, corrected, punished. It is, under pretext of public utility, and in the name of the general interest, to be placed under contribution, drilled, fleeced, exploited, monopolized, extorted from, squeezed, hoaxed, robbed; then, at the slightest resistance, the first word of complaint, to be repressed, fined, vilified, harassed, hunted down, abused, clubbed, disarmed, bound, choked, imprisoned, judged, condemned, shot, deported, sacrificed, sold, betrayed; and to crown all, mocked, ridiculed, derided, outraged, dishonored. That is government; that is its justice; that is its morality.",
            "Your pretty empire took so long to build, now, with a snap of history's fingers, down it goes.",
            "Every society has the criminals it deserves.",
            "If you would know who controls you see who you may not criticise.",
            "Authority, when first detecting chaos at its heels, will entertain the vilest schemes to save its orderly facade.",
            "Anarchism is founded on the observation that since few men are wise enough to rule themselves, even fewer are wise enough to rule others.",
            "At one time in the world there were woods that no one owned",
            "The individual cannot bargain with the State. The State recognizes no coinage but power: and it issues the coins itself.",
            "School has become the world religion of a modernized proletariat, and makes futile promises of salvation to the poor of the technological age.",
            "Love is it's own protection.",
            "We have been expropriated from our own language by television, from our songs by reality TV contests, from our flesh by mass pornography, from our city by the police and from our friends by wage-labor."
        };

        static string GetRandomQuote => Quotes[Rng.Next(Quotes.Length)];

        static IObservable<Unit> StartShitposting(HubConnection c)
        {
            var offset = Observable.Timer(TimeSpan.FromMilliseconds(Rng.Next(2000))).Select(_ => Unit.Default);
            var shitposting = Observable
                .Interval(TimeSpan.FromSeconds(10))
                .SelectMany(async _ => {
                    await c.InvokeAsync("SendMessage", GetRandomQuote);
                    return Unit.Default;
                });
            
            return offset.Concat(shitposting);
        }

        static HttpClient GetClient(CookieContainer c)
        {
            var handler = new HttpClientHandler();
            handler.CookieContainer = c;

            return new HttpClient(handler); 
        }

        static async Task<HubConnection> ConnectToChat(string username)
        {
            var cookies = new CookieContainer();
            var http = GetClient(cookies);
            http.BaseAddress = new Uri($"{Host}/login");

            var loginPayload = new [] 
                { 
                    ("Username", username) 
                }
                .ToDictionary(x => x.Item1, x => x.Item2);

            var loginRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = new FormUrlEncodedContent(loginPayload)
            };

            var r = await http.SendAsync(loginRequest);
            r.EnsureSuccessStatusCode();

            var connection = SignalR
                .WithUrl(
                    $"{Host}/chatHub", 
                    opts => 
                    {
                        opts.Cookies = cookies;
                    })
                .Build();

            await connection.StartAsync();

            return connection;
        }

        static void Main(string[] args)
        {
            Observable
                .Interval(TimeSpan.FromSeconds(2))
                .Zip(TestClients.ToObservable(), (ts, client) => client)
                .SelectMany(x => ConnectToChat(x))
                .SelectMany(x => StartShitposting(x))
                .Subscribe();

            Console.ReadKey();
        }
    }
}
