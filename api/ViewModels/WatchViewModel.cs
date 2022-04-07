using LiteralLifeChurch.LiveStreamingApi.Domain.UseCases;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace LiteralLifeChurch.LiveStreamingApi.ViewModels
{
    public class WatchViewModel : IWatchViewModel
    {
        private readonly GetConfigurationUseCase GetConfigurationUseCase;
        private readonly ILogger<WatchViewModel> Logger;
        private readonly TelemetryClient TelemetryClient;

        public WatchViewModel()
        {
/*            GetConfigurationUseCase = getConfigurationUseCase;
            Logger = logger;
            TelemetryClient = new TelemetryClient(telemetryConfiguration);*/
        }

        public async Task<IActionResult> WatchDefault()
        {
            return new OkObjectResult("Properly wired");
        }
    }
}
