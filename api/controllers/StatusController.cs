using LiteralLifeChurch.LiveStreamingApi.models.bootstrapping;
using LiteralLifeChurch.LiveStreamingApi.models.input;
using LiteralLifeChurch.LiveStreamingApi.models.output;
using Microsoft.Azure.Management.Media;
using Microsoft.Azure.Management.Media.Models;
using Microsoft.Rest.Azure;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LiteralLifeChurch.LiveStreamingApi.controllers
{
    public class StatusController : IController
    {
        private readonly AzureMediaServicesClient Client;
        private readonly ConfigurationModel Config;

        private const StatusOutputModel.Status deleting = StatusOutputModel.Status.Deleting;
        private const StatusOutputModel.Status error = StatusOutputModel.Status.Error;
        private const StatusOutputModel.Status running = StatusOutputModel.Status.Running;
        private const StatusOutputModel.Status scaling = StatusOutputModel.Status.Scaling;
        private const StatusOutputModel.Status starting = StatusOutputModel.Status.Starting;
        private const StatusOutputModel.Status stopped = StatusOutputModel.Status.Stopped;
        private const StatusOutputModel.Status stopping = StatusOutputModel.Status.Stopping;

        public StatusController(AzureMediaServicesClient client, ConfigurationModel config)
        {
            Client = client;
            Config = config;
        }

        public async Task<StatusOutputModel> GetStatus(InputRequestModel input)
        {
            // Get the Streaming Endpoint
            StreamingEndpoint endpoint = await Client.StreamingEndpoints.GetAsync(
                resourceGroupName: Config.ResourceGroup,
                accountName: Config.AccountName,
                streamingEndpointName: input.StreamingEndpoint
            );

            // Get all of the Live Events, then filter them
            IPage<LiveEvent> eventsPage = await Client.LiveEvents.ListAsync(
                resourceGroupName: Config.ResourceGroup,
                accountName: Config.AccountName
            );

            List<LiveEvent> filteredEvents = eventsPage
                .Where(liveEvent => input.LiveEvents.Contains(liveEvent.Name))
                .ToList();

            // Summarize our findings
            StatusOutputModel.Resource mappedEndpoint = new StatusOutputModel.Resource()
            {
                Name = endpoint.Name,
                Status = MapStreamingStatusToOurStatus(endpoint.ResourceState)
            };

            List<StatusOutputModel.Resource> mappedEvents = filteredEvents
                .Select(liveEvent =>
                {
                    return new StatusOutputModel.Resource()
                    {
                        Name = liveEvent.Name,
                        Status = MapEventStatusToOurStatus(liveEvent.ResourceState)
                    };
                })
                .ToList();

            return new StatusOutputModel()
            {
                Endpoint = mappedEndpoint,
                Events = mappedEvents,
                Summary = DetermineSummary(mappedEndpoint, mappedEvents)
            };
        }

        private StatusOutputModel.Status DetermineSummary(StatusOutputModel.Resource endpoint, List<StatusOutputModel.Resource> events)
        {
            if (endpoint.Status == error || events.Any(liveEvent => liveEvent.Status == error))
            {
                return error;
            }
            else if (endpoint.Status == stopped && events.All(liveEvent => liveEvent.Status == stopped))
            {
                return stopped;
            }
            else if (endpoint.Status == starting && events.Any(liveEvent => liveEvent.Status == starting))
            {
                return starting;
            }
            else if (endpoint.Status == running && events.All(liveEvent => liveEvent.Status == running))
            {
                return running;
            }
            else if (endpoint.Status == stopping && events.Any(liveEvent => liveEvent.Status == stopping))
            {
                return stopping;
            }
            else if ((endpoint.Status == running || endpoint.Status == scaling) && events.Any(liveEvent => liveEvent.Status == running || liveEvent.Status == scaling))
            {
                return running;
            }
            else if ((endpoint.Status == deleting || endpoint.Status == stopped) && events.Any(liveEvent => liveEvent.Status == deleting || liveEvent.Status == stopped))
            {
                return stopped;
            }
            else
            {
                return error;
            }
        }

        private StatusOutputModel.Status MapEventStatusToOurStatus(LiveEventResourceState? state)
        {
            if (!state.HasValue)
            {
                return error;
            }

            if (state.Value == LiveEventResourceState.Deleting)
            {
                return deleting;
            }
            else if (state.Value == LiveEventResourceState.Running)
            {
                return running;
            }
            else if (state.Value == LiveEventResourceState.Starting)
            {
                return starting;
            }
            else if (state.Value == LiveEventResourceState.Stopped)
            {
                return stopped;
            }
            else if (state.Value == LiveEventResourceState.Stopping)
            {
                return stopping;
            }
            else
            {
                return error;
            }
        }

        private StatusOutputModel.Status MapStreamingStatusToOurStatus(StreamingEndpointResourceState? state)
        {
            if (!state.HasValue)
            {
                return error;
            }

            if (state.Value == StreamingEndpointResourceState.Deleting)
            {
                return deleting;
            }
            else if (state.Value == StreamingEndpointResourceState.Running)
            {
                return running;
            }
            else if (state.Value == StreamingEndpointResourceState.Scaling)
            {
                return scaling;
            }
            else if (state.Value == StreamingEndpointResourceState.Starting)
            {
                return starting;
            }
            else if (state.Value == StreamingEndpointResourceState.Stopped)
            {
                return stopped;
            }
            else if (state.Value == StreamingEndpointResourceState.Stopping)
            {
                return stopping;
            }
            else
            {
                return error;
            }
        }
    }
}
