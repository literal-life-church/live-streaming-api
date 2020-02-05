using LiteralLifeChurch.LiveStreamingApi.models;
using LiteralLifeChurch.LiveStreamingApi.models.input;
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
    public static class Start
    {
        private static readonly AuthenticationService authService = new AuthenticationService();
        private static readonly ConfigurationService configService = new ConfigurationService();
        private static readonly string EndpointQuery = "endpoint";
        private static readonly string EventsQuery = "events";

        [FunctionName("Start")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "start")] HttpRequest req,
            ILogger log)
        {
            StartInputModel input = GetInputModel(req);

            if (!input.LiveEvents.Any() && string.IsNullOrEmpty(input.StreamingEndpoint))
                return CreateError("Input requires the name of a streaming endpoint and the name of one or more live events");

            if (!input.LiveEvents.Any())
                return CreateError("Input requires the name of one or more live events");

            if (string.IsNullOrEmpty(input.StreamingEndpoint))
                return CreateError("Input requires the name of a streaming endpoint");

            try
            {
                await StartServicesAsync(input);
            } catch (Exception)
            {
                return CreateError("An internal error occured during. Check the logs.");
            }

            return CreateSuccess("Created");
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

        private static HttpResponseMessage CreateSuccess(string message)
        {
            SuccessModel error = new SuccessModel()
            {
                Message = message
            };

            string successJson = JsonConvert.SerializeObject(error);

            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(successJson, Encoding.UTF8, "application/json")
            };
        }

        private static StartInputModel GetInputModel(HttpRequest req)
        {
            List<string> liveEvents = req.Query[EventsQuery]
                .ToString()
                .Split(',')
                .Where(x => !string.IsNullOrEmpty(x.Trim()))
                .ToList();

            string streamingEndpoint = req.Query[EndpointQuery]
                .ToString()
                .Trim();

            return new StartInputModel()
            {
                LiveEvents = liveEvents,
                StreamingEndpoint = streamingEndpoint
            };
        }

        private static async Task StartServicesAsync(StartInputModel serviceList)
        {
            ConfigurationModel config = configService.GetConfiguration();

            // 1. Authenticate with Azure
            AzureMediaServicesClient client = await authService.GetClientAsync();

            // 2. Start the Streaming Endpoint
            StreamingEndpoint streamingEndpoint = await client.StreamingEndpoints.GetAsync(
                resourceGroupName: config.ResourceGroup,
                accountName: config.AccountName,
                streamingEndpointName: serviceList.StreamingEndpoint
            );

            if (streamingEndpoint.ResourceState == StreamingEndpointResourceState.Stopped)
            {
                await client.StreamingEndpoints.StartAsync(
                    resourceGroupName: config.ResourceGroup,
                    accountName: config.AccountName,
                    streamingEndpointName: serviceList.StreamingEndpoint
                );
            }

            foreach (string liveEventName in serviceList.LiveEvents)
            {
                string assetName = $"LiveStreamingApi-Asset-{liveEventName}-{Guid.NewGuid().ToString()}";
                string manifestName = "output";
                string liveOutputName = $"LiveStreamingApi-LiveOutput-{liveEventName}-{Guid.NewGuid().ToString()}";
                string streamingLocatorName = $"LiveStreamingApi-StreamingLocator-{liveEventName}-{Guid.NewGuid().ToString()}";

                // 3. Create the asset
                Asset asset = await client.Assets.CreateOrUpdateAsync(
                    resourceGroupName: config.ResourceGroup,
                    accountName: config.AccountName,
                    assetName: assetName,
                    parameters: new Asset()
                );

                // 4. Create the Live Output
                LiveOutput liveOutput = new LiveOutput(
                    assetName: asset.Name,
                    manifestName: manifestName,
                    archiveWindowLength: TimeSpan.FromMinutes(10)
                );

                await client.LiveOutputs.CreateAsync(
                    resourceGroupName: config.ResourceGroup,
                    accountName: config.AccountName,
                    liveEventName: liveEventName,
                    liveOutputName: liveOutputName,
                    parameters: liveOutput
                );

                // 5. Create a Streaming Locator
                StreamingLocator locator = new StreamingLocator(
                    assetName: assetName,
                    streamingPolicyName: PredefinedStreamingPolicy.ClearStreamingOnly
                );

                await client.StreamingLocators.CreateAsync(
                    resourceGroupName: config.ResourceGroup,
                    accountName: config.AccountName,
                    streamingLocatorName: streamingLocatorName,
                    parameters: locator
                );

                // 6. Start the Live Event
                await client.LiveEvents.StartAsync(
                    resourceGroupName: config.ResourceGroup,
                    accountName: config.AccountName,
                    liveEventName: liveEventName
                );
            }
        }
    }
}
