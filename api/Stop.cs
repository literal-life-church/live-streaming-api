using LiteralLifeChurch.LiveStreamingApi.Controllers;
using LiteralLifeChurch.LiveStreamingApi.Enums;
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
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "broadcaster")] HttpRequest req,
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
