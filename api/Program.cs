using Microsoft.Extensions.Hosting;

namespace LiteralLifeChurch.LiveStreamingApi
{
    public class Program
    {
        public static void Main()
        {
            var host = new HostBuilder()
                .ConfigureFunctionsWorkerDefaults()
                .Build();

            host.Run();
        }
    }
}
