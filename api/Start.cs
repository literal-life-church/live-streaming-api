using LiteralLifeChurch.LiveStreamingApi.services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Management.Media;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LiteralLifeChurch.LiveStreamingApi
{
    public static class Start
    {
        private static readonly AuthenticationService auth = new AuthenticationService();

        [FunctionName("Start")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            AzureMediaServicesClient client = await auth.GetClientAsync();

            List<string> liveEvents = req.Query["events"]
                .ToString()
                .Split(',')
                .Where(x => !string.IsNullOrEmpty(x))
                .ToList();

            string streamingEndpoint = req.Query["endpoint"].ToString().Trim();

            if (!liveEvents.Any())
            {
                return new BadRequestObjectResult("No Live events");
            }

            if (!string.IsNullOrEmpty(streamingEndpoint))
            {
                return new BadRequestObjectResult("Missing a streaming endpoint");
            }

            return new OkObjectResult($"Hello");
        }
    }
}
