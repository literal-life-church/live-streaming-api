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
            // 1. Check the current status of all of the services
            StatusOutputModel preRunServiceStatus = await StatusService.GetStatusAsync(input);

            // 2. Stop the Streaming Endpoint
            if (preRunServiceStatus.StreamingEndpoint.Status == ResourceStatusEnum.Running)
            {
                await Client.StreamingEndpoints.StopAsync(
                    resourceGroupName: Config.ResourceGroup,
                    accountName: Config.AccountName,
                    streamingEndpointName: input.StreamingEndpoint
                );
            }

            foreach (string liveEventName in input.LiveEvents)
            {
                // 3. Abort if the current live event is not running
                StatusOutputModel.Resource liveEvent = preRunServiceStatus
                    .LiveEvents
                    .FindLast(currentEvent => currentEvent.Name == liveEventName);

                if (liveEvent.Status != ResourceStatusEnum.Running)
                {
                    continue;
                }

                // 4. Get all of the Live Outputs for the given Live Event
                IPage<LiveOutput> liveOutputsPage = await Client.LiveOutputs.ListAsync(
                    resourceGroupName: Config.ResourceGroup,
                    accountName: Config.AccountName,
                    liveEventName: liveEventName
                );

                List<LiveOutput> liveOutputs = liveOutputsPage.ToList();

                // 5. Stop the Live Event
                await Client.LiveEvents.StopAsync(
                    resourceGroupName: Config.ResourceGroup,
                    accountName: Config.AccountName,
                    liveEventName: liveEventName
                );

                foreach (LiveOutput liveOutput in liveOutputs)
                {

                    // 6. Delete the Streaming Locators
                    ListStreamingLocatorsResponse locators = await Client.Assets.ListStreamingLocatorsAsync(
                        resourceGroupName: Config.ResourceGroup,
                        accountName: Config.AccountName,
                        assetName: liveOutput.AssetName
                    );

                    foreach (AssetStreamingLocator locator in locators.StreamingLocators)
                    {
                        await Client.StreamingLocators.DeleteAsync(
                            resourceGroupName: Config.ResourceGroup,
                            accountName: Config.AccountName,
                            streamingLocatorName: locator.Name
                        );
                    }

                    // 7. Delete the asset
                    await Client.Assets.DeleteAsync(
                        resourceGroupName: Config.ResourceGroup,
                        accountName: Config.AccountName,
                        assetName: liveOutput.AssetName
                    );

                    // 8. Delete the Live Output
                    await Client.LiveOutputs.DeleteAsync(
                        resourceGroupName: Config.ResourceGroup,
                        accountName: Config.AccountName,
                        liveEventName: liveEventName,
                        liveOutputName: liveOutput.Name
                    );
                }
            }

            // 9. Check the current status of all of the services
            StatusOutputModel postRunServiceStatus = await StatusService.GetStatusAsync(input);
            StatusChangeOutputModel.Diff diff = calculateDiff(preRunServiceStatus, postRunServiceStatus);

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
