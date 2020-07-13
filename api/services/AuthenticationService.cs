using LiteralLifeChurch.LiveStreamingApi.Models.Bootstrapping;
using LiteralLifeChurch.LiveStreamingApi.Services.Common;
using Microsoft.Azure.Management.Media;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Rest;
using Microsoft.Rest.Azure.Authentication;
using System.Threading.Tasks;

namespace LiteralLifeChurch.LiveStreamingApi.Services
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
