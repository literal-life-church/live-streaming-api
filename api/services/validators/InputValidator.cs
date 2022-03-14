using LiteralLifeChurch.LiveStreamingApi.Exceptions;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace LiteralLifeChurch.LiveStreamingApi.Services.Validators
{
    public static class InputValidator
    {
        private static readonly string EndpointQuery = "endpoint";
        private static readonly string EventsQuery = "events";

        public static void Validate(HttpRequest request)
        {
            bool hasEndpoint = request.Query.ContainsKey(EndpointQuery) && !string.IsNullOrEmpty(request.Query[EndpointQuery].ToString().Trim());
            bool hasEvents = request.Query.ContainsKey(EventsQuery) && !string.IsNullOrEmpty(request.Query[EventsQuery].ToString().Trim()) && request.Query[EventsQuery]
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
