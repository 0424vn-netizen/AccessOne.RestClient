namespace VW.Api.RestClient.Models
{
    /// <summary>
    /// The authentication type
    /// </summary>
    public enum ApiAuthenticationMode
    {
        /// <summary>
        /// The none
        /// </summary>
        None = 0,

        /// <summary>
        /// The basic
        /// </summary>
        Basic = 1,

        /// <summary>
        /// The simple
        /// </summary>
        Simple = 2,

        /// <summary>
        /// The JWT
        /// </summary>
        Jwt = 3,

        /// <summary>
        /// The NTML
        /// </summary>
        Ntml = 4,

        /// <summary>
        /// The o auth2
        /// </summary>
        OAuth2 = 5,

        /// <summary>
        /// The o auth2 URI query parameter
        /// </summary>
        OAuth2UriQueryParameter = 6,

        /// <summary>
        /// The custom
        /// </summary>
        Custom = 99

    }
}
