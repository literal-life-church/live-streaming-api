using System;

namespace LiteralLifeChurch.LiveStreamingApi.models
{
    public class ConfigurationModel
    {
        public string AccountName { get; set; }

        public string ClientId { get; set; }

        public string ClientSecret { get; set; }

        public Uri ManagementEndpoint { get; set; }

        public string ResourceGroup { get; set; }

        public string SubscriptionId { get; set; }

        public string TenantId { get; set; }
    }
}
