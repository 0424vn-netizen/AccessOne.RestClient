using VW.Api.RestClient;
using VW.Api.RestClient.Models;

namespace VW.PCI.Api.Client.Sample.Infrastructure
{
    /// <summary>
    /// Logging service mẫu — ghi tracking info ra logger.
    /// Thực tế có thể ghi vào DB, Elasticsearch, Azure AppInsights...
    /// </summary>
    public class SampleLoggingService : ILoggingService
    {
        private readonly ILogger _logger;

        public SampleLoggingService(ILogger logger)
        {
            _logger = logger;
        }

        public void Log(ApiTrackingInfo trackingInfo)
        {
            if (trackingInfo == null) return;
            _logger.Debug($"[TRACKING] Id={trackingInfo.TrackingId} | " +
                          $"URL={trackingInfo.Url} | " +
                          $"Status={trackingInfo.ResponseStatusCode} | " +
                          $"Duration={trackingInfo.Duration}ms");
        }
    }
}
