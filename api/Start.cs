using LiteralLifeChurch.LiveStreamingApi.controllers;
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

        [FunctionName("Start")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "start")] HttpRequest req,
            ILogger log)
        {
            try
            {
                AzureMediaServicesClient client = await authService.GetClientAsync();
                ConfigurationModel config = configService.GetConfiguration();

                InputRequestService inputRequestService = new InputRequestService(client, config);
                StartController startController = new StartController(client, config);

                InputRequestModel inputModel = await inputRequestService.GetInputRequestModelAsync(req);
                StatusChangeOutputModel outputModel = await startController.StartServicesAsync(inputModel);

                return successResponseService.CreateResponse(outputModel, HttpStatusCode.Created);
            }
            catch (AppException e)
            {
                return errorResponseService.CreateResponse(e);
            }
            catch (Exception e)
            {
                return errorResponseService.CreateResponse(e);
            }
        }
    }
}
