using LiteralLifeChurch.LiveStreamingApi.Enums;
using LiteralLifeChurch.LiveStreamingApi.Exceptions;
using LiteralLifeChurch.LiveStreamingApi.Models.Output;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Specialized;
using System.Net;
using System.Threading.Tasks;
using System.Web;

namespace LiteralLifeChurch.LiveStreamingApi.Services.Common
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

            RestClient client = new();
            RestRequest request = CreateRequest(uri, action, status);

            RestResponse response = await client.ExecuteAsync(request);
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
            UriBuilder uriBuilder = new(uri);

            queryParams["action"] = model.Action.ToString().ToLower();
            queryParams["status"] = model.Status.ToString().ToLower();

            uriBuilder.Query = queryParams.ToString();
            return uriBuilder.Uri;
        }

        public static RestRequest CreateRequest(Uri uri, ActionEnum action, ResourceStatusEnum status)
        {
            WebhookOutputModel model = new()
            {
                Action = action,
                Status = status
            };

            uri = AddActionAndStatusToUri(uri, model);
            RestRequest request = new(uri, Method.Post);
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
