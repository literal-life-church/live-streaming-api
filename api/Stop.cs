using LiteralLifeChurch.LiveStreamingApi.controllers;
using LiteralLifeChurch.LiveStreamingApi.enums;
using LiteralLifeChurch.LiveStreamingApi.exceptions;
using LiteralLifeChurch.LiveStreamingApi.models.bootstrapping;
using LiteralLifeChurch.LiveStreamingApi.models.input;
using LiteralLifeChurch.LiveStreamingApi.models.output;
using LiteralLifeChurch.LiveStreamingApi.services;
using LiteralLifeChurch.LiveStreamingApi.services.common;
using LiteralLifeChurch.LiveStreamingApi.services.responses;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Management.Media;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace LiteralLifeChurch.LiveStreamingApi
{
    public class Stop
    {
        private readonly TelemetryClient TelemetryClient;

        public Stop(TelemetryConfiguration telemetryConfiguration)
        {
            TelemetryClient = new TelemetryClient(telemetryConfiguration);
        }

        [FunctionName("Stop")]
        public async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "stop")] HttpRequest req,
            ILogger log)
        {
            TelemetryClient.TrackEvent("Stop");

            using (LoggerService.Init(log))
            {
                ConfigurationModel config = ConfigurationService.GetConfiguration();

                try
                {
                    AzureMediaServicesClient client = await AuthenticationService.GetClientAsync(config);

                    InputRequestService inputRequestService = new InputRequestService(client, config);
                    StopController stopController = new StopController(client, config);

                    InputRequestModel inputModel = await inputRequestService.GetInputRequestModelAsync(req);
                    StatusChangeOutputModel outputModel = await stopController.StopServicesAsync(inputModel);

                    await WebhookService.CallWebhookAsync(config.WebhookStartSuccess, ActionEnum.Stop, outputModel.Status.Summary.Name);
                    return SuccessResponseService.CreateResponse(outputModel);
                }
                catch (AppException e)
                {
                    return await ReportErrorAsync(config, e);
                }
                catch (Exception e)
                {
                    return await ReportErrorAsync(config, e);
                }
            }
        }

        private static async Task<HttpResponseMessage> ReportErrorAsync(ConfigurationModel config, Exception exception)
        {
            LoggerService.CaptureException(exception);
            await WebhookService.CallWebhookAsync(config.WebhookStartFailure, ActionEnum.Start, ResourceStatusEnum.Error);
            return ErrorResponseService.CreateResponse(exception);
        }
    }
}
