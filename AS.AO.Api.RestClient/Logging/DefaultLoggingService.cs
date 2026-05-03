using System.Text.RegularExpressions;
using AS.AO.Api.RestClient.Models;

namespace AS.AO.Api.RestClient.Logging
{
    /// <summary>
    /// The default logging service
    /// </summary>
    /// <seealso cref="ILoggingService" />
    public class DefaultLoggingService : ILoggingService
    {
        /// <summary>
        /// Gets or sets the logger.
        /// </summary>
        protected ILogger Logger { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultLoggingService"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public DefaultLoggingService(ILogger logger)
        {
            this.Logger = logger;
        }

        /// <summary>
        /// Logs the API request.
        /// </summary>
        /// <param name="trackingInfo">The tracking information.</param>
        public virtual void LogRequest(ApiTrackingInfo trackingInfo)
        {
            if (trackingInfo == null)
            {
                return;
            }

            var log = new
            {
                trackingInfo.TrackingId,
                trackingInfo.Source,
                trackingInfo.Resource,
                trackingInfo.Method,
                trackingInfo.RequestParameters,
                trackingInfo.RequestHeaders,
                trackingInfo.ResponseStatusCode,
                trackingInfo.ResponseHeaders,
                trackingInfo.ResponseContent,
                trackingInfo.ErrorMessage,
                trackingInfo.Duration
            };

            var value = Regex.Unescape(JsonSerializer.Serialize(log));
            this.Logger.Debug(value);
        }

    }
}