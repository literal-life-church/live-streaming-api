using LiteralLifeChurch.LiveStreamingApi.models;
using LiteralLifeChurch.LiveStreamingApi.models.input;
using LiteralLifeChurch.LiveStreamingApi.services;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Management.Media;
using Microsoft.Azure.Management.Media.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
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
        private static readonly AuthenticationService auth = new AuthenticationService();
        private static readonly string EndpointQuery = "endpoint";
        private static readonly string EventsQuery = "events";

        [FunctionName("Start")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            StartInputModel input = GetInputModel(req);

            if (!input.LiveEvents.Any() && string.IsNullOrEmpty(input.StreamingEndpoint))
                return CreateError("Input requires the name of a streaming endpoint and the name of one or more live events");

            if (!input.LiveEvents.Any())
                return CreateError("Input requires the name of one or more live events");

            if (string.IsNullOrEmpty(input.StreamingEndpoint))
                return CreateError("Input requires the name of a streaming endpoint");

            await StartServicesAsync(input);

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("yay", Encoding.UTF8, "application/json")
            };
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
            ConfigurationService configService = new ConfigurationService();
            ConfigurationModel config = configService.GetConfiguration();

            // 1. Authenticate with Azure
            AzureMediaServicesClient client = await auth.GetClientAsync();

            // 2. Start the Streaming Endpoint
            await client.StreamingEndpoints.StartAsync(
                resourceGroupName: config.ResourceGroup,
                accountName: config.AccountName,
                streamingEndpointName: serviceList.StreamingEndpoint
            );

            foreach (string liveEvent in serviceList.LiveEvents)
            {
                string assetName = $"LiveStreamingApi-Asset-{liveEvent}-{Guid.NewGuid().ToString()}";
                string manifestName = "output";
                string liveOutputName = $"LiveStreamingApi-LiveOutput-{liveEvent}-{Guid.NewGuid().ToString()}";

                // 3. Create the Live Output
                LiveOutput liveOutput = new LiveOutput(
                    assetName: assetName,
                    manifestName: manifestName,
                    archiveWindowLength: TimeSpan.FromMinutes(10)
                );

                await client.LiveOutputs.CreateAsync(
                    resourceGroupName: config.ResourceGroup,
                    accountName: config.AccountName,
                    liveEventName: liveEvent,
                    liveOutputName: liveOutputName,
                    parameters: liveOutput
                );
            }

            var x = 42;
        }
    }
}
