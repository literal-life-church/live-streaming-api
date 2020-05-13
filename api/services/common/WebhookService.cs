using LiteralLifeChurch.LiveStreamingApi.enums;
using LiteralLifeChurch.LiveStreamingApi.exceptions;
using LiteralLifeChurch.LiveStreamingApi.models.output;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Specialized;
using System.Net;
using System.Threading.Tasks;
using System.Web;

namespace LiteralLifeChurch.LiveStreamingApi.services.common
{
    public class WebhookService : IService
    {
        public async Task CallWebhookAsync(Uri uri, ActionEnum action, ResourceStatusEnum status)
        {
            if (uri == null)
            {
                return;
            }

            WebhookOutputModel model = new WebhookOutputModel
            {
                Action = action,
                Status = status
            };

            uri = AddActionAndStatusToUri(uri, model);

            RestClient client = new RestClient();
            RestRequest request = new RestRequest(uri, Method.POST);
            string payload = JsonConvert.SerializeObject(model);

            request.AddParameter("application/json", payload, ParameterType.RequestBody);
            request.RequestFormat = DataFormat.Json;

            IRestResponse response = await client.ExecuteAsync(request);
            bool isSuccessful = Is2xxResponse(response.StatusCode);

            if (!isSuccessful)
            {
                throw new WebhookResponseException
                {
                    DeveloperMessage = "A call to the provided webhook returned a response other than 200 - 299",
                    Message = "Unable to make a call out to the provided webhook"
                };
            }
        }

        private static Uri AddActionAndStatusToUri(Uri uri, WebhookOutputModel model)
        {
            NameValueCollection queryParams = HttpUtility.ParseQueryString(uri.Query);
            UriBuilder uriBuilder = new UriBuilder(uri);

            queryParams["action"] = model.Action.ToString().ToLower();
            queryParams["status"] = model.Status.ToString().ToLower();

            uriBuilder.Query = queryParams.ToString();
            return uriBuilder.Uri;
        }

        private static bool Is2xxResponse(HttpStatusCode statusCode)
        {
            int underlyingStatusNumber = (int)statusCode;
            return 200 <= underlyingStatusNumber && underlyingStatusNumber <= 299;
        }
    }
}
