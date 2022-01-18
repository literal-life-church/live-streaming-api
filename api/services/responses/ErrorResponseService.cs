using LiteralLifeChurch.LiveStreamingApi.Exceptions;
using LiteralLifeChurch.LiveStreamingApi.Models.Output;
using Microsoft.Azure.Functions.Worker.Http;
using System;
using System.Net;
using System.Threading.Tasks;

namespace LiteralLifeChurch.LiveStreamingApi.Services.Responses
{
    public static class ErrorResponseService
    {
        public static async Task<HttpResponseData> CreateResponse(HttpRequestData request, Exception error)
        {
            ErrorModel model;

            if (error is AppException)
            {
                AppException appException = error as AppException;

                model = new ErrorModel
                {
                    DeveloperMessage = appException.DeveloperMessage,
                    Message = appException.Message,
                    Status = appException.Status,
                    Type = appException.GetType().Name
                };
            }
            else
            {
                model = new ErrorModel
                {
                    DeveloperMessage = error.Message,
                    Message = error.Message,
                    Status = HttpStatusCode.InternalServerError,
                    Type = error.GetType().Name
                };
            }

            HttpResponseData response = request.CreateResponse(model.Status);
            await response.WriteAsJsonAsync(model, model.Status);

            return response;
        }
    }
}
