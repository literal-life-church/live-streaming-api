using Newtonsoft.Json;
using System.Net;

namespace LiteralLifeChurch.LiveStreamingApi.Models.Output
{
    public class ErrorModel : IOutputModel
    {
        [JsonProperty("developerMessage")]
        public string DeveloperMessage { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("status")]
        public HttpStatusCode Status { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }
}
