using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace LiteralLifeChurch.LiveStreamingApi
{
    public class Watch
    {
        private readonly TelemetryClient TelemetryClient;

        public Watch(TelemetryConfiguration telemetryConfiguration)
        {
            TelemetryClient = new TelemetryClient(telemetryConfiguration);
        }

        [FunctionName("WatchDefault")]
        public IActionResult RunWatchDefault(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "watch")] HttpRequest req,
            ILogger log)
        {
            TelemetryClient.TrackEvent("WatchDefault");
            return new OkObjectResult("Watch Default");
        }

        [FunctionName("WatchSpecifiedEvent")]
        public IActionResult RunWatchSpecifiedEvent(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "watch/{endpointName:alpha}/{eventName:alpha}")] HttpRequest req,
            string endpointName,
            string eventName,
            ILogger log)
        {
            TelemetryClient.TrackEvent("WatchSpecifiedEvent");
            return new OkObjectResult($"Watch Specified Event: {endpointName}, {eventName}");
        }

        [FunctionName("WatchSpecifiedEventAndStreamType")]
        public IActionResult RunWatchSpecifiedEventAndStreamType(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "watch/{endpointName:alpha}/{eventName:alpha}/{streamType:alpha}")] HttpRequest req,
            string endpointName,
            string eventName,
            string streamType,
            ILogger log)
        {
            TelemetryClient.TrackEvent("WatchSpecifiedEventAndStreamType");
            return new OkObjectResult($"Watch Specified Event and Stream Type: {endpointName}, {eventName}, {streamType}");
        }
    }
}
