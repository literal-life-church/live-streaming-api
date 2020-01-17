using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Rest.Azure.Authentication;
using LiteralLifeChurch.LiveStreamingApi.models;
using LiteralLifeChurch.LiveStreamingApi.services;
using Microsoft.Azure.Management.Media;
using Microsoft.Rest;

namespace LiteralLifeChurch.LiveStreamingApi
{
    public static class Start
    {
        [FunctionName("Start")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            ConfigurationService configService = new ConfigurationService();
            ConfigurationModel config = configService.GetConfiguration();

            ClientCredential clientCredentials = new ClientCredential(config.ClientId, config.ClientSecret);
            ServiceClientCredentials appCredentials = await ApplicationTokenProvider.LoginSilentAsync(config.TenantId, clientCredentials, ActiveDirectoryServiceSettings.Azure);

            var client =  new AzureMediaServicesClient(config.ManagementEndpoint, appCredentials)
            {
                SubscriptionId = config.SubscriptionId,
            };

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;

            return name != null
                ? (ActionResult)new OkObjectResult($"Hello, {name}")
                : new BadRequestObjectResult("Please pass a name on the query string or in the request body");
        }
    }
}
