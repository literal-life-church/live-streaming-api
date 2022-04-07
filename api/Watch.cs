using LiteralLifeChurch.LiveStreamingApi.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using System.Threading.Tasks;

namespace LiteralLifeChurch.LiveStreamingApi
{
    public class Watch
    {
        private readonly IWatchViewModel WatchViewModel;

        public Watch(IWatchViewModel watchViewModel)
        {
            WatchViewModel = watchViewModel;
        }

        [FunctionName("WatchDefault")]
        public async Task<IActionResult> RunWatchDefault(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "watch")] HttpRequest req)
        {
            return await WatchViewModel.WatchDefault();
        }

        [FunctionName("WatchSpecifiedEvent")]
        public IActionResult RunWatchSpecifiedEvent(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "watch/{endpointName:alpha}/{eventName:alpha}")] HttpRequest req,
            string endpointName,
            string eventName)
        {
            return new OkObjectResult($"Watch Specified Event: {endpointName}, {eventName}");
        }

        [FunctionName("WatchSpecifiedEventAndStreamType")]
        public IActionResult RunWatchSpecifiedEventAndStreamType(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "watch/{endpointName:alpha}/{eventName:alpha}/{streamType:alpha}")] HttpRequest req,
            string endpointName,
            string eventName,
            string streamType)
        {
            return new OkObjectResult($"Watch Specified Event and Stream Type: {endpointName}, {eventName}, {streamType}");
        }
    }
}
