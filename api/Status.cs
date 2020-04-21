using LiteralLifeChurch.LiveStreamingApi.controllers;
using LiteralLifeChurch.LiveStreamingApi.exceptions;
using LiteralLifeChurch.LiveStreamingApi.models;
using LiteralLifeChurch.LiveStreamingApi.models.input;
using LiteralLifeChurch.LiveStreamingApi.models.output;
using LiteralLifeChurch.LiveStreamingApi.services;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace LiteralLifeChurch.LiveStreamingApi
{
    public static class Status
    {
        private static readonly InputRequestService inputRequestService = new InputRequestService();
        private static readonly StatusController statusController = new StatusController();

        [FunctionName("Status")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "status")] HttpRequest req,
            ILogger log)
        {
            try
            {
                InputRequestModel inputModel = await inputRequestService.GetInputRequestModel(req);
                StatusOutputModel outputModel = await statusController.GetStatus(inputModel);
                return CreateSuccess(outputModel);
            }
            catch (BaseException e)
            {
                return CreateError(e.Message);
            }
            catch (Exception e)
            {
                return CreateError(e.Message);
            }
        }

        private static HttpResponseMessage CreateSuccess(StatusOutputModel outputModel)
        {
            string successJson = JsonConvert.SerializeObject(outputModel);

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(successJson, Encoding.UTF8, "application/json")
            };
        }

        private static HttpResponseMessage CreateError(string message)
        {
            ErrorModel error = new ErrorModel()
            {
                Message = message
            };

            string errorJson = JsonConvert.SerializeObject(error);

            return new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent(errorJson, Encoding.UTF8, "application/json")
            };
        }
    }
}
