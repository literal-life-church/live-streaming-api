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
    public static class Stop
    {
        private static readonly AuthenticationService authService = new AuthenticationService();
        private static readonly ConfigurationService configService = new ConfigurationService();
        private static readonly string EndpointQuery = "endpoint";
        private static readonly string EventsQuery = "events";

        [FunctionName("Stop")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "stop")] HttpRequest req,
            ILogger log)
        {
            StopInputModel input = GetInputModel(req);

            if (!input.LiveEvents.Any() && string.IsNullOrEmpty(input.StreamingEndpoint))
                return CreateError("Input requires the name of a streaming endpoint and the name of one or more live events");

            if (!input.LiveEvents.Any())
                return CreateError("Input requires the name of one or more live events");

            if (string.IsNullOrEmpty(input.StreamingEndpoint))
                return CreateError("Input requires the name of a streaming endpoint");

            try
            {
                await StopServicesAsync(input);
            }
            catch (Exception e)
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

        private static StopInputModel GetInputModel(HttpRequest req)
        {
            List<string> liveEvents = req.Query[EventsQuery]
                .ToString()
                .Split(',')
                .Where(x => !string.IsNullOrEmpty(x.Trim()))
                .ToList();

            string streamingEndpoint = req.Query[EndpointQuery]
                .ToString()
                .Trim();

            return new StopInputModel()
            {
                LiveEvents = liveEvents,
                StreamingEndpoint = streamingEndpoint
            };
        }

        private static async Task StopServicesAsync(StopInputModel serviceList)
        {
            ConfigurationModel config = configService.GetConfiguration();

            // 1. Authenticate with Azure
            AzureMediaServicesClient client = await authService.GetClientAsync();

            // 2. Stop the Streaming Endpoint
            StreamingEndpoint streamingEndpoint = await client.StreamingEndpoints.GetAsync(
                resourceGroupName: config.ResourceGroup,
                accountName: config.AccountName,
                streamingEndpointName: serviceList.StreamingEndpoint
            );

            if (streamingEndpoint.ResourceState == StreamingEndpointResourceState.Running)
            {
                await client.StreamingEndpoints.StopAsync(
                    resourceGroupName: config.ResourceGroup,
                    accountName: config.AccountName,
                    streamingEndpointName: serviceList.StreamingEndpoint
                );
            }

            foreach (string liveEventName in serviceList.LiveEvents)
            {
                IPage<LiveOutput> liveOutputsPage = await client.LiveOutputs.ListAsync(
                    resourceGroupName: config.ResourceGroup,
                    accountName: config.AccountName,
                    liveEventName: liveEventName
                );

                List<LiveOutput> liveOutputs = liveOutputsPage.ToList();

                foreach (LiveOutput liveOutput in liveOutputs)
                {
                    // 3. Delete the Live Output
                    await client.LiveOutputs.DeleteAsync(
                        resourceGroupName: config.ResourceGroup,
                        accountName: config.AccountName,
                        liveEventName: liveEventName,
                        liveOutputName: liveOutput.Name
                    );

                    // 4. Delete the Streaming Locator
                    IPage<StreamingLocator> locatorsPage = await client.StreamingLocators.ListAsync(
                        resourceGroupName: config.ResourceGroup,
                        accountName: config.AccountName
                    );

                    StreamingLocator locator = locatorsPage
                        .ToList()
                        .Where(x => x.AssetName == liveOutput.AssetName)
                        .FirstOrDefault(null);

                    //client.Assets.ListStreamingLocators()

                    await client.StreamingLocators.DeleteAsync(
                        resourceGroupName: config.ResourceGroup,
                        accountName: config.AccountName,
                        streamingLocatorName: locator.Name
                    );

                    // 5. Delete the asset
                    await client.Assets.DeleteAsync(
                        resourceGroupName: config.ResourceGroup,
                        accountName: config.AccountName,
                        assetName: liveOutput.AssetName
                    );
                }

                // 6. Stop the Live Event
                await client.LiveEvents.StopAsync(
                    resourceGroupName: config.ResourceGroup,
                    accountName: config.AccountName,
                    liveEventName: liveEventName
                );
            }
        }
    }
}
