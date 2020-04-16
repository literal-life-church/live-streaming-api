using LiteralLifeChurch.LiveStreamingApi.exceptions;
using Microsoft.AspNetCore.Http;

namespace LiteralLifeChurch.LiveStreamingApi.services.validators
{
    public class RequestValidator
    {
        private const string EndpointQuery = "endpoint";
        private const string EventsQuery = "events";
        private readonly HttpRequest Request;

        public RequestValidator(HttpRequest request)
        {
            Request = request;
        }

        public void Validate()
        {
            BaseException exception = null;

            if (!Request.Query.ContainsKey(EndpointQuery) && !Request.Query.ContainsKey(EventsQuery))
            {
                exception = new InputValidationException()
                {
                    DeveloperMessage = "The query parameter 'endpoint' is required and must be the name of an existing streaming endpoint, and another query parameter 'events' is also required and must be a comma-separated list of names of existing live events",
                    Message = "Input requires the name of a streaming endpoint and the name of one or more live events"
                };
            }
            else if (Request.Query.ContainsKey(EndpointQuery))
            {
                exception = new InputValidationException()
                {
                    DeveloperMessage = "The query parameter 'endpoint' is required and must be the name of an existing streaming endpoint",
                    Message = "Input requires the name of a streaming endpoint"
                };
            }
            else if (Request.Query.ContainsKey(EventsQuery))
            {
                exception = new InputValidationException()
                {
                    DeveloperMessage = "The query parameter 'events' is required and must be a comma-separated list of names of existing live events",
                    Message = "Input requires the name of one or more live events"
                };
            }

            if (exception == null) return;
            throw exception;
        }
    }
}
