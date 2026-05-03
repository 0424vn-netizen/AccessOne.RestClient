using AS.AO.Api.RestClient.Models;
using RestSharp.Authenticators;

namespace AS.AO.Api.RestClient
{
    public interface IAuthenticatorProvider
    {
        /// <summary>
        /// Gets the authenticator.
        /// </summary>
        /// <param name="apiSetting">The API setting.</param>
        /// <returns></returns>
        IAuthenticator GetAuthenticator(ApiSetting apiSetting);
    }
}