using LiteralLifeChurch.LiveStreamingApi.Models.Bootstrapping;
using LiteralLifeChurch.LiveStreamingApi.Models.Input;
using LiteralLifeChurch.LiveStreamingApi.Models.Output;
using LiteralLifeChurch.LiveStreamingApi.Services.Common;
using Microsoft.Azure.Management.Media;
using Microsoft.Azure.Management.Media.Models;
using Microsoft.Rest.Azure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LiteralLifeChurch.LiveStreamingApi.Controllers
{
    public class LocatorsController : IController
    {
        private static readonly string DashExtension = "mpd";
        private static readonly string HlsExtension = "m3u8";
        private static readonly string Scheme = "https";

        private readonly AzureMediaServicesClient Client;
        private readonly ConfigurationModel Config;

        public LocatorsController(AzureMediaServicesClient client, ConfigurationModel config)
        {
            Client = client;
            Config = config;
        }

        public async Task<LocatorsOutputModel> GetLocatorsAsync(InputRequestModel input)
        {
            bool didAbortBuildingUrl = false;
            LocatorsOutputModel allLocators = new LocatorsOutputModel
            {
                IsAllLive = false,
                IsAnyLive = false,
                LiveEvents = new List<LocatorsOutputModel.LiveEvent>()
            };

            LoggerService.Info("Beginning the locators procedure", LoggerService.Locators);
            StreamingEndpoint streamingEndpoint = await GetStreamingEndpointAsync(input.StreamingEndpoint);

            LoggerService.Info($"Building {input.LiveEvents.Count} locator(s)", LoggerService.Locators);

            foreach (string liveEventName in input.LiveEvents)
            {
                List<LiveOutput> liveOutputs = await GetLiveOutputsAsync(liveEventName);

                if (!liveOutputs.Any())
                {
                    allLocators.LiveEvents.Add(GenerateEmptyLiveEvent(liveEventName));
                    didAbortBuildingUrl = true;

                    LoggerService.Warn($"Could not find any live outputs for live event '{liveEventName}'", LoggerService.Locators);
                    continue;
                }

                AssetStreamingLocator streamingLocator = await GetStreamingLocatorForAssetAsync(liveOutputs.First().AssetName);
                ListPathsResponse paths = await GetPathsForStreamingLocatorAsync(streamingLocator.Name);

                if (!paths.StreamingPaths.Any() || !paths.StreamingPaths.First().Paths.Any())
                {
                    allLocators.LiveEvents.Add(GenerateEmptyLiveEvent(liveEventName));
                    didAbortBuildingUrl = true;

                    LoggerService.Warn($"Could not find any paths for the streaming locator '{streamingLocator.Name}' associated with the live event '{liveEventName}'", LoggerService.Locators);
                    continue;
                }

                List<LocatorsOutputModel.LiveEvent.Locator> locators = MapStreamingPathsToLocatorUrls(streamingEndpoint.HostName, paths.StreamingPaths);

                allLocators.IsAnyLive = true;
                allLocators.LiveEvents.Add(new LocatorsOutputModel.LiveEvent
                {
                    Name = liveEventName,
                    IsLive = true,
                    Locators = locators
                });
            }

            LoggerService.Info($"Finished building {input.LiveEvents.Count} locator(s)", LoggerService.Locators);
            allLocators.IsAllLive = !didAbortBuildingUrl;
            return allLocators;
        }

        // region Workflow

        private static LocatorsOutputModel.LiveEvent GenerateEmptyLiveEvent(string liveEventName)
        {
            return new LocatorsOutputModel.LiveEvent
            {
                Name = liveEventName,
                IsLive = false,
                Locators = new List<LocatorsOutputModel.LiveEvent.Locator>()
            };
        }

        private async Task<List<LiveOutput>> GetLiveOutputsAsync(string liveEventName)
        {
            IPage<LiveOutput> liveOutputsPage = await Client.LiveOutputs.ListAsync(
                resourceGroupName: Config.ResourceGroup,
                accountName: Config.AccountName,
                liveEventName: liveEventName
            );

            List<LiveOutput> liveOutputs = liveOutputsPage.ToList();

            LoggerService.Info("Got the live outputs", LoggerService.Locators);
            return liveOutputs;
        }

        private async Task<ListPathsResponse> GetPathsForStreamingLocatorAsync(string streamingLocatorName)
        {
            ListPathsResponse paths = await Client.StreamingLocators.ListPathsAsync(
                resourceGroupName: Config.ResourceGroup,
                accountName: Config.AccountName,
                streamingLocatorName: streamingLocatorName
            );

            LoggerService.Info("Got the paths for the streaming locator", LoggerService.Locators);
            return paths;
        }

        private async Task<StreamingEndpoint> GetStreamingEndpointAsync(string streamingEndpoint)
        {
            StreamingEndpoint endpoint = await Client.StreamingEndpoints.GetAsync(
                resourceGroupName: Config.ResourceGroup,
                accountName: Config.AccountName,
                streamingEndpointName: streamingEndpoint
            );

            LoggerService.Info("Got the streaming endpoint", LoggerService.Locators);
            return endpoint;
        }

        private async Task<AssetStreamingLocator> GetStreamingLocatorForAssetAsync(string assetName)
        {
            ListStreamingLocatorsResponse locatorResponse = await Client.Assets.ListStreamingLocatorsAsync(
                resourceGroupName: Config.ResourceGroup,
                accountName: Config.AccountName,
                assetName: assetName
            );

            AssetStreamingLocator firstStreamingLocator = locatorResponse.StreamingLocators.First();

            LoggerService.Info("Got the streaming locator for asset", LoggerService.Locators);
            return firstStreamingLocator;
        }

        private static List<LocatorsOutputModel.LiveEvent.Locator> MapStreamingPathsToLocatorUrls(string hostName, IList<StreamingPath> paths)
        {
            List<LocatorsOutputModel.LiveEvent.Locator> locators = paths.Select(path =>
             {
                 LocatorsOutputModel.LiveEvent.Locator.LocatorType type;
                 UriBuilder uriBuilder;

                 if (path.StreamingProtocol == StreamingPolicyStreamingProtocol.Dash)
                 {
                     type = LocatorsOutputModel.LiveEvent.Locator.LocatorType.Dash;
                     uriBuilder = new UriBuilder
                     {
                         Scheme = Scheme,
                         Host = hostName,
                         Path = $"{path.Paths.First()}.{DashExtension}"
                     };
                 }
                 else if (path.StreamingProtocol == StreamingPolicyStreamingProtocol.Hls)
                 {
                     type = LocatorsOutputModel.LiveEvent.Locator.LocatorType.Hls;
                     uriBuilder = new UriBuilder
                     {
                         Scheme = Scheme,
                         Host = hostName,
                         Path = $"{path.Paths.First()}.{HlsExtension}"
                     };
                 }
                 else
                 {
                     type = LocatorsOutputModel.LiveEvent.Locator.LocatorType.Smooth;
                     uriBuilder = new UriBuilder
                     {
                         Scheme = Scheme,
                         Host = hostName,
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

            LoggerService.Info("Got the streaming paths to locator URLs", LoggerService.Locators);
            return locators;
        }

        // endregion
    }
}
