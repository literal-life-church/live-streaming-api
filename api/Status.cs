using LiteralLifeChurch.LiveStreamingApi.controllers;
using LiteralLifeChurch.LiveStreamingApi.exceptions;
using LiteralLifeChurch.LiveStreamingApi.models.bootstrapping;
using LiteralLifeChurch.LiveStreamingApi.models.input;
using LiteralLifeChurch.LiveStreamingApi.models.output;
using LiteralLifeChurch.LiveStreamingApi.services;
using LiteralLifeChurch.LiveStreamingApi.services.responses;
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
    public static class Status
    {
        private static readonly AuthenticationService authService = new AuthenticationService();
        private static readonly ConfigurationService configService = new ConfigurationService();
        private static readonly ErrorResponseService errorResponseService = new ErrorResponseService();
        private static readonly SuccessResponseService<StatusOutputModel> successResponseService = new SuccessResponseService<StatusOutputModel>();

        [FunctionName("Status")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "status")] HttpRequest req,
            ILogger log)
        {
            try
            {
                AzureMediaServicesClient client = await authService.GetClientAsync();
                ConfigurationModel config = configService.GetConfiguration();

                InputRequestService inputRequestService = new InputRequestService(client, config);
                StatusController statusController = new StatusController(client, config);

                InputRequestModel inputModel = await inputRequestService.GetInputRequestModel(req);
                StatusOutputModel outputModel = await statusController.GetStatus(inputModel);
                return successResponseService.CreateResponse(outputModel);
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
