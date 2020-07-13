using System;

namespace LiteralLifeChurch.LiveStreamingApi.Models.Bootstrapping
{
    public class ConfigurationModel : IBootstrappingModel
    {
        public string AccountName { get; set; }

        public int ArchiveWindowLength { get; set; }

        public string ClientId { get; set; }

        public string ClientSecret { get; set; }

        public Uri ManagementEndpoint { get; set; }

        public string ResourceGroup { get; set; }

        public string SubscriptionId { get; set; }

        public string TenantId { get; set; }

        public Uri WebhookStartFailure { get; set; }

        public Uri WebhookStartSuccess { get; set; }

        public Uri WebhookStopFailure { get; set; }

        public Uri WebhookStopSuccess { get; set; }
    }
}
