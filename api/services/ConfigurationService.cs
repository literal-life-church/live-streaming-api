using LiteralLifeChurch.LiveStreamingApi.models;
using System;

namespace LiteralLifeChurch.LiveStreamingApi.services
{
    class ConfigurationService
    {
        private static readonly string ClientIdName = "LIVE_STREAMING_API_CLIENT_ID";
        private static readonly string ClientSecretName = "LIVE_STREAMING_API_CLIENT_SECRET";
        private static readonly string SubscriptionIdName = "LIVE_STREAMING_API_SUBSCRIPTION_ID";
        private static readonly string TenantIdName = "LIVE_STREAMING_API_TENANT_ID";

        public ConfigurationModel GetConfiguration()
        {
            return new ConfigurationModel()
            {
                ClientId = Environment.GetEnvironmentVariable(ClientIdName),
                ClientSecret = Environment.GetEnvironmentVariable(ClientSecretName),
                ManagementEndpoint = new Uri("https://management.azure.com/"),
                SubscriptionId = Environment.GetEnvironmentVariable(SubscriptionIdName),
                TenantId = Environment.GetEnvironmentVariable(TenantIdName)
            };
        }
    }
}
