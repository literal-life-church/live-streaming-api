using Microsoft.Extensions.Logging;
using Sentry;
using Sentry.Protocol;
using System;

namespace LiteralLifeChurch.LiveStreamingApi.services.common
{
    public class LoggerService : ICommonService
    {
        // region Categories

        public static readonly string Bootstrapping = "bootstrapping";
        public static readonly string Locators = "locators";
        public static readonly string Start = "start";
        public static readonly string Status = "start";
        public static readonly string Stop = "stop";
        public static readonly string Validation = "validation";
        public static readonly string Webhook = "webhook";

        // endregion

        private static ILogger Logger;

        public static IDisposable Init(ILogger log)
        {
            Logger = log;
            return SentrySdk.Init();
        }

        public static void CaptureException(Exception e)
        {
            SentrySdk.CaptureException(e);
            Logger.LogCritical(e.Message);
        }

        public static void Error(string message, string category)
        {
            SentrySdk.AddBreadcrumb(message, category, level: BreadcrumbLevel.Error);
            Logger.LogError(message);
        }

        public static void Info(string message, string category)
        {
            SentrySdk.AddBreadcrumb(message, category, level: BreadcrumbLevel.Info);
            Logger.LogInformation(message);
        }

        public static void Warn(string message, string category)
        {
            SentrySdk.AddBreadcrumb(message, category, level: BreadcrumbLevel.Warning);
            Logger.LogWarning(message);
        }
    }
}
