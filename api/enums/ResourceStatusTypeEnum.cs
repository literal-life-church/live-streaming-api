using System.Runtime.Serialization;

namespace LiteralLifeChurch.LiveStreamingApi.enums
{
    public enum ResourceStatusTypeEnum
    {
        [EnumMember(Value = "stable")]
        Stable,

        [EnumMember(Value = "transient")]
        Transient
    }
}
