using LiteralLifeChurch.LiveStreamingApi.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

namespace LiteralLifeChurch.LiveStreamingApi.Models.Output
{
    public class StatusChangeOutputModel : IOutputModel
    {
        [JsonProperty("changes")]
        public Diff Changes { get; set; }

        [JsonProperty("status")]
        public StatusOutputModel Status { get; set; }

        public class Diff
        {
            [JsonProperty("events")]
            public List<Resource> LiveEvents { get; set; }

            [JsonProperty("endpoint")]
            public Resource StreamingEndpoint { get; set; }

            public class Resource
            {
                [JsonProperty("name")]
                public string Name { get; set; }

                [JsonConverter(typeof(StringEnumConverter))]
                [JsonProperty("newStatus")]
                public ResourceStatusEnum NewStatus { get; set; }

                [JsonConverter(typeof(StringEnumConverter))]
                [JsonProperty("oldStatus")]
                public ResourceStatusEnum OldStatus { get; set; }
            }
        }
    }
}
