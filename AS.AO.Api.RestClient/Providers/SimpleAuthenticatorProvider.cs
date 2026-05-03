using AS.AO.Api.RestClient.Models;
using RestSharp.Authenticators;
using System;

namespace AS.AO.Api.RestClient.Providers
{
    /// <summary>
    /// The simple authenticator provider
    /// </summary>
    /// <seealso cref="IAuthenticatorProvider" />
    public class SimpleAuthenticatorProvider : IAuthenticatorProvider
    {
        /// <summary>
        /// Gets the authenticator.
        /// </summary>
        /// <param name="apiSetting">The API setting.</param>
        /// <returns></returns>
        public IAuthenticator GetAuthenticator(ApiSetting apiSetting)
        {
            if (apiSetting == null ||
                apiSetting.AuthenticationInfo == null ||
                apiSetting.AuthenticationInfo.Mode == ApiAuthenticationMode.None)
            {
                return null;
            }
            
            switch (apiSetting.AuthenticationInfo.Mode)
            {
                case ApiAuthenticationMode.Basic:
                    return this.GetBasicAuthenticator(apiSetting);

                case ApiAuthenticationMode.Simple:
                    return this.GetSimpleAuthenticator(apiSetting);

                case ApiAuthenticationMode.Jwt:
                    return this.GetJwtAuthenticator(apiSetting);

                case ApiAuthenticationMode.Ntml:
                    return this.GetNtmlAuthenticator(apiSetting);

                case ApiAuthenticationMode.OAuth2:
                    return this.GetOAuth2Authenticator(apiSetting);

                case ApiAuthenticationMode.OAuth2UriQueryParameter:
                    return this.GetOAuth2UriQueryParameterAuthenticator(apiSetting);

                case ApiAuthenticationMode.Custom:
                    return this.GetCustomAuthenticator(apiSetting);

                default:
                    return null;
            }
        }

        /// <summary>
        /// Gets the custom authenticator.
        /// </summary>
        /// <param name="apiSetting">The API setting.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        protected virtual IAuthenticator GetCustomAuthenticator(ApiSetting apiSetting)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the basic authenticator.
        /// </summary>
        /// <param name="apiSetting">The API setting.</param>
        /// <returns></returns>
        protected virtual IAuthenticator GetBasicAuthenticator(ApiSetting apiSetting)
        {
            var settings = apiSetting.AuthenticationInfo.Settings;
            if (settings == null || settings.Count == 0)
            {
                return null;
            }

            var userName = settings["userName"].Value;
            var password = settings["password"].Value;

            return new HttpBasicAuthenticator(userName, password);
        }

        /// <summary>
        /// Gets the simple authenticator.
        /// </summary>
        /// <param name="apiSetting">The API setting.</param>
        /// <returns></returns>
        protected virtual IAuthenticator GetSimpleAuthenticator(ApiSetting apiSetting)
        {
            var settings = apiSetting.AuthenticationInfo.Settings;
            if (settings == null || settings.Count == 0)
            {
                return null;
            }

            var userName = settings["userName"].Value;
            var password = settings["password"].Value;
            var userNameKey = CommonUtils.GetValueOrDefault(settings, "userNameKey", null)?.Value;
            var passwordKey = CommonUtils.GetValueOrDefault(settings, "passwordKey", null)?.Value;

            return new SimpleAuthenticator(userNameKey ?? "username", userName, passwordKey ?? "password", password);
        }

        /// <summary>
        /// Gets the NTML authenticator.
        /// </summary>
        /// <param name="apiSetting">The API setting.</param>
        /// <returns></returns>
        protected virtual IAuthenticator GetNtmlAuthenticator(ApiSetting apiSetting)
        {
            var settings = apiSetting.AuthenticationInfo.Settings;
            if (settings == null || settings.Count == 0)
            {
                return null;
            }

            var userName = settings["userName"].Value;
            var password = settings["password"].Value;

            return new NtlmAuthenticator(userName, password);
        }

        /// <summary>
        /// Gets the JWT authenticator.
        /// </summary>
        /// <param name="apiSetting">The API setting.</param>
        /// <returns></returns>
        protected virtual IAuthenticator GetJwtAuthenticator(ApiSetting apiSetting)
        {
            var accessToken = GetJwtAccessToken(apiSetting);
            return new JwtAuthenticator(accessToken);
        }

        /// <summary>
        /// Gets the o auth2 authenticator.
        /// </summary>
        /// <param name="apiSetting">The API setting.</param>
        /// <returns></returns>
        protected virtual IAuthenticator GetOAuth2Authenticator(ApiSetting apiSetting)
        {
            var settings = apiSetting.AuthenticationInfo.Settings;
            if (settings == null || settings.Count == 0)
            {
                return null;
            }

            var tokenType = CommonUtils.GetValueOrDefault(settings, "tokenType", null)?.Value;
            var accessToken = GetOAuth2AccessToken(apiSetting);

            return string.IsNullOrWhiteSpace(tokenType) ? new OAuth2AuthorizationRequestHeaderAuthenticator(accessToken) :
                                                          new OAuth2AuthorizationRequestHeaderAuthenticator(accessToken, tokenType);
        }

        /// <summary>
        /// Gets the o auth2 URI query parameter authenticator.
        /// </summary>
        /// <param name="apiSetting">The API setting.</param>
        /// <returns></returns>
        protected virtual IAuthenticator GetOAuth2UriQueryParameterAuthenticator(ApiSetting apiSetting)
        {
            var settings = apiSetting.AuthenticationInfo.Settings;
            if (settings == null || settings.Count == 0)
            {
                return null;
            }

            var accessToken = GetOAuth2AccessToken(apiSetting);

            return new OAuth2UriQueryParameterAuthenticator(accessToken);
        }

        /// <summary>
        /// Gets the JWT access token.
        /// </summary>
        /// <param name="apiSetting">The API setting.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        protected virtual string GetJwtAccessToken(ApiSetting apiSetting)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the o auth2 access token.
        /// </summary>
        /// <param name="apiSetting">The API setting.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        protected virtual string GetOAuth2AccessToken(ApiSetting apiSetting)
        {
            throw new NotImplementedException();
        }
    }
}