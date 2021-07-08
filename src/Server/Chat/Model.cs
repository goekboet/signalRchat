using System.Text.Json.Serialization;

namespace signalRtest
{
    public record ChattMessage
    {
        [JsonPropertyName("unixMsTimestamp")]
        public long UnixMsTimestamp { get; init; }

        [JsonPropertyName("channel")]
        public string Channel { get; init; } = "";

        [JsonPropertyName("sender")]
        public string Sender { get; init; }

        [JsonPropertyName("payload")]
        public string Payload { get; init; }
    }

    public static class KeyNames
    {
        /// <summary>
        /// The set uf usernames currently online
        /// </summary>
        public const string UsersKey = "chat.users";
        /// <summary>
        /// A list of json-serialized messages. Newest first. 
        /// </summary>
        public const string BroadcastMessagesKey = "chat.broadcast";

        /// <summary>
        /// Stores the SignalR connectionId-string for the username. 
        /// This is for grouping. 
        /// You need the connectionId of all users you want to put in a group. 
        /// </summary>
        /// <param name="username">The username is part of the key</param>
        /// <returns></returns>
        public static string ConnectionId(string username) => $"chat.username.{username}.connectionId";
        /// <summary>
        /// Stores a list of messages on json-format for a channel. Newest first.
        /// </summary>
        /// <param name="channelId">the ch</param>
        /// <returns></returns>
        public static string ChannelKey(string channelId) => $"chat.channels.{channelId}";
        /// <summary>
        /// The set of open channels for a given user
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public static string OpenChannel(string username) => $"chat.username.{username}";
        /// <summary>
        /// The set of closed channels. This is a bit poorly thought out. Can a channel be closed and opened at the same time? 
        /// Are we thinking of named conversations or the fact that both listeners are online?
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public static string ClosedChannel(string username) => $"chat.username.{username}.archive";
    }
}