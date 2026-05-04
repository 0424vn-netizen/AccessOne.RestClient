using System;
using VW.Api.RestClient;
using VW.Api.RestClient.Models;

namespace VW.PCI.Api.Client.Sample.Infrastructure
{
    public class SampleLoggingService : ILoggingService
    {
        private static readonly Lazy<SampleLoggingService> _lazy =
            new Lazy<SampleLoggingService>(() => new SampleLoggingService());

        public static SampleLoggingService Instance => _lazy.Value;

        private SampleLoggingService() { }

        public void Log(ApiTrackingInfo trackingInfo)
        {
            if (trackingInfo == null) return;
            SampleLogger.Instance.Debug(
                $"[TRACKING] Id={trackingInfo.TrackingId} | URL={trackingInfo.Url} | " +
                $"Status={trackingInfo.ResponseStatusCode} | Duration={trackingInfo.Duration}ms");
        }
    }
}
