using LiteralLifeChurch.LiveStreamingApi.models.bootstrapping;
using LiteralLifeChurch.LiveStreamingApi.models.input;
using LiteralLifeChurch.LiveStreamingApi.services.validators;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Management.Media;
using Sentry;
using Sentry.Protocol;
using System.Linq;
using System.Threading.Tasks;

namespace LiteralLifeChurch.LiveStreamingApi.services.common
{
    public class InputRequestService : ICommonService
    {
        private const string EndpointQuery = "endpoint";
        private const string EventsQuery = "events";
        private readonly InputValidator InputValidator;
        private readonly ServiceValidator ServiceValidator;

        public InputRequestService(AzureMediaServicesClient client, ConfigurationModel config)
        {
            InputValidator = new InputValidator();
            ServiceValidator = new ServiceValidator(client, config);
        }

        public async Task<InputRequestModel> GetInputRequestModelAsync(HttpRequest request)
        {
            SentrySdk.AddBreadcrumb(message: "Beginning validation", category: "validation", level: BreadcrumbLevel.Info);
            InputValidator.Validate(request);

            SentrySdk.AddBreadcrumb(message: "Passed local validation", category: "validation", level: BreadcrumbLevel.Info);

            InputRequestModel model = new InputRequestModel()
            {
                LiveEvents = request.Query[EventsQuery]
                    .ToString()
                    .Split(',')
                    .Select(eventName => eventName.Trim())
                    .Where(eventName => !string.IsNullOrEmpty(eventName))
                    .ToList(),

                StreamingEndpoint = request.Query[EndpointQuery]
                    .ToString()
                    .Trim()
            };

            await ServiceValidator.ValidateAsync(model);
            SentrySdk.AddBreadcrumb(message: "Passed remote validation", category: "validation", level: BreadcrumbLevel.Info);

            return model;
        }
    }
}
