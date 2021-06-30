using System.Text.Json.Serialization;

namespace signalRtest
{
    public record BroadcastMessage
    {
        [JsonPropertyName("unixMsTimestamp")]
        public long UnixMsTimestamp { get; init; }
        [JsonPropertyName("sender")]
        public string Sender { get; init; }
        [JsonPropertyName("payload")]
        public string Payload { get; init; }
    }
}