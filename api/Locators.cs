using LiteralLifeChurch.LiveStreamingApi.Controllers;
using LiteralLifeChurch.LiveStreamingApi.Exceptions;
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
    public class Locators
    {
        private readonly TelemetryClient TelemetryClient;

        public Locators(TelemetryConfiguration telemetryConfiguration)
        {
            TelemetryClient = new TelemetryClient(telemetryConfiguration);
        }

        [FunctionName("Locators")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "api/v1/viewer/locators")] HttpRequest request,
            ILogger logger)
        {
            TelemetryClient.TrackEvent("Locators");

            using (LoggerService.Init(logger))
            {
                try
                {
                    ConfigurationModel config = ConfigurationService.GetConfiguration();
                    AzureMediaServicesClient client = await AuthenticationService.GetClientAsync(config);

                    InputRequestService inputRequestService = new(client, config);
                    LocatorsController locatorsController = new(client, config);

                    InputRequestModel inputModel = await inputRequestService.GetInputRequestModelAsync(request);
                    LocatorsOutputModel outputModel = await locatorsController.GetLocatorsAsync(inputModel);

                    return new OkObjectResult(outputModel);
                }
                catch (AppException e)
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
