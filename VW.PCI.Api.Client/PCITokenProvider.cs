using System;
using VW.Api.RestClient.Models;
using VW.PCI.Api.Client.Models.Common;
using VW.PCI.Api.Client.Models.Requests;
using VW.PCI.Api.Client.Models.Responses;

namespace VW.PCI.Api.Client
{
    /// <summary>
    /// Base class quản lý vòng đời PCI token.
    ///
    /// Luồng hoạt động:
    ///   1. GetTokenFromDB()  → lấy token đang lưu
    ///   2. token.IsValid()?  → còn hạn (dựa vào expireMinutes) → dùng luôn
    ///   3. Hết hạn/chưa có  → gọi /auth/token API lấy token mới
    ///   4. SaveTokenToDB()   → lưu token mới vào storage
    ///
    /// Token refresh hoàn toàn dựa vào expireMinutes trả về từ /auth/token.
    /// Không detect từ response vì PCI document chỉ định nghĩa 200 và 500.
    ///
    /// Cách dùng: kế thừa class này và implement GetTokenFromDB() + SaveTokenToDB()
    /// với bất kỳ DB framework nào (EF, Dapper, ADO.NET...).
    /// </summary>
    public abstract class PCITokenProvider
    {
        protected readonly AuthTokenRequest Credentials;
        protected readonly ILogger Logger;

        private static readonly object _lock = new object();

        protected PCITokenProvider(AuthTokenRequest credentials, ILogger logger)
        {
            Credentials = credentials ?? throw new ArgumentNullException(nameof(credentials));
            Logger = logger;
        }

        /// <summary>
        /// Trả về Bearer token hợp lệ.
        /// Tự động gọi API và lưu storage nếu token hết hạn hoặc chưa có.
        /// </summary>
        public string GetAccessToken(Func<AuthTokenRequest, AuthTokenResponse> fetchTokenFunc)
        {
            var tokenInfo = GetTokenFromDB();

            if (tokenInfo != null && tokenInfo.IsValid())
            {
                Logger.Debug("PCITokenProvider: Using existing token from storage.");
                return tokenInfo.AccessToken;
            }

            lock (_lock)
            {
                // Double-check: tránh nhiều thread cùng gọi API khi token vừa hết hạn
                tokenInfo = GetTokenFromDB();
                if (tokenInfo != null && tokenInfo.IsValid())
                    return tokenInfo.AccessToken;

                Logger.Debug("PCITokenProvider: Token expired or not found. Fetching new token...");

                var response = fetchTokenFunc(Credentials);
                if (response == null || string.IsNullOrWhiteSpace(response.AccessToken))
                    throw new InvalidOperationException("PCITokenProvider: Failed to retrieve access token from PCI API.");

                var newToken = new PCITokenInfo
                {
                    AccessToken = response.AccessToken,
                    TokenType   = response.TokenType,
                    // Trừ 1 phút buffer để tránh dùng token ngay sát lúc hết hạn
                    ExpireAt    = DateTime.UtcNow.AddMinutes(response.ExpireMinutes - 1)
                };

                SaveTokenToDB(newToken);
                Logger.Debug($"PCITokenProvider: New token saved. Expires at {newToken.ExpireAt:O} UTC.");

                return newToken.AccessToken;
            }
        }

        // -------------------------------------------------------------------------
        // Abstract methods — consumer project tự implement theo DB framework
        // -------------------------------------------------------------------------

        /// <summary>
        /// Lấy token hiện tại từ DB/storage.
        /// Trả về null nếu chưa có token nào.
        /// </summary>
        protected abstract PCITokenInfo GetTokenFromDB();

        /// <summary>
        /// Lưu token mới vào DB/storage (insert hoặc update).
        /// </summary>
        protected abstract void SaveTokenToDB(PCITokenInfo token);
    }
}
