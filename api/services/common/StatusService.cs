using LiteralLifeChurch.LiveStreamingApi.enums;
using LiteralLifeChurch.LiveStreamingApi.models.bootstrapping;
using LiteralLifeChurch.LiveStreamingApi.models.input;
using LiteralLifeChurch.LiveStreamingApi.models.output;
using Microsoft.Azure.Management.Media;
using Microsoft.Azure.Management.Media.Models;
using Microsoft.Rest.Azure;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LiteralLifeChurch.LiveStreamingApi.services.common
{
    public class StatusService : ICommonService
    {
        private readonly AzureMediaServicesClient Client;
        private readonly ConfigurationModel Config;

        private const ResourceStatusEnum deleting = ResourceStatusEnum.Deleting;
        private const ResourceStatusEnum error = ResourceStatusEnum.Error;
        private const ResourceStatusEnum running = ResourceStatusEnum.Running;
        private const ResourceStatusEnum scaling = ResourceStatusEnum.Scaling;
        private const ResourceStatusEnum starting = ResourceStatusEnum.Starting;
        private const ResourceStatusEnum stopped = ResourceStatusEnum.Stopped;
        private const ResourceStatusEnum stopping = ResourceStatusEnum.Stopping;

        public StatusService(AzureMediaServicesClient client, ConfigurationModel config)
        {
            Client = client;
            Config = config;
        }

        public async Task<StatusOutputModel> GetStatusAsync(InputRequestModel input)
        {
            LoggerService.Info("Beginning the status procedure", LoggerService.Status);

            StreamingEndpoint endpoint = await GetStreamingEndpointAsync(input.StreamingEndpoint);
            List<LiveEvent> liveEvents = await GetLiveEventsAsync(input.LiveEvents);

            StatusOutputModel.Resource mappedEndpoint = MapStreamingResourceToOurResource(endpoint);
            List<StatusOutputModel.Resource> mappedEvents = MapLiveEventResourceToOurResource(liveEvents);

            return new StatusOutputModel
            {
                LiveEvents = mappedEvents,
                StreamingEndpoint = mappedEndpoint,
                Summary = DetermineSummary(mappedEndpoint, mappedEvents)
            };
        }

        // region Workflow

        private static ResourceStatusEnum DetermineSummary(StatusOutputModel.Resource endpoint, List<StatusOutputModel.Resource> events)
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
                LoggerService.Error("Encountered an unknown summary state", LoggerService.Status);
                return error;
            }
        }

        private async Task<List<LiveEvent>> GetLiveEventsAsync(List<string> liveEventNames)
        {
            IPage<LiveEvent> eventsPage = await Client.LiveEvents.ListAsync(
                resourceGroupName: Config.ResourceGroup,
                accountName: Config.AccountName
            );

            List<LiveEvent> events = eventsPage
                .Where(liveEvent => liveEventNames.Contains(liveEvent.Name))
                .ToList();

            LoggerService.Info("Got the live events", LoggerService.Status);
            return events;
        }

        private async Task<StreamingEndpoint> GetStreamingEndpointAsync(string streamingEndpoint)
        {
            StreamingEndpoint endpoint = await Client.StreamingEndpoints.GetAsync(
                resourceGroupName: Config.ResourceGroup,
                accountName: Config.AccountName,
                streamingEndpointName: streamingEndpoint
            );

            LoggerService.Info("Got the streaming endpoint", LoggerService.Status);
            return endpoint;
        }

        private static List<StatusOutputModel.Resource> MapLiveEventResourceToOurResource(List<LiveEvent> liveEvents)
        {
            LoggerService.Info($"Mapping {liveEvents.Count} live event(s)", LoggerService.Status);

            List<StatusOutputModel.Resource> mappedLiveEvents = liveEvents
                .Select(liveEvent =>
                {
                    ResourceStatusEnum status;

                    if (!liveEvent.ResourceState.HasValue)
                    {
                        LoggerService.Error($"Azure did not report a status for the live event '{liveEvent.Name}'", LoggerService.Status);
                        status = error;
                    }
                    else if (liveEvent.ResourceState.Value == LiveEventResourceState.Deleting)
                    {
                        status = deleting;
                    }
                    else if (liveEvent.ResourceState.Value == LiveEventResourceState.Running)
                    {
                        status = running;
                    }
                    else if (liveEvent.ResourceState.Value == LiveEventResourceState.Starting)
                    {
                        status = starting;
                    }
                    else if (liveEvent.ResourceState.Value == LiveEventResourceState.Stopped)
                    {
                        status = stopped;
                    }
                    else if (liveEvent.ResourceState.Value == LiveEventResourceState.Stopping)
                    {
                        status = stopping;
                    }
                    else
                    {
                        LoggerService.Error($"Encountered an unknown state for the live event '{liveEvent.Name}'", LoggerService.Status);
                        status = error;
                    }

                    return new StatusOutputModel.Resource
                    {
                        Name = liveEvent.Name,
                        Status = status
                    };
                })
                .ToList();

            LoggerService.Info("Mapped the live event(s)", LoggerService.Status);
            return mappedLiveEvents;
        }

        private static StatusOutputModel.Resource MapStreamingResourceToOurResource(StreamingEndpoint endpoint)
        {
            ResourceStatusEnum status;

            if (!endpoint.ResourceState.HasValue)
            {
                LoggerService.Error($"Azure did not report a status for the streaming endpoint '{endpoint.Name}'", LoggerService.Status);
                status = error;
            }
            else if (endpoint.ResourceState.Value == StreamingEndpointResourceState.Deleting)
            {
                status = deleting;
            }
            else if (endpoint.ResourceState.Value == StreamingEndpointResourceState.Running)
            {
                status = running;
            }
            else if (endpoint.ResourceState.Value == StreamingEndpointResourceState.Scaling)
            {
                status = scaling;
            }
            else if (endpoint.ResourceState.Value == StreamingEndpointResourceState.Starting)
            {
                status = starting;
            }
            else if (endpoint.ResourceState.Value == StreamingEndpointResourceState.Stopped)
            {
                status = stopped;
            }
            else if (endpoint.ResourceState.Value == StreamingEndpointResourceState.Stopping)
            {
                status = stopping;
            }
            else
            {
                LoggerService.Error($"Encountered an unknown state for the streaming endpoint '{endpoint.Name}'", LoggerService.Status);
                status = error;
            }

            StatusOutputModel.Resource mappedStreamingEndpoint = new StatusOutputModel.Resource
            {
                Name = endpoint.Name,
                Status = status
            };

            LoggerService.Info($"Mapped the streaming endpoint", LoggerService.Status);
            return mappedStreamingEndpoint;
        }

        // endregion
    }
}
