using LiteralLifeChurch.LiveStreamingApi.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

namespace LiteralLifeChurch.LiveStreamingApi.Models.Output
{
    public class StatusOutputModel : IOutputModel
    {
        [JsonProperty("events")]
        public List<Resource> LiveEvents { get; set; }

        [JsonProperty("endpoint")]
        public Resource StreamingEndpoint { get; set; }

        [JsonProperty("summary")]
        public Status Summary { get; set; }

        public class Resource
        {
            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("status")]
            public Status Status { get; set; }
        }

        public class Status
        {
            [JsonConverter(typeof(StringEnumConverter))]
            [JsonProperty("name")]
            public ResourceStatusEnum Name { get; set; }


            [JsonConverter(typeof(StringEnumConverter))]
            [JsonProperty("type")]
            public ResourceStatusTypeEnum Type { get; set; }
        }
    }
}
