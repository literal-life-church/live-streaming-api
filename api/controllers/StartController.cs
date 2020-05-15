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

            StatusOutputModel preRunServiceStatus = await GetPreRunServiceStatus(input);
            await StartStreamingEndpoint(preRunServiceStatus, input);

            SentrySdk.AddBreadcrumb(message: $"Starting {input.LiveEvents.Count} event(s)", category: "start", level: BreadcrumbLevel.Info);

            foreach (string liveEventName in input.LiveEvents)
            {
                string assetName = $"LiveStreamingApi-Asset-{liveEventName}-{Guid.NewGuid()}";
                string manifestName = "manifest";
                string liveOutputName = $"LiveStreamingApi-LiveOutput-{liveEventName}-{Guid.NewGuid()}";
                string streamingLocatorName = $"LiveStreamingApi-StreamingLocator-{liveEventName}-{Guid.NewGuid()}";

                if (!IsLiveEventStopped(preRunServiceStatus, liveEventName))
                {
                    continue;
                }

                Asset asset = await CreateAsset(assetName);
                await CreateLiveOutput(asset, liveEventName, liveOutputName, manifestName);
                await CreateStreamingLocator(asset, streamingLocatorName);
                await StartLiveEvent(liveEventName);
            }

            StatusOutputModel postRunServiceStatus = await GetPostRunServiceStatus(input);
            return GenerateStatusChange(preRunServiceStatus, postRunServiceStatus);
        }

        // region Workflow

        private async Task<Asset> CreateAsset(string assetName)
        {
            Asset asset = await Client.Assets.CreateOrUpdateAsync(
                resourceGroupName: Config.ResourceGroup,
                accountName: Config.AccountName,
                assetName: assetName,
                parameters: new Asset()
            );

            SentrySdk.AddBreadcrumb(message: "Created the asset", category: "start", level: BreadcrumbLevel.Info);
            return asset;
        }

        private async Task CreateLiveOutput(Asset asset, string liveEventName, string liveOutputName, string manifestName)
        {
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
        }

        private async Task CreateStreamingLocator(Asset asset, string streamingLocatorName)
        {
            StreamingLocator locator = new StreamingLocator(
                assetName: asset.Name,
                streamingPolicyName: PredefinedStreamingPolicy.ClearStreamingOnly
            );

            await Client.StreamingLocators.CreateAsync(
                resourceGroupName: Config.ResourceGroup,
                accountName: Config.AccountName,
                streamingLocatorName: streamingLocatorName,
                parameters: locator
            );

            SentrySdk.AddBreadcrumb(message: "Created the streaming locator", category: "start", level: BreadcrumbLevel.Info);
        }

        private static StatusChangeOutputModel GenerateStatusChange(StatusOutputModel preRunServiceStatus, StatusOutputModel postRunServiceStatus)
        {
            List<StatusChangeOutputModel.Diff.Resource> liveEventDiff = new List<StatusChangeOutputModel.Diff.Resource>();

            for (int i = 0; i < preRunServiceStatus.LiveEvents.Count; ++i)
            {
                StatusChangeOutputModel.Diff.Resource diff = new StatusChangeOutputModel.Diff.Resource
                {
                    Name = preRunServiceStatus.LiveEvents[i].Name,
                    NewStatus = postRunServiceStatus.LiveEvents[i].Status,
                    OldStatus = preRunServiceStatus.LiveEvents[i].Status
                };

                liveEventDiff.Add(diff);
            }

            return new StatusChangeOutputModel
            {
                Changes = new StatusChangeOutputModel.Diff
                {
                    LiveEvents = liveEventDiff,
                    StreamingEndpoint = new StatusChangeOutputModel.Diff.Resource
                    {
                        Name = preRunServiceStatus.StreamingEndpoint.Name,
                        NewStatus = postRunServiceStatus.StreamingEndpoint.Status,
                        OldStatus = preRunServiceStatus.StreamingEndpoint.Status
                    }
                },
                Status = postRunServiceStatus
            };
        }

        private async Task<StatusOutputModel> GetPostRunServiceStatus(InputRequestModel input)
        {
            StatusOutputModel status = await StatusService.GetStatusAsync(input);
            SentrySdk.AddBreadcrumb(message: "Got post-run service status", category: "start", level: BreadcrumbLevel.Info);
            return status;
        }

        private async Task<StatusOutputModel> GetPreRunServiceStatus(InputRequestModel input)
        {
            StatusOutputModel status = await StatusService.GetStatusAsync(input);
            SentrySdk.AddBreadcrumb(message: "Got pre-run service status", category: "start", level: BreadcrumbLevel.Info);
            return status;
        }

        private bool IsLiveEventStopped(StatusOutputModel preRunServiceStatus, string liveEventName)
        {
            StatusOutputModel.Resource liveEvent = preRunServiceStatus
                    .LiveEvents
                    .FindLast(currentEvent => currentEvent.Name == liveEventName);

            if (liveEvent.Status != ResourceStatusEnum.Stopped)
            {
                SentrySdk.AddBreadcrumb(message: "Did not start the live event", category: "start", level: BreadcrumbLevel.Info);
                return false;
            }
            else
            {
                SentrySdk.AddBreadcrumb(message: "Starting the live event", category: "start", level: BreadcrumbLevel.Info);
                return true;
            }
        }

        private async Task StartLiveEvent(string liveEventName)
        {
            await Client.LiveEvents.StartAsync(
                resourceGroupName: Config.ResourceGroup,
                accountName: Config.AccountName,
                liveEventName: liveEventName
            );

            SentrySdk.AddBreadcrumb(message: "Started the event", category: "start", level: BreadcrumbLevel.Info);
        }

        private async Task StartStreamingEndpoint(StatusOutputModel preRunServiceStatus, InputRequestModel input)
        {
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
        }

        // endregion
    }
}
