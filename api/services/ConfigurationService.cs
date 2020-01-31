using LiteralLifeChurch.LiveStreamingApi.models;
using System;

namespace LiteralLifeChurch.LiveStreamingApi.services
{
    class ConfigurationService
    {
        private static readonly string AccountName = "LIVE_STREAMING_API_ACCOUNT_NAME";
        private static readonly string ClientIdName = "LIVE_STREAMING_API_CLIENT_ID";
        private static readonly string ClientSecretName = "LIVE_STREAMING_API_CLIENT_SECRET";
        private static readonly string ResourceGroupName = "LIVE_STREAMING_API_RESOURCE_GROUP";
        private static readonly string SubscriptionIdName = "LIVE_STREAMING_API_SUBSCRIPTION_ID";
        private static readonly string TenantIdName = "LIVE_STREAMING_API_TENANT_ID";

        public ConfigurationModel GetConfiguration()
        {
            return new ConfigurationModel()
            {
                AccountName = Environment.GetEnvironmentVariable(AccountName),
                ClientId = Environment.GetEnvironmentVariable(ClientIdName),
                ClientSecret = Environment.GetEnvironmentVariable(ClientSecretName),
                ManagementEndpoint = new Uri("https://management.azure.com/"),
                ResourceGroup = Environment.GetEnvironmentVariable(ResourceGroupName),
                SubscriptionId = Environment.GetEnvironmentVariable(SubscriptionIdName),
                TenantId = Environment.GetEnvironmentVariable(TenantIdName)
            };
        }
    }
}
