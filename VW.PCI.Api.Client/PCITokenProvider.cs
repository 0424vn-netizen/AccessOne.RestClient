using System;
using System.Threading;
using VW.Api.RestClient.Models;
using VW.PCI.Api.Client.Models.Requests;
using VW.PCI.Api.Client.Models.Responses;

namespace VW.PCI.Api.Client
{
    /// <summary>
    /// Quản lý vòng đời token PCI: lấy mới, cache in-memory, tự refresh khi hết hạn.
    /// Thread-safe — dùng được trong môi trường multi-thread.
    /// </summary>
    public class PCITokenProvider
    {
        private readonly AuthTokenRequest _credentials;
        private readonly ILogger _logger;
        private readonly object _lock = new object();

        private string _accessToken;
        private string _tokenType;
        private DateTime _expireAt = DateTime.MinValue;

        public PCITokenProvider(AuthTokenRequest credentials, ILogger logger)
        {
            _credentials = credentials ?? throw new ArgumentNullException(nameof(credentials));
            _logger = logger;
        }

        /// <summary>
        /// Trả về Bearer token hợp lệ. Tự động gọi lại API nếu token đã hết hạn.
        /// </summary>
        public string GetAccessToken(Func<AuthTokenRequest, AuthTokenResponse> fetchTokenFunc)
        {
            if (IsTokenValid())
                return _accessToken;

            lock (_lock)
            {
                // Double-check sau khi vào lock, tránh nhiều thread cùng refresh
                if (IsTokenValid())
                    return _accessToken;

                _logger?.Debug("PCITokenProvider: Token expired or not found. Fetching new token...");

                var response = fetchTokenFunc(_credentials);
                if (response == null || string.IsNullOrWhiteSpace(response.AccessToken))
                    throw new InvalidOperationException("PCITokenProvider: Failed to retrieve access token from PCI API.");

                _accessToken = response.AccessToken;
                _tokenType = response.TokenType;
                // Trừ 1 phút để tránh race condition ngay sát thời điểm hết hạn
                _expireAt = DateTime.UtcNow.AddMinutes(response.ExpireMinutes - 1);

                _logger?.Debug($"PCITokenProvider: New token acquired. Expires at {_expireAt:O} UTC.");
            }

            return _accessToken;
        }

        /// <summary>
        /// Buộc xóa token hiện tại để lần gọi tiếp theo sẽ lấy token mới.
        /// Dùng khi server trả về 401 Unauthorized.
        /// </summary>
        public void InvalidateToken()
        {
            lock (_lock)
            {
                _accessToken = null;
                _expireAt = DateTime.MinValue;
                _logger?.Debug("PCITokenProvider: Token invalidated.");
            }
        }

        public string GetAuthorizationHeaderValue() =>
            $"{(_tokenType ?? "Bearer")} {_accessToken}";

        private bool IsTokenValid() =>
            !string.IsNullOrWhiteSpace(_accessToken) && DateTime.UtcNow < _expireAt;
    }
}
