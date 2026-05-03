namespace AS.AO.Api.RestClient.Models
{
    public class ApiToken
    {
        /// <summary>
        /// Gets or sets the token.
        /// </summary>
        // [JsonProperty(PropertyName = "access_token")]
        public string Access_token { get; set; }

        /// <summary>
        /// Gets or sets the token type.
        /// </summary>
        // [JsonProperty(PropertyName = "token_type")]
        public string Token_type { get; set; }

        /// <summary>
        /// Gets or sets the refresh token.
        /// </summary>
        // [JsonProperty(PropertyName = "refresh_token")]
        public string Refresh_token { get; set; }

        /// <summary>
        /// Gets or sets the expires in value.
        /// </summary>
        // [JsonProperty(PropertyName = "expires_in")]
        public int? Expires_in { get; set; }

        /// <summary>
        /// Gets or sets the user name.
        /// </summary>
        // [JsonProperty(PropertyName = "userName")]
        public string UserName { get; set; }


    }
}