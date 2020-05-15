using LiteralLifeChurch.LiveStreamingApi.models.bootstrapping;
using Sentry;
using Sentry.Protocol;
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
        private static readonly string WebhookStartFailure = "LIVE_STREAMING_API_WEBHOOK_START_FAILURE";
        private static readonly string WebhookStartSuccess = "LIVE_STREAMING_API_WEBHOOK_START_SUCCESS";
        private static readonly string WebhookStopFailure = "LIVE_STREAMING_API_WEBHOOK_STOP_FAILURE";
        private static readonly string WebhookStopSuccess = "LIVE_STREAMING_API_WEBHOOK_STOP_SUCCESS";

        public ConfigurationModel GetConfiguration()
        {
            ConfigurationModel model = new ConfigurationModel
            {
                AccountName = Environment.GetEnvironmentVariable(AccountName),
                ClientId = Environment.GetEnvironmentVariable(ClientIdName),
                ClientSecret = Environment.GetEnvironmentVariable(ClientSecretName),
                ManagementEndpoint = new Uri("https://management.azure.com/"),
                ResourceGroup = Environment.GetEnvironmentVariable(ResourceGroupName),
                SubscriptionId = Environment.GetEnvironmentVariable(SubscriptionIdName),
                TenantId = Environment.GetEnvironmentVariable(TenantIdName),
                WebhookStartFailure = new Uri(Environment.GetEnvironmentVariable(WebhookStartFailure)),
                WebhookStartSuccess = new Uri(Environment.GetEnvironmentVariable(WebhookStartSuccess)),
                WebhookStopFailure = new Uri(Environment.GetEnvironmentVariable(WebhookStopFailure)),
                WebhookStopSuccess = new Uri(Environment.GetEnvironmentVariable(WebhookStopSuccess))
            };

            SentrySdk.AddBreadcrumb(message: "Extracted configuration", category: "bootstrapping", level: BreadcrumbLevel.Info);
            return model;
        }
    }
}
