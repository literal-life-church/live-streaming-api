using LiteralLifeChurch.LiveStreamingApi.Models.Bootstrapping;
using LiteralLifeChurch.LiveStreamingApi.Services.Common;
using System;

namespace LiteralLifeChurch.LiveStreamingApi.Services
{
    public static class ConfigurationService
    {
        private static readonly string AccountName = "LIVE_STREAMING_API_ACCOUNT_NAME";
        private static readonly string ArchiveWindowLengthName = "LIVE_STREAMING_API_ARCHIVE_WINDOW_LENGTH";
        private static readonly string ClientIdName = "LIVE_STREAMING_API_CLIENT_ID";
        private static readonly string ClientSecretName = "LIVE_STREAMING_API_CLIENT_SECRET";
        private static readonly string ResourceGroupName = "LIVE_STREAMING_API_RESOURCE_GROUP";
        private static readonly string SubscriptionIdName = "LIVE_STREAMING_API_SUBSCRIPTION_ID";
        private static readonly string TenantIdName = "LIVE_STREAMING_API_TENANT_ID";
        private static readonly string WebhookStartFailure = "LIVE_STREAMING_API_WEBHOOK_START_FAILURE";
        private static readonly string WebhookStartSuccess = "LIVE_STREAMING_API_WEBHOOK_START_SUCCESS";
        private static readonly string WebhookStopFailure = "LIVE_STREAMING_API_WEBHOOK_STOP_FAILURE";
        private static readonly string WebhookStopSuccess = "LIVE_STREAMING_API_WEBHOOK_STOP_SUCCESS";

        public static ConfigurationModel GetConfiguration()
        {
            Uri startFailure = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable(WebhookStartFailure)) ?
                new Uri(Environment.GetEnvironmentVariable(WebhookStartFailure)) : null;

            Uri startSuccess = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable(WebhookStartSuccess)) ?
                new Uri(Environment.GetEnvironmentVariable(WebhookStartSuccess)) : null;

            Uri stopFailure = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable(WebhookStopFailure)) ?
                new Uri(Environment.GetEnvironmentVariable(WebhookStopFailure)) : null;

            Uri stopSuccess = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable(WebhookStopSuccess)) ?
                new Uri(Environment.GetEnvironmentVariable(WebhookStopSuccess)) : null;

            ConfigurationModel model = new()
            {
                AccountName = Environment.GetEnvironmentVariable(AccountName),
                ArchiveWindowLength = Convert.ToInt32(Environment.GetEnvironmentVariable(ArchiveWindowLengthName)),
                ClientId = Environment.GetEnvironmentVariable(ClientIdName),
                ClientSecret = Environment.GetEnvironmentVariable(ClientSecretName),
                ManagementEndpoint = new Uri("https://management.azure.com/"),
                ResourceGroup = Environment.GetEnvironmentVariable(ResourceGroupName),
                SubscriptionId = Environment.GetEnvironmentVariable(SubscriptionIdName),
                TenantId = Environment.GetEnvironmentVariable(TenantIdName),
                WebhookStartFailure = startFailure,
                WebhookStartSuccess = startSuccess,
                WebhookStopFailure = stopFailure,
                WebhookStopSuccess = stopSuccess
            };

            LoggerService.Info("Extracted configuration", LoggerService.Bootstrapping);
            return model;
        }
    }
}

