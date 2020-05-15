using LiteralLifeChurch.LiveStreamingApi.models.bootstrapping;
using Microsoft.Azure.Management.Media;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Rest;
using Microsoft.Rest.Azure.Authentication;
using Sentry;
using Sentry.Protocol;
using System.Threading.Tasks;

namespace LiteralLifeChurch.LiveStreamingApi.services
{
    class AuthenticationService
    {
        private readonly ConfigurationService configService = new ConfigurationService();

        public async Task<AzureMediaServicesClient> GetClientAsync()
        {
            ConfigurationModel config = configService.GetConfiguration();

            ClientCredential clientCredentials = new ClientCredential(config.ClientId, config.ClientSecret);
            ServiceClientCredentials appCredentials = await ApplicationTokenProvider.LoginSilentAsync(config.TenantId, clientCredentials, ActiveDirectoryServiceSettings.Azure);

            AzureMediaServicesClient client = new AzureMediaServicesClient(config.ManagementEndpoint, appCredentials)
            {
                SubscriptionId = config.SubscriptionId,
            };

            SentrySdk.AddBreadcrumb(message: "Created authorization client", category: "bootstrapping", level: BreadcrumbLevel.Info);
            return client;
        }
    }
}
