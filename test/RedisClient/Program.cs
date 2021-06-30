using System;
using StackExchange.Redis;

namespace RedisClient
{
    class Program
    {
        static ConnectionMultiplexer Redis { get; } = ConnectionMultiplexer.Connect("localhost");
        const string Key  = "TestSet";
        static void Main(string[] args)
        {
            var members = new [] { "Alpha", "Beta", "Gamma" };
            var db = Redis.GetDatabase();
            foreach (var s in members)
            {
                db.SetAdd(Key, s);
            }

            var r1 = db.SetMembers(Key);
            Console.WriteLine($"Expected: {string.Join(", ", members)}");
            Console.WriteLine($"Actual:   {string.Join(", ", r1)}");

            Console.WriteLine("Remove Beta");
            db.SetRemove(Key, "Beta");

            var r2 = db.SetMembers(Key);
            Console.WriteLine($"{string.Join(", ", r2)}");
        }
    }
}
