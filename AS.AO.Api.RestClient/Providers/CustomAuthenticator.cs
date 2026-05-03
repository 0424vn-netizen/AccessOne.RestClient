using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Linq;

namespace AS.AO.Api.RestClient.Providers
{
    /// <summary>
    /// The custom authenticator
    /// </summary>
    /// <seealso cref="RestSharp.Authenticators.IAuthenticator" />
    public class CustomAuthenticator : IAuthenticator
    {
        /// <summary>
        /// The authentication header
        /// </summary>
        private readonly string _authHeader;

        /// <summary>Initializes a new instance of the <see cref="T:AS.AO.Api.RestClient.Providers.CustomAuthenticator"/> class.</summary>
        /// <param name="authHeader"></param>
        /// <exception cref="ArgumentNullException">accessToken</exception>
        public CustomAuthenticator(string authHeader)
        {
            this._authHeader = authHeader ?? throw new ArgumentNullException(nameof(authHeader));
        }

        /// <summary>
        /// Authenticates the specified client.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="request">The request.</param>
        public void Authenticate(IRestClient client, IRestRequest request)
        {
            if (!request.Parameters.Any((Parameter p) =>
            {
                if (!p.Type.Equals(ParameterType.HttpHeader)) { return false; }
                return p.Name.Equals("Authorization", StringComparison.OrdinalIgnoreCase);
            }))
            {
                request.AddParameter("Authorization", this._authHeader, ParameterType.HttpHeader);
            }
        }
    }
}
