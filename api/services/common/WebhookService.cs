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
    public static class WebhookService
    {
        public static async Task CallWebhookAsync(Uri uri, ActionEnum action, ResourceStatusEnum status)
        {
            if (uri == null)
            {
                LoggerService.Warn("No URI for webhook call", LoggerService.Webhook);
                return;
            }

            RestClient client = new RestClient();
            RestRequest request = CreateRequest(uri, action, status);

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
            else
            {
                string message = string.Format("Called webhook for action {0}, and status {1}", action, status);
                LoggerService.Info(message, LoggerService.Webhook);
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

        public static RestRequest CreateRequest(Uri uri, ActionEnum action, ResourceStatusEnum status)
        {
            WebhookOutputModel model = new WebhookOutputModel
            {
                Action = action,
                Status = status
            };

            uri = AddActionAndStatusToUri(uri, model);
            RestRequest request = new RestRequest(uri, Method.POST);
            string payload = JsonConvert.SerializeObject(model);

            request.AddParameter("application/json", payload, ParameterType.RequestBody);
            request.RequestFormat = DataFormat.Json;

            return request;
        }

        private static bool Is2xxResponse(HttpStatusCode statusCode)
        {
            int underlyingStatusNumber = (int)statusCode;
            return 200 <= underlyingStatusNumber && underlyingStatusNumber <= 299;
        }
    }
}
