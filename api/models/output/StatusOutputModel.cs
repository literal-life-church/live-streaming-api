using LiteralLifeChurch.LiveStreamingApi.enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

namespace LiteralLifeChurch.LiveStreamingApi.models.output
{
    public class StatusOutputModel : IOutputModel
    {
        [JsonProperty("events")]
        public List<Resource> LiveEvents { get; set; }

        [JsonProperty("endpoint")]
        public Resource StreamingEndpoint { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("summary")]
        public ResourceStatusEnum Summary { get; set; }

        public class Resource
        {
            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonConverter(typeof(StringEnumConverter))]
            [JsonProperty("status")]
            public ResourceStatusEnum Status { get; set; }
        }
    }
}
