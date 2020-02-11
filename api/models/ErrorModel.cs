using Newtonsoft.Json;

namespace LiteralLifeChurch.LiveStreamingApi.models
{
    public class ErrorModel
    {
        [JsonProperty("message")]
        public string Message { get; set; }
    }
}
