using VW.Api.RestClient.Models;

namespace VW.Api.RestClient
{
    /// <summary>
    /// The logging service Interface 
    /// </summary>
    public interface ILoggingService
    {
        /// <summary>
        /// Logs the API request.
        /// </summary>
        /// <param name="trackingInfo">The tracking information.</param>
        void LogRequest(ApiTrackingInfo trackingInfo);

    }
}