using System;
using VW.Api.RestClient.Models;
using VW.PCI.Api.Client.Models.Requests;
using VW.PCI.Api.Client.Models.Responses;

namespace VW.PCI.Api.Client
{
    /// <summary>
    /// Quản lý vòng đời token PCI: lấy mới, cache in-memory, tự refresh khi hết hạn.
    /// Token được lưu trong static fields — sống xuyên suốt vòng đời app,
    /// dùng chung giữa mọi instance và mọi thread.
    /// Thread-safe qua static lock object.
    /// </summary>
    public class PCITokenProvider
    {
        private readonly AuthTokenRequest _credentials;
        private readonly ILogger _logger;

        // Static fields: token tồn tại ở class level, không bị mất khi tạo instance mới
        private static string _accessToken;
        private static string _tokenType;
        private static DateTime _expireAt = DateTime.MinValue;
        private static readonly object _lock = new object();

        public PCITokenProvider(AuthTokenRequest credentials, ILogger logger)
        {
            _credentials = credentials ?? throw new ArgumentNullException(nameof(credentials));
            _logger = logger;
        }

        /// <summary>
        /// Trả về Bearer token hợp lệ.
        /// - Còn hạn: trả luôn từ cache, không gọi API.
        /// - Hết hạn hoặc chưa có: gọi /auth/token, lưu vào static cache.
        /// </summary>
        public string GetAccessToken(Func<AuthTokenRequest, AuthTokenResponse> fetchTokenFunc)
        {
            if (IsTokenValid())
                return _accessToken;

            lock (_lock)
            {
                // Double-check sau khi vào lock: tránh nhiều thread cùng gọi API lấy token
                if (IsTokenValid())
                    return _accessToken;

                _logger?.Debug("PCITokenProvider: Token expired or not found. Fetching new token...");

                var response = fetchTokenFunc(_credentials);

                if (response == null || string.IsNullOrWhiteSpace(response.AccessToken))
                    throw new InvalidOperationException("PCITokenProvider: Failed to retrieve access token from PCI API.");

                // Lưu vào static cache — tồn tại cho đến khi hết hạn hoặc bị InvalidateToken()
                _accessToken = response.AccessToken;
                _tokenType   = response.TokenType;
                // Trừ 1 phút để tránh dùng token ngay sát thời điểm hết hạn
                _expireAt = DateTime.UtcNow.AddMinutes(response.ExpireMinutes - 1);

                _logger?.Debug($"PCITokenProvider: Token cached in-memory. Expires at {_expireAt:O} UTC.");
            }

            return _accessToken;
        }

        /// <summary>
        /// Xóa token đang cache. Lần gọi GetAccessToken() tiếp theo sẽ fetch token mới.
        /// Dùng khi server trả về 401 Unauthorized.
        /// </summary>
        public void InvalidateToken()
        {
            lock (_lock)
            {
                _accessToken = null;
                _expireAt    = DateTime.MinValue;
                _logger?.Debug("PCITokenProvider: Token invalidated.");
            }
        }

        private static bool IsTokenValid() =>
            !string.IsNullOrWhiteSpace(_accessToken) && DateTime.UtcNow < _expireAt;
    }
}
