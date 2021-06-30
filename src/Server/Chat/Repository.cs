using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using StackExchange.Redis;

namespace signalRtest
{
    public class ChatRepository
    {
        ConnectionMultiplexer Redis { get; }
        IDatabase Db => Redis.GetDatabase();

        static Random Rng = new Random(); 

        const string UsersKey = "chat.users";
        const string BroadcastMessagesKey = "chat.broadcast";

        public ChatRepository(
            ConnectionMultiplexer redis
        )
        {
            Redis = redis;
        }

        public void AddUsername(string username) => Db.SetAdd(UsersKey, username);
        
        public void RemoveUserName(string username) => Db.SetRemove(UsersKey, username);
        public IEnumerable<string> GetUsers() => Db.SetMembers(UsersKey).Select(x => x.ToString());

        public bool UserNameTaken(string username) => Db.SetContains(UsersKey, username);

        public void Broadcast(string msgJson )
        {
            Db.ListLeftPush(BroadcastMessagesKey, msgJson);
            Db.ListTrim(BroadcastMessagesKey, 0, 32);
        }

        public IEnumerable<string> GetBroadcast() => Db.ListRange(BroadcastMessagesKey, 0, 99).Select(x => x.ToString());

    }
}