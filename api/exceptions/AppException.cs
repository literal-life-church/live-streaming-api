using Newtonsoft.Json;
using System;
using System.Net;

namespace LiteralLifeChurch.LiveStreamingApi.Exceptions
{
    public abstract class AppException : Exception
    {
        [JsonProperty("developerMessage")]
        public string DeveloperMessage { get; set; }

        [JsonProperty("message")]
        public new string Message { get; set; }

        [JsonProperty("status")]
        public HttpStatusCode Status { get; set; }
    }
}
