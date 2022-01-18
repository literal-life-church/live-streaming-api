using LiteralLifeChurch.LiveStreamingApi.Exceptions;
using Microsoft.Azure.Functions.Worker.Http;
using System.Linq;
using System.Web;

namespace LiteralLifeChurch.LiveStreamingApi.Services.Validators
{
    public static class InputValidator
    {
        private static readonly string EndpointQuery = "endpoint";
        private static readonly string EventsQuery = "events";

        public static void Validate(HttpRequestData request)
        {
            bool hasEndpoint = !string.IsNullOrEmpty(HttpUtility.ParseQueryString(request.Url.Query).Get(EndpointQuery)?.Trim());
            bool hasEvents = !string.IsNullOrEmpty(HttpUtility.ParseQueryString(request.Url.Query).Get(EventsQuery)?.Trim()) && HttpUtility
                .ParseQueryString(request.Url.Query)
                .Get(EventsQuery)
                .ToString()
                .Split(',')
                .Select(eventName => eventName.Trim())
                .Where(eventName => !string.IsNullOrEmpty(eventName))
                .Any();

            if (!hasEndpoint && !hasEvents)
            {
                throw new InputValidationException
                {
                    DeveloperMessage = "The query parameter 'endpoint' is required and must be the name of an existing streaming endpoint, and another query parameter 'events' is also required and must be a comma-separated list of names of existing live events",
                    Message = "Input requires the name of a streaming endpoint and the name of one or more live events"
                };
            }
            else if (!hasEndpoint)
            {
                throw new InputValidationException
                {
                    DeveloperMessage = "The query parameter 'endpoint' is required and must be the name of an existing streaming endpoint",
                    Message = "Input requires the name of a streaming endpoint"
                };
            }
            else if (!hasEvents)
            {
                throw new InputValidationException
                {
                    DeveloperMessage = "The query parameter 'events' is required and must be a comma-separated list of names of existing live events",
                    Message = "Input requires the name of one or more live events"
                };
            }
        }
    }
}
