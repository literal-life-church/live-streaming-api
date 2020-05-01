using LiteralLifeChurch.LiveStreamingApi.models.bootstrapping;
using LiteralLifeChurch.LiveStreamingApi.models.input;
using LiteralLifeChurch.LiveStreamingApi.models.output;
using LiteralLifeChurch.LiveStreamingApi.services.common;
using Microsoft.Azure.Management.Media;
using System.Threading.Tasks;

namespace LiteralLifeChurch.LiveStreamingApi.controllers
{
    public class StatusController : IController
    {
        private readonly StatusService StatusService;

        public StatusController(AzureMediaServicesClient client, ConfigurationModel config)
        {
            StatusService = new StatusService(client, config);
        }

        public async Task<StatusOutputModel> GetStatusAsync(InputRequestModel input)
        {
            return await StatusService.GetStatusAsync(input);
        }
    }
}
