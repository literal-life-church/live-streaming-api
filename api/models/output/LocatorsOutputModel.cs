using Newtonsoft.Json;
using System;
using System.Collections.Generic;

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
            DASH,
            HLS,
            Smooth
        }

        public class Locator
        {
            [JsonProperty("type")]
            public LocatorType Type { get; set; }

            [JsonProperty("url")]
            public Uri url;
        }
    }
}
