using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LiteralLifeChurch.LiveStreamingApi.models.output
{
    public class LocatorsOutputModel
    {
        [JsonProperty("isLive")]
        public bool IsLive { get; set; }

        [JsonProperty("liveEvents")]
        public List<LiveEvent> LiveEvents { get; set; }

        public class LiveEvent
        {
            [JsonProperty("liveEventName")]
            public string LiveEventName { get; set; }

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
                    DASH,

                    [EnumMember(Value = "hls")]
                    HLS,

                    [EnumMember(Value = "smooth")]
                    Smooth
                }
            }
        }
    }
}
