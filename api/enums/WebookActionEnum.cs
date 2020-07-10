using System.Runtime.Serialization;

namespace LiteralLifeChurch.LiveStreamingApi.Enums
{
    public enum ActionEnum
    {
        [EnumMember(Value = "start")]
        Start,

        [EnumMember(Value = "stop")]
        Stop
    }
}
