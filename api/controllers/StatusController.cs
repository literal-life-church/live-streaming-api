using LiteralLifeChurch.LiveStreamingApi.Models.Bootstrapping;
using LiteralLifeChurch.LiveStreamingApi.Models.Input;
using LiteralLifeChurch.LiveStreamingApi.Models.Output;
using LiteralLifeChurch.LiveStreamingApi.Services.Common;
using Microsoft.Azure.Management.Media;
using System.Threading.Tasks;

namespace LiteralLifeChurch.LiveStreamingApi.Controllers
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
