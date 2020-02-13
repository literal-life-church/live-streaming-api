using LiteralLifeChurch.LiveStreamingApi.models;
using LiteralLifeChurch.LiveStreamingApi.models.input;
using LiteralLifeChurch.LiveStreamingApi.models.output;
using LiteralLifeChurch.LiveStreamingApi.services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
        private const string EventsQuery = "events";

        [FunctionName("Locators")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "locators")] HttpRequest req,
            ILogger log)
        {
            LocatorsInputModel input = GetInputModel(req);

            if (!input.LiveEvents.Any())
                return CreateError("Input requires the name of one or more live events");

            try
            {
                await FetchLocatorsAsync(input);
            }
            catch (Exception)
            {
                return CreateError("An internal error occured during. Check the logs.");
            }

            return name != null
                ? (ActionResult)new OkObjectResult($"Hello, {name}")
                : new BadRequestObjectResult("Please pass a name on the query string or in the request body");
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

        private static LocatorsInputModel GetInputModel(HttpRequest req)
        {
            List<string> liveEvents = req.Query[EventsQuery]
                .ToString()
                .Split(',')
                .Where(x => !string.IsNullOrEmpty(x.Trim()))
                .ToList();

            return new LocatorsInputModel()
            {
                LiveEvents = liveEvents
            };
        }

        private static async Task FetchLocatorsAsync(LocatorsInputModel locatorsList)
        {
            ConfigurationModel config = configService.GetConfiguration();

            // 1. Authenticate with Azure
            AzureMediaServicesClient client = await authService.GetClientAsync();

            foreach (string liveEventName in locatorsList.LiveEvents)
            {
                // 2. Get the Live Output
                IPage<LiveOutput> liveOutputsPage = await client.LiveOutputs.ListAsync(
                    resourceGroupName: config.ResourceGroup,
                    accountName: config.AccountName,
                    liveEventName: liveEventName
                );

                LiveOutput firstLiveOutput = liveOutputsPage.ToList().First();

                // 3. Fetch the Locators
                ListStreamingLocatorsResponse locatorResponse = await client.Assets.ListStreamingLocatorsAsync(
                    resourceGroupName: config.ResourceGroup,
                    accountName: config.AccountName,
                    assetName: firstLiveOutput.AssetName
                );

                /*Locators locators = locatorResponse
                    .StreamingLocators
                    .Select(locator =>
                    {
                        locator.
                    });

                LocatorsOutputModel output = new LocatorsOutputModel()
                {
                    LiveEventName = liveEventName,
                    Locators = 
                };*/
            }
    }
}
