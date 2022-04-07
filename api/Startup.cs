using LiteralLifeChurch.LiveStreamingApi.ViewModels;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(LiteralLifeChurch.LiveStreamingApi.Startup))]

namespace LiteralLifeChurch.LiveStreamingApi
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddHttpClient();

            builder.Services.AddSingleton<IWatchViewModel>((service) => {
                return new WatchViewModel();
            });
        }
    }
}
