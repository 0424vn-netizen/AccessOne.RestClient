using System;
using System.Net;

namespace AS.AO.Api.RestClient.Models
{
    /// <summary>
    /// The API request information
    /// </summary>
    [Serializable]
    public class ApiTrackingInfo
    {
        /// <summary>
        /// Gets or sets the tracking identifier.
        /// </summary>
        public string TrackingId { get; set; }

        /// <summary>
        /// Gets or sets the source.
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Gets or sets the resource.
        /// </summary>
        public string Resource { get; set; }

        /// <summary>
        /// Gets or sets the method.
        /// </summary>
        public string Method { get; set; }

        /// <summary>
        /// Gets or sets the request headers.
        /// </summary>
        public string RequestHeaders { get; set; }

        /// <summary>
        /// Gets or sets the request parameters.
        /// </summary>
        public string RequestParameters { get; set; }

        /// <summary>
        /// Gets or sets the status code.
        /// </summary>
        public HttpStatusCode ResponseStatusCode { get; set; }

        /// <summary>
        /// Gets or sets the request parameters.
        /// </summary>
        public string ResponseHeaders { get; set; }

        /// <summary>
        /// Gets or sets the content.
        /// </summary>
        public string ResponseContent { get; set; }

        /// <summary>
        /// Gets or sets the exception.
        /// </summary>
        public string Exception { get; set; }

        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the other info.
        /// </summary>
        public string OtherInfo { get; set; }

        /// <summary>
        /// Gets or sets the server ip address.
        /// </summary>
        public string ServerIPAddress { get; set; }

        /// <summary>
        /// Gets or sets the client ip address.
        /// </summary>
        public string ClientIPAddress { get; set; }

        /// <summary>
        /// Gets or sets the duration.
        /// </summary>
        public double? Duration { get; set; }
    }

    /// <summary>
    /// The API logging parameter information
    /// </summary>
    [Serializable]
    public class ApiTrackingParameter
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        public object Value { get; set; }
    }
}