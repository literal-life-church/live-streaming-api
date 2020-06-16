using LiteralLifeChurch.LiveStreamingApi.enums;
using LiteralLifeChurch.LiveStreamingApi.models.bootstrapping;
using LiteralLifeChurch.LiveStreamingApi.models.input;
using LiteralLifeChurch.LiveStreamingApi.models.output;
using LiteralLifeChurch.LiveStreamingApi.services.common;
using Microsoft.Azure.Management.Media;
using Microsoft.Azure.Management.Media.Models;
using Microsoft.Rest.Azure;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LiteralLifeChurch.LiveStreamingApi.controllers
{
    public class StopController : IController
    {
        private readonly AzureMediaServicesClient Client;
        private readonly ConfigurationModel Config;
        private readonly StatusService StatusService;

        public StopController(AzureMediaServicesClient client, ConfigurationModel config)
        {
            Client = client;
            Config = config;
            StatusService = new StatusService(client, config);
        }

        public async Task<StatusChangeOutputModel> StopServicesAsync(InputRequestModel input)
        {
            LoggerService.Info("Beginning the stop procedure", LoggerService.Stop);

            StatusOutputModel preRunServiceStatus = await GetServiceStatusAsync(input);
            await StopStreamingEndpointAsync(preRunServiceStatus, input.StreamingEndpoint);

            LoggerService.Info($"Stopping {input.LiveEvents.Count} live event(s)", LoggerService.Stop);

            foreach (string liveEventName in input.LiveEvents)
            {
                if (!IsLiveEventRunning(preRunServiceStatus, liveEventName))
                {
                    continue;
                }

                await StopLiveEventAsync(liveEventName);
                List<LiveOutput> liveOutputs = await GetAllLiveOutputsForLiveEventAsync(liveEventName);

                LoggerService.Info($"Stopping {liveOutputs.Count} live output(s)", LoggerService.Stop);

                foreach (LiveOutput liveOutput in liveOutputs)
                {
                    await DeleteAllStreamingLocatorsForAssetAsync(liveOutput.AssetName);
                    await DeleteAssetAsync(liveOutput.AssetName);
                    await DeleteLiveOutputAsync(liveEventName, liveOutput.Name);
                }
            }

            StatusOutputModel postRunServiceStatus = await GetServiceStatusAsync(input);
            return GenerateStatusChange(preRunServiceStatus, postRunServiceStatus);
        }

        // region Workflow

        private async Task DeleteAllStreamingLocatorsForAssetAsync(string assetName)
        {
            ListStreamingLocatorsResponse locators = await Client.Assets.ListStreamingLocatorsAsync(
                resourceGroupName: Config.ResourceGroup,
                accountName: Config.AccountName,
                assetName: assetName
            );

            LoggerService.Info($"Deleting {locators.StreamingLocators.Count} streaming locator(s)", LoggerService.Stop);

            foreach (AssetStreamingLocator locator in locators.StreamingLocators)
            {
                await Client.StreamingLocators.DeleteAsync(
                    resourceGroupName: Config.ResourceGroup,
                    accountName: Config.AccountName,
                    streamingLocatorName: locator.Name
                );
            }

            LoggerService.Info("Deleted all streaming locator(s)", LoggerService.Stop);
        }

        private async Task DeleteAssetAsync(string assetName)
        {
            await Client.Assets.DeleteAsync(
                resourceGroupName: Config.ResourceGroup,
                accountName: Config.AccountName,
                assetName: assetName
            );

            LoggerService.Info("Deleted the asset", LoggerService.Stop);
        }

        private async Task DeleteLiveOutputAsync(string liveEventName, string liveOutputName)
        {
            await Client.LiveOutputs.DeleteAsync(
                resourceGroupName: Config.ResourceGroup,
                accountName: Config.AccountName,
                liveEventName: liveEventName,
                liveOutputName: liveOutputName
            );

            LoggerService.Info("Deleted the live output", LoggerService.Stop);
        }

        private static StatusChangeOutputModel GenerateStatusChange(StatusOutputModel preRunServiceStatus, StatusOutputModel postRunServiceStatus)
        {
            List<StatusChangeOutputModel.Diff.Resource> liveEventDiff = new List<StatusChangeOutputModel.Diff.Resource>();

            for (int i = 0; i < preRunServiceStatus.LiveEvents.Count; ++i)
            {
                liveEventDiff.Add(new StatusChangeOutputModel.Diff.Resource
                {
                    Name = preRunServiceStatus.LiveEvents[i].Name,
                    NewStatus = postRunServiceStatus.LiveEvents[i].Status.Name,
                    OldStatus = preRunServiceStatus.LiveEvents[i].Status.Name
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
                        NewStatus = postRunServiceStatus.StreamingEndpoint.Status.Name,
                        OldStatus = preRunServiceStatus.StreamingEndpoint.Status.Name
                    }
                },
                Status = postRunServiceStatus
            };
        }

        private async Task<List<LiveOutput>> GetAllLiveOutputsForLiveEventAsync(string liveEventName)
        {
            IPage<LiveOutput> liveOutputsPage = await Client.LiveOutputs.ListAsync(
                resourceGroupName: Config.ResourceGroup,
                accountName: Config.AccountName,
                liveEventName: liveEventName
            );

            List<LiveOutput> liveOutputs = liveOutputsPage.ToList();
            LoggerService.Info($"Got {liveOutputs.Count} for the live event '{liveEventName}'", LoggerService.Stop);

            return liveOutputs;
        }

        private async Task<StatusOutputModel> GetServiceStatusAsync(InputRequestModel input)
        {
            StatusOutputModel status = await StatusService.GetStatusAsync(input);
            LoggerService.Info("Got service status", LoggerService.Stop);
            return status;
        }

        private static bool IsLiveEventRunning(StatusOutputModel preRunServiceStatus, string liveEventName)
        {
            StatusOutputModel.Resource liveEvent = preRunServiceStatus
                    .LiveEvents
                    .FindLast(currentEvent => currentEvent.Name == liveEventName);

            if (liveEvent.Status.Name != ResourceStatusEnum.Running)
            {
                LoggerService.Warn("Did not stop the live event", LoggerService.Stop);
                return false;
            }
            else
            {
                LoggerService.Info("Stopping the live event", LoggerService.Stop);
                return true;
            }
        }

        private async Task StopLiveEventAsync(string liveEventName)
        {
            await Client.LiveEvents.StopAsync(
                resourceGroupName: Config.ResourceGroup,
                accountName: Config.AccountName,
                liveEventName: liveEventName
            );

            LoggerService.Info("Stopped the live event", LoggerService.Stop);
        }

        private async Task StopStreamingEndpointAsync(StatusOutputModel preRunServiceStatus, string endpointName)
        {
            if (preRunServiceStatus.StreamingEndpoint.Status.Name == ResourceStatusEnum.Running)
            {
                await Client.StreamingEndpoints.StopAsync(
                    resourceGroupName: Config.ResourceGroup,
                    accountName: Config.AccountName,
                    streamingEndpointName: endpointName
                );

                LoggerService.Info("Stopped streaming endpoint", LoggerService.Stop);
            }
            else
            {
                LoggerService.Warn("Did not need to stop streaming endpoint", LoggerService.Stop);
            }
        }

        // endregion
    }
}
