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

        public ChatRepository(
            ConnectionMultiplexer redis
        )
        {
            Redis = redis;
        }

        public void AddUsername(
            string username,
            string connectionId)
        {
            Db.SetAdd(KeyNames.UsersKey, username);
            Db.StringSet(KeyNames.ConnectionId(username), connectionId);
        }

        public void RemoveUserName(string username)
        {
            Db.SetRemove(KeyNames.UsersKey, username);
            Db.KeyDelete(KeyNames.ConnectionId(username));
        }

        public string GetConnectionId(string username) => Db.StringGet(KeyNames.ConnectionId(username));
        public IEnumerable<string> GetUsers() => Db.SetMembers(KeyNames.UsersKey).Select(x => x.ToString());

        public bool UserNameTaken(string username) => Db.SetContains(KeyNames.UsersKey, username);

        public void Broadcast(ChattMessage m)
        {
            var json = JsonSerializer.Serialize(m);
            Db.ListLeftPush(KeyNames.BroadcastMessagesKey, json);
            Db.ListTrim(KeyNames.BroadcastMessagesKey, 0, 32);
        }

        public IEnumerable<string> GetBroadcast() => Db.ListRange(KeyNames.BroadcastMessagesKey, 0, 99).Select(x => x.ToString());

        public IEnumerable<string> GetChannel(string cId) => Db.ListRange(KeyNames.ChannelKey(cId)).Select(x => x.ToString());

        public void RecordChannel(string channelId, ChattMessage m)
        {
            var members = channelId.Split(".");
            Db.SetAdd(KeyNames.OpenChannel(members[0]), channelId);
            Db.SetAdd(KeyNames.OpenChannel(members[1]), channelId);
            var json = JsonSerializer.Serialize(m);
            Db.ListLeftPush(KeyNames.ChannelKey(channelId), json);
        }

        public string[] OpenChannels(string username)
        {
            var r = Db.SetMembers(KeyNames.OpenChannel(username));
            return r.Select(x => x.ToString()).ToArray();
        }

        public string[] ClosedChannels(string username)
        {
            var r = Db.SetMembers(KeyNames.ClosedChannel(username));
            return r.Select(x => x.ToString()).ToArray();
        }

        public void CloseChannel(string cId)
        {
            var members = cId.Split(".");
            Db.SetRemove(KeyNames.OpenChannel(members[0]), cId);
            Db.SetRemove(KeyNames.OpenChannel(members[1]), cId);
            Db.SetAdd(KeyNames.ClosedChannel(members[0]), cId);
            Db.SetAdd(KeyNames.ClosedChannel(members[1]), cId);
        }
    }
}