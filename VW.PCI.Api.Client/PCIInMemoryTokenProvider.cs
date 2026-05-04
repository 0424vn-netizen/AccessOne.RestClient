using VW.Api.RestClient.Models;
using VW.PCI.Api.Client.Models.Common;
using VW.PCI.Api.Client.Models.Requests;

namespace VW.PCI.Api.Client
{
    /// <summary>
    /// Token provider lưu trong bộ nhớ (static).
    /// Phù hợp khi chạy single-instance hoặc không cần chia sẻ token giữa nhiều server.
    /// Token mất khi app restart.
    /// </summary>
    public class PCIInMemoryTokenProvider : PCITokenProvider
    {
        private static PCITokenInfo _cachedToken;

        public PCIInMemoryTokenProvider(AuthTokenRequest credentials, ILogger logger)
            : base(credentials, logger) { }

        protected override PCITokenInfo GetTokenFromDB() => _cachedToken;

        protected override void SaveTokenToDB(PCITokenInfo token) => _cachedToken = token;
    }
}
