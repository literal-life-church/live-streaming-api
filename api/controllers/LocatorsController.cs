using LiteralLifeChurch.LiveStreamingApi.models.bootstrapping;
using LiteralLifeChurch.LiveStreamingApi.models.input;
using LiteralLifeChurch.LiveStreamingApi.models.output;
using LiteralLifeChurch.LiveStreamingApi.services.common;
using Microsoft.Azure.Management.Media;
using Microsoft.Azure.Management.Media.Models;
using Microsoft.Rest.Azure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LiteralLifeChurch.LiveStreamingApi.controllers
{
    public class LocatorsController : IController
    {
        private readonly AzureMediaServicesClient Client;
        private readonly ConfigurationModel Config;

        public LocatorsController(AzureMediaServicesClient client, ConfigurationModel config)
        {
            Client = client;
            Config = config;
        }

        public async Task<LocatorsOutputModel> GetLocatorsAsync(InputRequestModel input)
        {
            LocatorsOutputModel allLocators = new LocatorsOutputModel
            {
                IsAllLive = false,
                IsAnyLive = false,
                LiveEvents = new List<LocatorsOutputModel.LiveEvent>()
            };

            bool didAbortBuildingUrl = false;

            // 1. Get the Streaming Endpoint
            StreamingEndpoint streamingEndpoint = await Client.StreamingEndpoints.GetAsync(
                resourceGroupName: Config.ResourceGroup,
                accountName: Config.AccountName,
                streamingEndpointName: input.StreamingEndpoint
            );

            foreach (string liveEventName in input.LiveEvents)
            {
                // 2. Get the Live Output
                IPage<LiveOutput> liveOutputsPage = await Client.LiveOutputs.ListAsync(
                    resourceGroupName: Config.ResourceGroup,
                    accountName: Config.AccountName,
                    liveEventName: liveEventName
                );

                List<LiveOutput> liveOutputs = liveOutputsPage.ToList();

                if (!liveOutputs.Any())
                {
                    LocatorsOutputModel.LiveEvent emptyOutput = new LocatorsOutputModel.LiveEvent
                    {
                        Name = liveEventName,
                        IsLive = false,
                        Locators = new List<LocatorsOutputModel.LiveEvent.Locator>()
                    };

                    allLocators.LiveEvents.Add(emptyOutput);
                    didAbortBuildingUrl = true;

                    continue;
                }

                // 3. Fetch the Locators for the Asset associated with the Live Output
                ListStreamingLocatorsResponse locatorResponse = await Client.Assets.ListStreamingLocatorsAsync(
                    resourceGroupName: Config.ResourceGroup,
                    accountName: Config.AccountName,
                    assetName: liveOutputs.First().AssetName
                );

                AssetStreamingLocator firstStreamingLocator = locatorResponse.StreamingLocators.First();

                // 4. Fetch the Paths for that Locator
                ListPathsResponse paths = await Client.StreamingLocators.ListPathsAsync(
                    resourceGroupName: Config.ResourceGroup,
                    accountName: Config.AccountName,
                    streamingLocatorName: firstStreamingLocator.Name
                );

                if (!paths.StreamingPaths.Any() || !paths.StreamingPaths.First().Paths.Any())
                {
                    LocatorsOutputModel.LiveEvent emptyOutput = new LocatorsOutputModel.LiveEvent
                    {
                        Name = liveEventName,
                        IsLive = false,
                        Locators = new List<LocatorsOutputModel.LiveEvent.Locator>()
                    };

                    allLocators.LiveEvents.Add(emptyOutput);
                    didAbortBuildingUrl = true;

                    continue;
                }

                // 5. Build the URL
                List<LocatorsOutputModel.LiveEvent.Locator> locators = paths
                    .StreamingPaths
                    .Select(path =>
                    {
                        LocatorsOutputModel.LiveEvent.Locator.LocatorType type = LocatorsOutputModel.LiveEvent.Locator.LocatorType.Dash;
                        UriBuilder uriBuilder;

                        if (path.StreamingProtocol == StreamingPolicyStreamingProtocol.Dash)
                        {
                            type = LocatorsOutputModel.LiveEvent.Locator.LocatorType.Dash;
                            uriBuilder = new UriBuilder
                            {
                                Scheme = "https",
                                Host = streamingEndpoint.HostName,
                                Path = path.Paths.First() + ".mpd"
                            };
                        }
                        else if (path.StreamingProtocol == StreamingPolicyStreamingProtocol.Hls)
                        {
                            type = LocatorsOutputModel.LiveEvent.Locator.LocatorType.Hls;
                            uriBuilder = new UriBuilder
                            {
                                Scheme = "https",
                                Host = streamingEndpoint.HostName,
                                Path = path.Paths.First() + ".m3u8"
                            };
                        }
                        else
                        {
                            type = LocatorsOutputModel.LiveEvent.Locator.LocatorType.Smooth;
                            uriBuilder = new UriBuilder
                            {
                                Scheme = "https",
                                Host = streamingEndpoint.HostName,
                                Path = path.Paths.First()
                            };
                        }

                        return new LocatorsOutputModel.LiveEvent.Locator
                        {
                            Type = type,
                            Url = uriBuilder.Uri
                        };
                    })
                    .ToList();

                // 6. Build the return model
                LocatorsOutputModel.LiveEvent output = new LocatorsOutputModel.LiveEvent
                {
                    Name = liveEventName,
                    IsLive = true,
                    Locators = locators
                };

                allLocators.IsAnyLive = true;
                allLocators.LiveEvents.Add(output);
            }

            allLocators.IsAllLive = !didAbortBuildingUrl;
            return allLocators;
        }
    }
}
