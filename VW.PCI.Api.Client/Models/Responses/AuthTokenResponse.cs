namespace VW.PCI.Api.Client.Models.Responses
{
    public class AuthTokenResponse
    {
        public string AccessToken { get; set; }
        public string TokenType { get; set; }
        public int ExpireMinutes { get; set; }
    }

    internal class AuthTokenApiResponse
    {
        public AuthTokenResponse Data { get; set; }
    }
}
