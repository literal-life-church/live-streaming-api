using LiteralLifeChurch.LiveStreamingApi.models.bootstrapping;
using LiteralLifeChurch.LiveStreamingApi.services.common;
using Microsoft.Azure.Management.Media;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Rest;
using Microsoft.Rest.Azure.Authentication;
using System.Threading.Tasks;

namespace LiteralLifeChurch.LiveStreamingApi.services
{
    public static class AuthenticationService
    {
        public static async Task<AzureMediaServicesClient> GetClientAsync(ConfigurationModel config)
        {
            ClientCredential clientCredentials = new ClientCredential(config.ClientId, config.ClientSecret);
            ServiceClientCredentials appCredentials = await ApplicationTokenProvider.LoginSilentAsync(config.TenantId, clientCredentials, ActiveDirectoryServiceSettings.Azure);

            AzureMediaServicesClient client = new AzureMediaServicesClient(config.ManagementEndpoint, appCredentials)
            {
                SubscriptionId = config.SubscriptionId,
            };

            LoggerService.Info("Created authorization client", LoggerService.Bootstrapping);
            return client;
        }
    }
}
