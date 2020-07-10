using System.Collections.Generic;

namespace LiteralLifeChurch.LiveStreamingApi.Models.Input
{
    public class LocatorsInputModel : IInputModel
    {
        public List<string> LiveEvents { get; set; }

        public string StreamingEndpoint { get; set; }
    }
}
