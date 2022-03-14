using LiteralLifeChurch.LiveStreamingApi.Models.Output;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LiteralLifeChurch.LiveStreamingApi.Services.Responses
{
    public static class SuccessResponseService
    {
        public static IActionResult CreateResponse<Output>(Output output) where Output : IOutputModel
        {
            return CreateResponse(output, StatusCodes.Status200OK);
        }

        public static IActionResult CreateResponse<Output>(Output output, int statusCode) where Output : IOutputModel
        {
            ObjectResult result = new(output);
            result.StatusCode = statusCode;
            return result;
        }
    }
}
