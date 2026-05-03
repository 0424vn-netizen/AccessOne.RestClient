using System;
using System.Net;

namespace AS.AO.Api.RestClient.Models
{
    /// <summary>
    /// The API exception
    /// </summary>
    /// <seealso cref="Exception" />
    [Serializable]
    public class ApiException : Exception
    {
        /// <summary>
        /// Gets or sets the HTTP status code.
        /// </summary>
        public HttpStatusCode StatusCode { get; set; }

        /// <summary>
        /// Gets or sets the tracking identifier.
        /// </summary>
        public string TrackingId { get; set; }

        /// <summary>
        /// ApiException
        /// </summary>
        /// <param name="trackingId">The tracking identifier.</param>
        /// <param name="message">The message.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="httpStatusCode">The HTTP status code.</param>
        public ApiException(string trackingId, string message, Exception exception, HttpStatusCode? httpStatusCode = null) : base(message, exception)
        {
            this.TrackingId = trackingId;
            this.StatusCode = httpStatusCode ?? HttpStatusCode.InternalServerError;
        }
    }
}