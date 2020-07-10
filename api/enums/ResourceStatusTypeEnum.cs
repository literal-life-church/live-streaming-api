using System.Runtime.Serialization;

namespace LiteralLifeChurch.LiveStreamingApi.Enums
{
    public enum ResourceStatusTypeEnum
    {
        [EnumMember(Value = "stable")]
        Stable,

        [EnumMember(Value = "transient")]
        Transient
    }
}
