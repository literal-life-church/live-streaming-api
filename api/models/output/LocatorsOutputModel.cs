using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LiteralLifeChurch.LiveStreamingApi.models.output
{
    public class LocatorsOutputModel
    {
        [JsonProperty("liveEventName")]
        public string LiveEventName { get; set; }

        [JsonProperty("locators")]
        public List<Locator> Locators { get; set; }

        public enum LocatorType
        {
            [EnumMember(Value = "dash")]
            DASH,

            [EnumMember(Value = "hls")]
            HLS,

            [EnumMember(Value = "smooth")]
            Smooth
        }

        public class Locator
        {
            [JsonConverter(typeof(StringEnumConverter))]
            [JsonProperty("type")]
            public LocatorType Type { get; set; }

            [JsonProperty("url")]
            public Uri Url { get; set; }
        }
    }
}
