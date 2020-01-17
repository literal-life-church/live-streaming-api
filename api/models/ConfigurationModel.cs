using System;

namespace LiteralLifeChurch.LiveStreamingApi.models
{
    class ConfigurationModel
    {
        public string ClientId { get; set; }

        public string ClientSecret { get; set; }

        public Uri ManagementEndpoint { get; set; }

        public string SubscriptionId { get; set; }

        public string TenantId { get; set; }
    }
}
