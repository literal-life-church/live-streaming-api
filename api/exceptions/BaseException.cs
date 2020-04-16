using Newtonsoft.Json;
using System;

namespace LiteralLifeChurch.LiveStreamingApi.exceptions
{
    public abstract class BaseException : Exception
    {
        [JsonProperty("developerMessage")]
        public string DeveloperMessage { get; set; }

        [JsonProperty("message")]
        public new string Message { get; set; }

        [JsonProperty("status")]
        public int Status { get; set; }
    }
}
