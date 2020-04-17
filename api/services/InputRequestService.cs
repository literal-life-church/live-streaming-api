using LiteralLifeChurch.LiveStreamingApi.models.input;
using LiteralLifeChurch.LiveStreamingApi.services.validators;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Threading.Tasks;

namespace LiteralLifeChurch.LiveStreamingApi.services
{
    public class InputRequestService
    {
        private const string EndpointQuery = "endpoint";
        private const string EventsQuery = "events";
        private readonly HttpRequest Request;
        private readonly RequestValidator RequestValidator;
        private readonly ServiceValidator ServiceValidator;

        public InputRequestService(HttpRequest request)
        {
            Request = request;
            RequestValidator = new RequestValidator();
            ServiceValidator = new ServiceValidator();
        }

        public async Task<InputRequestModel> GetInputRequestModel()
        {
            RequestValidator.Validate(Request);

            InputRequestModel model = new InputRequestModel()
            {
                LiveEvents = Request.Query[EventsQuery]
                    .ToString()
                    .Split(',')
                    .Select(eventName => eventName.Trim())
                    .Where(eventName => !string.IsNullOrEmpty(eventName))
                    .ToList(),

                StreamingEndpoint = Request.Query[EndpointQuery]
                    .ToString()
                    .Trim()
            };

            await ServiceValidator.Validate(model);
            return model;
        }
    }
}
