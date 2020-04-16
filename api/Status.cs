using LiteralLifeChurch.LiveStreamingApi.models;
using LiteralLifeChurch.LiveStreamingApi.services;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace LiteralLifeChurch.LiveStreamingApi
{
    public static class Status
    {
        private static readonly AuthenticationService authService = new AuthenticationService();
        private static readonly ConfigurationService configService = new ConfigurationService();

        [FunctionName("Status")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "status")] HttpRequest req,
            ILogger log)
        {
            return CreateSuccess("Shut down");
        }

        private static HttpResponseMessage CreateSuccess(string message)
        {
            SuccessModel success = new SuccessModel()
            {
                Message = message
            };

            string successJson = JsonConvert.SerializeObject(success);

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(successJson, Encoding.UTF8, "application/json")
            };
        }
    }
}
