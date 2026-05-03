using System.Collections.Generic;

namespace VW.Api.RestClient.Models
{
    /// <summary>
    /// The data masking setting
    /// </summary>
    public class DataMaskingSetting
    {
        /// <summary>
        /// Gets or sets the properties.
        /// </summary>
        public IDictionary<string, DataMaskingType> Properties { get; set; }

        /// <summary>
        /// Gets or sets the patterns.
        /// </summary>
        public IDictionary<string, DataMaskingType> Patterns { get; set; }

    }
}