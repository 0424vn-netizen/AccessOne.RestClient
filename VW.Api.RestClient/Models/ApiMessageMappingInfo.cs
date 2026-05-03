namespace VW.Api.RestClient.Models
{
    /// <summary>
    /// The API message mapping information
    /// </summary>
    public class ApiMessageMappingInfo
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
        /// Gets or sets the return message code.
        /// </summary>
        public int RemapMessageCode { get; set; }

        /// <summary>
        /// Gets or sets the type of the return message.
        /// </summary>
        public string RemapMessageType { get; set; }

        /// <summary>
        /// Gets or sets the source.
        /// </summary>
        public string Source { get; set; }

    }
}