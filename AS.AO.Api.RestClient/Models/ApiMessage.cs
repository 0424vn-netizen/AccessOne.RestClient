using System;

namespace AS.AO.Api.RestClient.Models
{
    /// <summary>
    /// The API Response Message Info
    /// </summary>
    [Serializable]
    public class ApiMessage
    {
        /// <summary>
        /// Gets or sets the code.
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the message code.
        /// </summary>
        public int MessageCode { get; set; }
    }
}