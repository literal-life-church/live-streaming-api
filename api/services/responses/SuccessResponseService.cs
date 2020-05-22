using LiteralLifeChurch.LiveStreamingApi.models.output;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.Text;

namespace LiteralLifeChurch.LiveStreamingApi.services.responses
{
    public static class SuccessResponseService
    {
        public static HttpResponseMessage CreateResponse<Output>(Output response, HttpStatusCode statusCode = HttpStatusCode.OK) where Output : IOutputModel
        {
            string serializedJson = JsonConvert.SerializeObject(response);

            return new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(serializedJson, Encoding.UTF8, "application/json")
            };
        }
    }
}
