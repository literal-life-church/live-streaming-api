using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LiteralLifeChurch.LiveStreamingApi.models.output
{
    public class StatusOutputModel : IOutputModel
    {
        [JsonProperty("endpoint")]
        public Resource Endpoint { get; set; }

        [JsonProperty("events")]
        public List<Resource> Events { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("summary")]
        public Status Summary { get; set; }

        public class Resource
        {
            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonConverter(typeof(StringEnumConverter))]
            [JsonProperty("status")]
            public Status Status { get; set; }
        }

        public enum Status
        {
            [EnumMember(Value = "deleting")]
            Deleting,

            [EnumMember(Value = "error")]
            Error,

            [EnumMember(Value = "running")]
            Running,

            [EnumMember(Value = "scaling")]
            Scaling,

            [EnumMember(Value = "starting")]
            Starting,

            [EnumMember(Value = "stopped")]
            Stopped,

            [EnumMember(Value = "stopping")]
            Stopping
        }
    }
}
