using LiteralLifeChurch.LiveStreamingApi.Exceptions;
using LiteralLifeChurch.LiveStreamingApi.Models.Output;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;

namespace LiteralLifeChurch.LiveStreamingApi.Services.Responses
{
    public static class ErrorResponseService
    {
        public static IActionResult CreateResponse(Exception error)
        {
            if (error is AppException)
            {
                AppException appException = error as AppException;

                return new BadRequestObjectResult(new ErrorModel
                {
                    DeveloperMessage = appException.DeveloperMessage,
                    Message = appException.Message,
                    Status = appException.Status,
                    Type = appException.GetType().Name
                });
            }
            else
            {
                ObjectResult result = new(new ErrorModel
                {
                    DeveloperMessage = error.Message,
                    Message = error.Message,
                    Status = HttpStatusCode.InternalServerError,
                    Type = error.GetType().Name
                });

                result.StatusCode = StatusCodes.Status500InternalServerError;
                return result;
            }
        }
    }
}
