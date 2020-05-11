using LiteralLifeChurch.LiveStreamingApi.controllers;
using LiteralLifeChurch.LiveStreamingApi.enums;
using LiteralLifeChurch.LiveStreamingApi.exceptions;
using LiteralLifeChurch.LiveStreamingApi.models.bootstrapping;
using LiteralLifeChurch.LiveStreamingApi.models.input;
using LiteralLifeChurch.LiveStreamingApi.models.output;
using LiteralLifeChurch.LiveStreamingApi.services;
using LiteralLifeChurch.LiveStreamingApi.services.common;
using LiteralLifeChurch.LiveStreamingApi.services.responses;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Management.Media;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace LiteralLifeChurch.LiveStreamingApi
{
    public static class Start
    {
        private static readonly AuthenticationService authService = new AuthenticationService();
        private static readonly ConfigurationService configService = new ConfigurationService();
        private static readonly ErrorResponseService errorResponseService = new ErrorResponseService();
        private static readonly SuccessResponseService<StatusChangeOutputModel> successResponseService = new SuccessResponseService<StatusChangeOutputModel>();
        private static readonly WebhookService webhookService = new WebhookService();

        [FunctionName("Start")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "start")] HttpRequest req,
            ILogger log)
        {
            ConfigurationModel config = configService.GetConfiguration();

            try
            {
                AzureMediaServicesClient client = await authService.GetClientAsync();

                InputRequestService inputRequestService = new InputRequestService(client, config);
                StartController startController = new StartController(client, config);

                InputRequestModel inputModel = await inputRequestService.GetInputRequestModelAsync(req);
                StatusChangeOutputModel outputModel = await startController.StartServicesAsync(inputModel);

                await webhookService.CallWebhookAsync(config.WebhookStartSuccess, ActionEnum.Start, outputModel.Status.Summary);
                return successResponseService.CreateResponse(outputModel, HttpStatusCode.Created);
            }
            catch (AppException e)
            {
                await webhookService.CallWebhookAsync(config.WebhookStartFailure, ActionEnum.Start, ResourceStatusEnum.Error);
                return errorResponseService.CreateResponse(e);
            }
            catch (Exception e)
            {
                await webhookService.CallWebhookAsync(config.WebhookStartFailure, ActionEnum.Start, ResourceStatusEnum.Error);
                return errorResponseService.CreateResponse(e);
            }
        }
    }
}
