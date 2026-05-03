using VW.Api.RestClient.Providers;

namespace VW.Api.RestClient.Models
{
    /// <summary>
    /// The rest client settings
    /// </summary>
    public class RestClientSettings
    {
        //#region Statics

        ///// <summary>
        ///// The default rest client settings
        ///// </summary>
        //private static RestClientSettings _default;

        ///// <summary>
        ///// Gets or sets the default.
        ///// </summary>
        ///// <exception cref="ArgumentNullException">value</exception>
        //public static RestClientSettings Default
        //{
        //    get { return _default; }
        //    set
        //    {
        //        _default = value ?? throw new ArgumentNullException(nameof(value));
        //    }
        //}

        ///// <summary>
        ///// Initializes the <see cref="RestClientSettings"/> class.
        ///// </summary>
        //static RestClientSettings()
        //{
        //    _default = new RestClientSettings();
        //}

        //#endregion

        #region Properties

        /// <summary>
        /// Gets or sets the authenticator provider.
        /// </summary>
        public IAuthenticatorProvider AuthenticatorProvider { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="RestClientSettings" /> class.
        /// </summary>
        /// <param name="cacheManager">The cache manager.</param>
        /// <param name="logger">The logger.</param>
        public RestClientSettings()
        {
            this.AuthenticatorProvider = new SimpleAuthenticatorProvider();
        }

        #endregion

    }
}