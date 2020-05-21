using LiteralLifeChurch.LiveStreamingApi.enums;
using LiteralLifeChurch.LiveStreamingApi.models.bootstrapping;
using LiteralLifeChurch.LiveStreamingApi.models.input;
using LiteralLifeChurch.LiveStreamingApi.models.output;
using LiteralLifeChurch.LiveStreamingApi.models.workflow;
using LiteralLifeChurch.LiveStreamingApi.services.common;
using Microsoft.Azure.Management.Media;
using Microsoft.Azure.Management.Media.Models;
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
            LoggerService.Info("Beginning start procedure", LoggerService.Start);

            StatusOutputModel preRunServiceStatus = await GetServiceStatusAsync(input);
            await StartStreamingEndpointAsync(preRunServiceStatus, input.StreamingEndpoint);

            LoggerService.Info($"Starting {input.LiveEvents.Count} live event(s)", LoggerService.Start);

            foreach (string liveEventName in input.LiveEvents)
            {
                if (!IsLiveEventStopped(preRunServiceStatus, liveEventName))
                {
                    continue;
                }

                ResourceNamesModel resources = GenerateResourceNames(liveEventName);
                Asset asset = await CreateAssetAsync(resources.AssetName);
                await CreateLiveOutputAsync(asset, liveEventName, resources.LiveOutputName, resources.ManifestName);
                await CreateStreamingLocatorAsync(asset, resources.StreamingLocatorName);
                await StartLiveEventAsync(liveEventName);
            }

            StatusOutputModel postRunServiceStatus = await GetServiceStatusAsync(input);
            return GenerateStatusChange(preRunServiceStatus, postRunServiceStatus);
        }

        // region Workflow

        private async Task<Asset> CreateAssetAsync(string assetName)
        {
            Asset asset = await Client.Assets.CreateOrUpdateAsync(
                resourceGroupName: Config.ResourceGroup,
                accountName: Config.AccountName,
                assetName: assetName,
                parameters: new Asset()
            );

            LoggerService.Info("Created the asset", LoggerService.Start);
            return asset;
        }

        private async Task CreateLiveOutputAsync(Asset asset, string liveEventName, string liveOutputName, string manifestName)
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

            LoggerService.Info("Created the live output", LoggerService.Start);
        }

        private async Task CreateStreamingLocatorAsync(Asset asset, string streamingLocatorName)
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

            LoggerService.Info("Created the streaming locator", LoggerService.Start);
        }

        private static ResourceNamesModel GenerateResourceNames(string liveEventName)
        {
            return new ResourceNamesModel
            {
                AssetName = $"LiveStreamingApi-Asset-{liveEventName}-{Guid.NewGuid()}",
                LiveOutputName = $"LiveStreamingApi-LiveOutput-{liveEventName}-{Guid.NewGuid()}",
                ManifestName = "manifest",
                StreamingLocatorName = $"LiveStreamingApi-StreamingLocator-{liveEventName}-{Guid.NewGuid()}"
            };
        }

        private static StatusChangeOutputModel GenerateStatusChange(StatusOutputModel preRunServiceStatus, StatusOutputModel postRunServiceStatus)
        {
            List<StatusChangeOutputModel.Diff.Resource> liveEventDiff = new List<StatusChangeOutputModel.Diff.Resource>();

            for (int i = 0; i < preRunServiceStatus.LiveEvents.Count; ++i)
            {
                liveEventDiff.Add(new StatusChangeOutputModel.Diff.Resource
                {
                    Name = preRunServiceStatus.LiveEvents[i].Name,
                    NewStatus = postRunServiceStatus.LiveEvents[i].Status,
                    OldStatus = preRunServiceStatus.LiveEvents[i].Status
                });
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

        private async Task<StatusOutputModel> GetServiceStatusAsync(InputRequestModel input)
        {
            StatusOutputModel status = await StatusService.GetStatusAsync(input);
            LoggerService.Info("Got service status", LoggerService.Start);
            return status;
        }

        private static bool IsLiveEventStopped(StatusOutputModel preRunServiceStatus, string liveEventName)
        {
            StatusOutputModel.Resource liveEvent = preRunServiceStatus
                    .LiveEvents
                    .FindLast(currentEvent => currentEvent.Name == liveEventName);

            if (liveEvent.Status != ResourceStatusEnum.Stopped)
            {
                LoggerService.Warn("Did not start the live event", LoggerService.Start);
                return false;
            }
            else
            {
                LoggerService.Info("Starting the live event", LoggerService.Start);
                return true;
            }
        }

        private async Task StartLiveEventAsync(string liveEventName)
        {
            await Client.LiveEvents.StartAsync(
                resourceGroupName: Config.ResourceGroup,
                accountName: Config.AccountName,
                liveEventName: liveEventName
            );

            LoggerService.Info("Started the live event", LoggerService.Start);
        }

        private async Task StartStreamingEndpointAsync(StatusOutputModel preRunServiceStatus, string endpointName)
        {
            if (preRunServiceStatus.StreamingEndpoint.Status == ResourceStatusEnum.Stopped)
            {
                await Client.StreamingEndpoints.StartAsync(
                    resourceGroupName: Config.ResourceGroup,
                    accountName: Config.AccountName,
                    streamingEndpointName: endpointName
                );

                LoggerService.Info("Started streaming endpoint", LoggerService.Start);
            }
            else
            {
                LoggerService.Warn("Did not need to start streaming endpoint", LoggerService.Start);
            }
        }

        // endregion
    }
}
