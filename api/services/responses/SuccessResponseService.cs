using LiteralLifeChurch.LiveStreamingApi.models.output;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.Text;

namespace LiteralLifeChurch.LiveStreamingApi.services.responses
{
    class SuccessResponseService<Output> : IResponseService where Output : IOutputModel
    {
        public HttpResponseMessage CreateResponse(Output response, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            string serializedJson = JsonConvert.SerializeObject(response);

            return new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(serializedJson, Encoding.UTF8, "application/json")
            };
        }
    }
}
