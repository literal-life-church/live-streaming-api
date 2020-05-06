using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LiteralLifeChurch.LiveStreamingApi.models.output
{
    public class LocatorsOutputModel : IOutputModel
    {
        [JsonProperty("events")]
        public List<LiveEvent> LiveEvents { get; set; }

        [JsonProperty("isAllLive")]
        public bool IsAllLive { get; set; }

        [JsonProperty("isAnyLive")]
        public bool IsAnyLive { get; set; }

        public class LiveEvent
        {
            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("isLive")]
            public bool IsLive { get; set; }

            [JsonProperty("locators")]
            public List<Locator> Locators { get; set; }

            public class Locator
            {
                [JsonConverter(typeof(StringEnumConverter))]
                [JsonProperty("type")]
                public LocatorType Type { get; set; }

                [JsonProperty("url")]
                public Uri Url { get; set; }

                public enum LocatorType
                {
                    [EnumMember(Value = "dash")]
                    Dash,

                    [EnumMember(Value = "hls")]
                    Hls,

                    [EnumMember(Value = "smooth")]
                    Smooth
                }
            }
        }
    }
}
