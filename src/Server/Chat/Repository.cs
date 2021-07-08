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

        static string ConnectionId(string username) => $"chat.username.{username}.connectionId";
        static string ChannelKey(string channelId) => $"chat.channels.{channelId}";
        static string OpenChannel(string username) => $"chat.username.{username}";
        static string ClosedChannel(string username) => $"chat.username.{username}.archive";

        public ChatRepository(
            ConnectionMultiplexer redis
        )
        {
            Redis = redis;
        }

        public void AddUsername(
            string username,
            string connectionId) {
            Db.SetAdd(UsersKey, username);
            Db.StringSet(ConnectionId(username), connectionId);
        }
        
        public void RemoveUserName(string username) 
        {
            Db.SetRemove(UsersKey, username);
            Db.KeyDelete(ConnectionId(username));
        }

        public string GetConnectionId(string username) => Db.StringGet(ConnectionId(username));
        public IEnumerable<string> GetUsers() => Db.SetMembers(UsersKey).Select(x => x.ToString());

        public bool UserNameTaken(string username) => Db.SetContains(UsersKey, username);

        public void Broadcast(BroadcastMessage m)
        {
            var json = JsonSerializer.Serialize(m);
            Db.ListLeftPush(BroadcastMessagesKey, json);
            Db.ListTrim(BroadcastMessagesKey, 0, 32);
        }

        public IEnumerable<string> GetBroadcast() => Db.ListRange(BroadcastMessagesKey, 0, 99).Select(x => x.ToString());

        public IEnumerable<string> GetChannel(string cId) => Db.ListRange(ChannelKey(cId)).Select(x => x.ToString());

        public void RecordChannel(string channelId, BroadcastMessage m)
        {
            var members = channelId.Split(".");
            Db.SetAdd(OpenChannel(members[0]), channelId);
            Db.SetAdd(OpenChannel(members[1]), channelId);
            var json = JsonSerializer.Serialize(m);
            Db.ListLeftPush(ChannelKey(channelId), json);
        }

        public string[] OpenChannels(string username)
        {
            var r = Db.SetMembers(OpenChannel(username));
            return r.Select(x => x.ToString()).ToArray();
        }

        public string[] ClosedChannels(string username)
        {
            var r = Db.SetMembers(ClosedChannel(username));
            return r.Select(x => x.ToString()).ToArray();
        }

        public void CloseChannel(string cId)
        {
            var members = cId.Split(".");
            Db.SetRemove(OpenChannel(members[0]), cId);
            Db.SetRemove(OpenChannel(members[1]), cId);
            Db.SetAdd(ClosedChannel(members[0]), cId);
            Db.SetAdd(ClosedChannel(members[1]), cId);
        }
    }
}