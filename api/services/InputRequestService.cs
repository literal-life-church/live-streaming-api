using LiteralLifeChurch.LiveStreamingApi.models.input;
using LiteralLifeChurch.LiveStreamingApi.services.validators;
using Microsoft.AspNetCore.Http;

namespace LiteralLifeChurch.LiveStreamingApi.services
{
    public class InputRequestService
    {
        private readonly RequestValidator RequestValidator;

        public InputRequestService(HttpRequest request)
        {
            RequestValidator = new RequestValidator(request);
        }

        public InputRequestModel GetInputRequestModel()
        {
            RequestValidator.Validate();
            return new InputRequestModel();
        }
    }
}
