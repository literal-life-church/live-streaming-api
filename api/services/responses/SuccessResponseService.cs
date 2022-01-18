using LiteralLifeChurch.LiveStreamingApi.Models.Output;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;
using System.Threading.Tasks;

namespace LiteralLifeChurch.LiveStreamingApi.Services.Responses
{
    public static class SuccessResponseService
    {
        public static async Task<HttpResponseData> CreateResponse<Output>(HttpRequestData request, Output output) where Output : IOutputModel
        {
            return await CreateResponse(request, output, HttpStatusCode.OK);
        }

        public static async Task<HttpResponseData> CreateResponse<Output>(HttpRequestData request, Output output, HttpStatusCode statusCode) where Output : IOutputModel
        {
            HttpResponseData response = request.CreateResponse(statusCode);
            await response.WriteAsJsonAsync(output, statusCode);

            return response;
        }
    }
}
