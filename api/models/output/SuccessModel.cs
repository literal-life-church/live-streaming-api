using Newtonsoft.Json;

namespace LiteralLifeChurch.LiveStreamingApi.models.output
{
    public class SuccessModel : IOutputModel
    {
        [JsonProperty("message")]
        public string Message { get; set; }
    }
}
