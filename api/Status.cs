using LiteralLifeChurch.LiveStreamingApi.Controllers;
using LiteralLifeChurch.LiveStreamingApi.Exceptions;
using LiteralLifeChurch.LiveStreamingApi.Models.Bootstrapping;
using LiteralLifeChurch.LiveStreamingApi.Models.Input;
using LiteralLifeChurch.LiveStreamingApi.Models.Output;
using LiteralLifeChurch.LiveStreamingApi.Services;
using LiteralLifeChurch.LiveStreamingApi.Services.Common;
using LiteralLifeChurch.LiveStreamingApi.Services.Responses;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Management.Media;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace LiteralLifeChurch.LiveStreamingApi
{
    public static class Status
    {
        [Function("Status")]
        public static async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "broadcaster")] HttpRequestData request,
            FunctionContext executionContext)
        {
            ILogger logger = executionContext.GetLogger("Status");

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

                    return await SuccessResponseService.CreateResponse(request, outputModel);
                }
                catch (AppException e)
                {
                    return await ReportError(request, e);
                }
                catch (Exception e)
                {
                    return await ReportError(request, e);
                }
            }
        }

        private static async Task<HttpResponseData> ReportError(HttpRequestData request, Exception exception)
        {
            LoggerService.CaptureException(exception);
            return await ErrorResponseService.CreateResponse(request, exception);
        }
    }
}
