using System.Runtime.Serialization;

namespace LiteralLifeChurch.LiveStreamingApi.enums
{
    public enum ResourceStatusEnum
    {
        [EnumMember(Value = "deleting")]
        Deleting,

        [EnumMember(Value = "error")]
        Error,

        [EnumMember(Value = "running")]
        Running,

        [EnumMember(Value = "scaling")]
        Scaling,

        [EnumMember(Value = "starting")]
        Starting,

        [EnumMember(Value = "stopped")]
        Stopped,

        [EnumMember(Value = "stopping")]
        Stopping
    }
}
