using System.Collections.Generic;

namespace LiteralLifeChurch.LiveStreamingApi.models.input
{
    public class InputRequestModel : IInputModel
    {
        public List<string> LiveEvents { get; set; }

        public string StreamingEndpoint { get; set; }
    }
}
