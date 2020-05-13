using System.Runtime.Serialization;

namespace LiteralLifeChurch.LiveStreamingApi.enums
{
    public enum ActionEnum
    {
        [EnumMember(Value = "start")]
        Start,

        [EnumMember(Value = "stop")]
        Stop
    }
}
