using LiteralLifeChurch.LiveStreamingApi.Controllers;
using LiteralLifeChurch.LiveStreamingApi.Models.Bootstrapping;
using LiteralLifeChurch.LiveStreamingApi.Models.Input;
using LiteralLifeChurch.LiveStreamingApi.Models.Output;
using LiteralLifeChurch.LiveStreamingApi.Services;
using LiteralLifeChurch.LiveStreamingApi.Services.Common;
using LiteralLifeChurch.LiveStreamingApi.Services.Responses;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Management.Media;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace LiteralLifeChurch.LiveStreamingApi
{
    public class Status
    {
        private readonly TelemetryClient TelemetryClient;

        public Status(TelemetryConfiguration telemetryConfiguration)
        {
            TelemetryClient = new TelemetryClient(telemetryConfiguration);
        }

        [FunctionName("Status")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "broadcaster")] HttpRequest request,
            ILogger logger)
        {
            TelemetryClient.TrackEvent("Status");

            using (LoggerService.Init(logger))
            {
                try
                {
                    ConfigurationModel config = ConfigurationService.GetConfiguration();
                    AzureMediaServicesClient client = await AuthenticationService.GetClientAsync(config);

                    InputRequestService inputRequestService = new(client, config);
                    StatusController statusController = new(client, config);

                    InputRequestModel inputModel = await inputRequestService.GetInputRequestModelAsync(request);
                    StatusOutputModel outputModel = await statusController.GetStatusAsync(inputModel);

                    return SuccessResponseService.CreateResponse(outputModel);
                }
                catch (Exception e)
                {
                    return ReportError(e);
                }
            }
        }

        private static IActionResult ReportError(Exception exception)
        {
            LoggerService.CaptureException(exception);
            return ErrorResponseService.CreateResponse(exception);
        }
    }
}
