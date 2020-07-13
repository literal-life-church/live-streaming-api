using LiteralLifeChurch.LiveStreamingApi.Models.Output;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.Text;

namespace LiteralLifeChurch.LiveStreamingApi.Services.Responses
{
    public static class SuccessResponseService
    {
        public static HttpResponseMessage CreateResponse<Output>(Output response) where Output : IOutputModel
        {
            return CreateResponse(response, HttpStatusCode.OK);
        }

        public static HttpResponseMessage CreateResponse<Output>(Output response, HttpStatusCode statusCode) where Output : IOutputModel
        {
            string serializedJson = JsonConvert.SerializeObject(response);

            return new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(serializedJson, Encoding.UTF8, "application/json")
            };
        }
    }
}
