using System.Collections.Generic;

namespace VW.Api.RestClient.Models
{
    /// <summary>
    /// The cache settings
    /// </summary>
    public class CacheSettings
    {
        /// <summary>
        /// Gets or sets the expires in minutes.
        /// </summary>
        public int ExpiresInMinutes { get; set; }

        /// <summary>
        /// Gets or sets the name of the application instance.
        /// </summary>
        public string AppInstanceName { get; set; }

        /// <summary>
        /// Gets or sets the default.
        /// </summary>
        public CacheProvider Default { get; set; }

        /// <summary>
        /// Gets or sets the providers.
        /// </summary>
        public List<CacheProvider> Providers { get; set; }

    }
}