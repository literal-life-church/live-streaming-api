using LiteralLifeChurch.LiveStreamingApi.enums;
using LiteralLifeChurch.LiveStreamingApi.models.bootstrapping;
using LiteralLifeChurch.LiveStreamingApi.models.input;
using LiteralLifeChurch.LiveStreamingApi.models.output;
using LiteralLifeChurch.LiveStreamingApi.services.common;
using Microsoft.Azure.Management.Media;
using Microsoft.Azure.Management.Media.Models;
using Sentry;
using Sentry.Protocol;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LiteralLifeChurch.LiveStreamingApi.controllers
{
    public class StartController : IController
    {
        private readonly AzureMediaServicesClient Client;
        private readonly ConfigurationModel Config;
        private readonly StatusService StatusService;

        public StartController(AzureMediaServicesClient client, ConfigurationModel config)
        {
            Client = client;
            Config = config;
            StatusService = new StatusService(client, config);
        }

        public async Task<StatusChangeOutputModel> StartServicesAsync(InputRequestModel input)
        {
            SentrySdk.AddBreadcrumb(message: "Beginning start procedure", category: "start", level: BreadcrumbLevel.Info);

            // 1. Check the current status of all of the services
            StatusOutputModel preRunServiceStatus = await StatusService.GetStatusAsync(input);
            SentrySdk.AddBreadcrumb(message: "Got pre-run service status", category: "start", level: BreadcrumbLevel.Info);

            // 2. Start the Streaming Endpoint
            if (preRunServiceStatus.StreamingEndpoint.Status == ResourceStatusEnum.Stopped)
            {
                await Client.StreamingEndpoints.StartAsync(
                    resourceGroupName: Config.ResourceGroup,
                    accountName: Config.AccountName,
                    streamingEndpointName: input.StreamingEndpoint
                );

                SentrySdk.AddBreadcrumb(message: "Started streaming endpoint", category: "start", level: BreadcrumbLevel.Info);
            }
            else
            {
                SentrySdk.AddBreadcrumb(message: "Did not need to start streaming endpoint", category: "start", level: BreadcrumbLevel.Warning);
            }

            string message = string.Format("Starting {0} events", input.LiveEvents.Count);
            SentrySdk.AddBreadcrumb(message: message, category: "start", level: BreadcrumbLevel.Info);

            foreach (string liveEventName in input.LiveEvents)
            {
                string assetName = $"LiveStreamingApi-Asset-{liveEventName}-{Guid.NewGuid()}";
                string manifestName = "manifest";
                string liveOutputName = $"LiveStreamingApi-LiveOutput-{liveEventName}-{Guid.NewGuid()}";
                string streamingLocatorName = $"LiveStreamingApi-StreamingLocator-{liveEventName}-{Guid.NewGuid()}";

                // 3. Abort if the current live event is not stopped
                StatusOutputModel.Resource liveEvent = preRunServiceStatus
                    .LiveEvents
                    .FindLast(currentEvent => currentEvent.Name == liveEventName);

                if (liveEvent.Status != ResourceStatusEnum.Stopped)
                {
                    SentrySdk.AddBreadcrumb(message: "Did not start the live event", category: "start", level: BreadcrumbLevel.Info);
                    continue;
                }
                else
                {
                    SentrySdk.AddBreadcrumb(message: "Starting the live event", category: "start", level: BreadcrumbLevel.Info);
                }

                // 4. Create the asset
                Asset asset = await Client.Assets.CreateOrUpdateAsync(
                    resourceGroupName: Config.ResourceGroup,
                    accountName: Config.AccountName,
                    assetName: assetName,
                    parameters: new Asset()
                );

                SentrySdk.AddBreadcrumb(message: "Created the asset", category: "start", level: BreadcrumbLevel.Info);

                // 5. Create the Live Output
                LiveOutput liveOutput = new LiveOutput(
                    assetName: asset.Name,
                    manifestName: manifestName,
                    archiveWindowLength: TimeSpan.FromMinutes(10)
                );

                await Client.LiveOutputs.CreateAsync(
                    resourceGroupName: Config.ResourceGroup,
                    accountName: Config.AccountName,
                    liveEventName: liveEventName,
                    liveOutputName: liveOutputName,
                    parameters: liveOutput
                );

                SentrySdk.AddBreadcrumb(message: "Created the live output", category: "start", level: BreadcrumbLevel.Info);

                // 6. Create a Streaming Locator
                StreamingLocator locator = new StreamingLocator(
                    assetName: assetName,
                    streamingPolicyName: PredefinedStreamingPolicy.ClearStreamingOnly
                );

                await Client.StreamingLocators.CreateAsync(
                    resourceGroupName: Config.ResourceGroup,
                    accountName: Config.AccountName,
                    streamingLocatorName: streamingLocatorName,
                    parameters: locator
                );

                SentrySdk.AddBreadcrumb(message: "Created the streaming locator", category: "start", level: BreadcrumbLevel.Info);

                // 7. Start the Live Event
                await Client.LiveEvents.StartAsync(
                    resourceGroupName: Config.ResourceGroup,
                    accountName: Config.AccountName,
                    liveEventName: liveEventName
                );

                SentrySdk.AddBreadcrumb(message: "Started the event", category: "start", level: BreadcrumbLevel.Info);
            }

            // 8. Check the current status of all of the services
            StatusOutputModel postRunServiceStatus = await StatusService.GetStatusAsync(input);
            StatusChangeOutputModel.Diff diff = calculateDiff(preRunServiceStatus, postRunServiceStatus);
            SentrySdk.AddBreadcrumb(message: "Got post-run service status", category: "start", level: BreadcrumbLevel.Info);

            return new StatusChangeOutputModel()
            {
                Changes = diff,
                Status = postRunServiceStatus
            };
        }

        private StatusChangeOutputModel.Diff calculateDiff(StatusOutputModel preRunServiceStatus, StatusOutputModel postRunServiceStatus)
        {
            List<StatusChangeOutputModel.Diff.Resource> liveEventDiff = new List<StatusChangeOutputModel.Diff.Resource>();

            for (int i = 0; i < preRunServiceStatus.LiveEvents.Count; ++i)
            {
                StatusChangeOutputModel.Diff.Resource diff = new StatusChangeOutputModel.Diff.Resource()
                {
                    Name = preRunServiceStatus.LiveEvents[i].Name,
                    NewStatus = postRunServiceStatus.LiveEvents[i].Status,
                    OldStatus = preRunServiceStatus.LiveEvents[i].Status
                };

                liveEventDiff.Add(diff);
            }

            return new StatusChangeOutputModel.Diff()
            {
                LiveEvents = liveEventDiff,
                StreamingEndpoint = new StatusChangeOutputModel.Diff.Resource()
                {
                    Name = preRunServiceStatus.StreamingEndpoint.Name,
                    NewStatus = postRunServiceStatus.StreamingEndpoint.Status,
                    OldStatus = preRunServiceStatus.StreamingEndpoint.Status
                }
            };
        }
    }
}
