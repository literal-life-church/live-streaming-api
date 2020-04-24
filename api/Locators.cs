using LiteralLifeChurch.LiveStreamingApi.models.bootstrapping;
using LiteralLifeChurch.LiveStreamingApi.models.input;
using LiteralLifeChurch.LiveStreamingApi.models.output;
using LiteralLifeChurch.LiveStreamingApi.services;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Management.Media;
using Microsoft.Azure.Management.Media.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Rest.Azure;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace LiteralLifeChurch.LiveStreamingApi
{
    public static class Locators
    {
        private static readonly AuthenticationService authService = new AuthenticationService();
        private static readonly ConfigurationService configService = new ConfigurationService();
        private const string EndpointQuery = "endpoint";
        private const string EventsQuery = "events";

        [FunctionName("Locators")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "locators")] HttpRequest req,
            ILogger log)
        {
            InputRequestModel input = GetInputModel(req);

            if (!input.LiveEvents.Any() && string.IsNullOrEmpty(input.StreamingEndpoint))
                return CreateError("Input requires the name of a streaming endpoint and the name of one or more live events");

            if (!input.LiveEvents.Any())
                return CreateError("Input requires the name of one or more live events");

            if (string.IsNullOrEmpty(input.StreamingEndpoint))
                return CreateError("Input requires the name of a streaming endpoint");

            try
            {
                LocatorsOutputModel locators = await FetchLocatorsAsync(input);
                return CreateSuccess(locators);
            }
            catch (Exception)
            {
                return CreateError("An internal error occured during. Check the logs.");
            }
        }

        private static HttpResponseMessage CreateError(string message)
        {
            ErrorModel error = new ErrorModel()
            {
                Message = message
            };

            string errorJson = JsonConvert.SerializeObject(error);

            return new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent(errorJson, Encoding.UTF8, "application/json")
            };
        }

        private static HttpResponseMessage CreateSuccess(LocatorsOutputModel locators)
        {
            string successJson = JsonConvert.SerializeObject(locators);

            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(successJson, Encoding.UTF8, "application/json")
            };
        }

        private static InputRequestModel GetInputModel(HttpRequest req)
        {
            List<string> liveEvents = req.Query[EventsQuery]
               .ToString()
               .Split(',')
               .Where(x => !string.IsNullOrEmpty(x.Trim()))
               .ToList();

            string streamingEndpoint = req.Query[EndpointQuery]
                .ToString()
                .Trim();

            return new InputRequestModel()
            {
                LiveEvents = liveEvents,
                StreamingEndpoint = streamingEndpoint
            };
        }

        private static async Task<LocatorsOutputModel> FetchLocatorsAsync(InputRequestModel input)
        {
            LocatorsOutputModel allLocators = new LocatorsOutputModel
            {
                IsLive = false,
                LiveEvents = new List<LocatorsOutputModel.LiveEvent>()
            };

            ConfigurationModel config = configService.GetConfiguration();

            // 1. Authenticate with Azure
            AzureMediaServicesClient client = await authService.GetClientAsync();

            // 2. Get the Streaming Endpoint
            StreamingEndpoint streamingEndpoint = await client.StreamingEndpoints.GetAsync(
                resourceGroupName: config.ResourceGroup,
                accountName: config.AccountName,
                streamingEndpointName: input.StreamingEndpoint
            );

            foreach (string liveEventName in input.LiveEvents)
            {
                // 3. Get the Live Output
                IPage<LiveOutput> liveOutputsPage = await client.LiveOutputs.ListAsync(
                    resourceGroupName: config.ResourceGroup,
                    accountName: config.AccountName,
                    liveEventName: liveEventName
                );

                List<LiveOutput> liveOutputs = liveOutputsPage.ToList();

                if (!liveOutputs.Any())
                {
                    continue;
                }

                // 4. Fetch the Locators for the Asset
                ListStreamingLocatorsResponse locatorResponse = await client.Assets.ListStreamingLocatorsAsync(
                    resourceGroupName: config.ResourceGroup,
                    accountName: config.AccountName,
                    assetName: liveOutputs.First().AssetName
                );

                AssetStreamingLocator firstStreamingLocator = locatorResponse.StreamingLocators.First();

                // 5. Fetch the Paths for that Locator
                ListPathsResponse paths = await client.StreamingLocators.ListPathsAsync(
                    resourceGroupName: config.ResourceGroup,
                    accountName: config.AccountName,
                    streamingLocatorName: firstStreamingLocator.Name
                );

                if (!paths.StreamingPaths.Any())
                {
                    continue;
                }

                if (!paths.StreamingPaths.First().Paths.Any())
                {
                    continue;
                }

                // 6. Build the URL
                List<LocatorsOutputModel.LiveEvent.Locator> locators = paths
                    .StreamingPaths
                    .Select(path =>
                    {
                        LocatorsOutputModel.LiveEvent.Locator.LocatorType type = LocatorsOutputModel.LiveEvent.Locator.LocatorType.DASH;
                        UriBuilder uriBuilder;

                        if (path.StreamingProtocol == StreamingPolicyStreamingProtocol.Dash)
                        {
                            type = LocatorsOutputModel.LiveEvent.Locator.LocatorType.DASH;
                            uriBuilder = new UriBuilder
                            {
                                Scheme = "https",
                                Host = streamingEndpoint.HostName,
                                Path = path.Paths.First() + ".mpd"
                            };
                        }
                        else if (path.StreamingProtocol == StreamingPolicyStreamingProtocol.Hls)
                        {
                            type = LocatorsOutputModel.LiveEvent.Locator.LocatorType.HLS;
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

                // 7. Build the return model
                LocatorsOutputModel.LiveEvent output = new LocatorsOutputModel.LiveEvent
                {
                    Name = liveEventName,
                    Locators = locators
                };

                allLocators.LiveEvents.Add(output);
            }

            allLocators.IsLive = allLocators.LiveEvents.Any();
            return allLocators;
        }
    }
}
