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
using System.Net.Http;
using System.Threading.Tasks;

namespace LiteralLifeChurch.LiveStreamingApi
{
    public static class Status
    {
        private static readonly ConfigurationService configService = new ConfigurationService();
        private static readonly ErrorResponseService errorResponseService = new ErrorResponseService();
        private static readonly SuccessResponseService<StatusOutputModel> successResponseService = new SuccessResponseService<StatusOutputModel>();

        [FunctionName("Status")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "status")] HttpRequest req,
            ILogger log)
        {
            using (LoggerService.Init(log))
            {
                try
                {
                    ConfigurationModel config = configService.GetConfiguration();
                    AzureMediaServicesClient client = await AuthenticationService.GetClientAsync(config);

                    InputRequestService inputRequestService = new InputRequestService(client, config);
                    StatusController statusController = new StatusController(client, config);

                    InputRequestModel inputModel = await inputRequestService.GetInputRequestModelAsync(req);
                    StatusOutputModel outputModel = await statusController.GetStatusAsync(inputModel);

                    return successResponseService.CreateResponse(outputModel);
                }
                catch (AppException e)
                {
                    return ReportError(e);
                }
                catch (Exception e)
                {
                    return ReportError(e);
                }
            }
        }

        private static HttpResponseMessage ReportError(Exception exception)
        {
            LoggerService.CaptureException(exception);
            return errorResponseService.CreateResponse(exception);
        }
    }
}
