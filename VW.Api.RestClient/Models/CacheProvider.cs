namespace VW.Api.RestClient.Models
{
    /// <summary>
    /// The cache provider
    /// </summary>
    public class CacheProvider
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the configuration.
        /// </summary>
        public string Configuration { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is default provider or not.
        /// </summary>
        public bool IsDefault { get; set; }
    }
}