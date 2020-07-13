using LiteralLifeChurch.LiveStreamingApi.Exceptions;
using LiteralLifeChurch.LiveStreamingApi.Models.Output;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Text;

namespace LiteralLifeChurch.LiveStreamingApi.Services.Responses
{
    public static class ErrorResponseService
    {
        public static HttpResponseMessage CreateResponse(Exception error)
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

            string serializedJson = JsonConvert.SerializeObject(model);

            return new HttpResponseMessage(model.Status)
            {
                Content = new StringContent(serializedJson, Encoding.UTF8, "application/json")
            };
        }
    }
}
