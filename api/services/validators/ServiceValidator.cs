using LiteralLifeChurch.LiveStreamingApi.exceptions;
using LiteralLifeChurch.LiveStreamingApi.models.bootstrapping;
using LiteralLifeChurch.LiveStreamingApi.models.input;
using Microsoft.Azure.Management.Media;
using Microsoft.Azure.Management.Media.Models;
using Microsoft.Rest.Azure;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LiteralLifeChurch.LiveStreamingApi.services.validators
{
    public class ServiceValidator : IValidatorService
    {
        private readonly AzureMediaServicesClient Client;
        private readonly ConfigurationModel Config;

        public ServiceValidator(AzureMediaServicesClient client, ConfigurationModel config)
        {
            Client = client;
            Config = config;
        }

        public async Task ValidateAsync(InputRequestModel input)
        {
            // Validate the Streaming Endpoint
            IPage<StreamingEndpoint> endpointsPage = await Client.StreamingEndpoints.ListAsync(
                resourceGroupName: Config.ResourceGroup,
                accountName: Config.AccountName
            );

            List<StreamingEndpoint> endpoints = endpointsPage.ToList();
            bool hasTargetEndpoint = endpoints.Any() && endpoints
                .Where(endpoint => endpoint.Name == input.StreamingEndpoint)
                .Any();

            if (!hasTargetEndpoint)
            {
                throw new ServiceValidationException()
                {
                    DeveloperMessage = "The query parameter 'endpoint' does not match the name of any existing streaming endpoints",
                    Message = "The given streaming endpoint does not exist"
                };
            }

            // Validate the Live Events
            IPage<LiveEvent> eventsPage = await Client.LiveEvents.ListAsync(
                resourceGroupName: Config.ResourceGroup,
                accountName: Config.AccountName
            );

            List<LiveEvent> events = eventsPage.ToList();
            bool hasAllTargetEvents = events.Any() && events
                .Where(liveEvent => input.LiveEvents.Contains(liveEvent.Name))
                .Count() == input.LiveEvents.Count();

            if (!hasAllTargetEvents)
            {
                string nonExistentEvents = "";

                if (!events.Any())
                {
                    nonExistentEvents = string.Join(", ", input.LiveEvents);
                }
                else
                {
                    List<string> givenLiveEvents = new List<string>(input.LiveEvents);

                    givenLiveEvents
                        .RemoveAll(givenLiveEvent => events.Any(existingLiveEvent => existingLiveEvent.Name == givenLiveEvent));

                    nonExistentEvents = string.Join(", ", givenLiveEvents);
                }

                throw new ServiceValidationException()
                {
                    DeveloperMessage = string.Format("The following live event(s) in the query parameter 'events' does not match the name(s) of any the existing live events: {0}", nonExistentEvents),
                    Message = string.Format("The following live event(s) do not exist: {0}", nonExistentEvents)
                };
            }
        }
    }
}
