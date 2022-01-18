using LiteralLifeChurch.LiveStreamingApi.Models.Bootstrapping;
using LiteralLifeChurch.LiveStreamingApi.Models.Input;
using LiteralLifeChurch.LiveStreamingApi.Services.Validators;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Management.Media;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace LiteralLifeChurch.LiveStreamingApi.Services.Common
{
    public class InputRequestService : ICommonService
    {
        private const string EndpointQuery = "endpoint";
        private const string EventsQuery = "events";
        private readonly ServiceValidator ServiceValidator;

        public InputRequestService(AzureMediaServicesClient client, ConfigurationModel config)
        {
            ServiceValidator = new ServiceValidator(client, config);
        }

        public async Task<InputRequestModel> GetInputRequestModelAsync(HttpRequestData request)
        {
            LoggerService.Info("Beginning validation", LoggerService.Validation);
            InputValidator.Validate(request);

            LoggerService.Info("Passed local validation", LoggerService.Validation);

            InputRequestModel model = new InputRequestModel
            {
                LiveEvents = HttpUtility
                    .ParseQueryString(request.Url.Query)
                    .Get(EventsQuery)
                    .ToString()
                    .Split(',')
                    .Select(eventName => eventName.Trim())
                    .Where(eventName => !string.IsNullOrEmpty(eventName))
                    .ToList(),

                StreamingEndpoint = HttpUtility
                    .ParseQueryString(request.Url.Query)
                    .Get(EndpointQuery)
                    .ToString()
                    .Trim()
            };

            await ServiceValidator.ValidateAsync(model);
            LoggerService.Info("Passed remote validation", LoggerService.Validation);

            return model;
        }
    }
}
