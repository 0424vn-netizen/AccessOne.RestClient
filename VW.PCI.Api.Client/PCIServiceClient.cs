using RestSharp;
using System;
using System.Collections.Concurrent;
using System.Net;
using VW.Api.RestClient;
using VW.Api.RestClient.Models;
using VW.PCI.Api.Client.Models.Common;
using VW.PCI.Api.Client.Models.Requests;
using VW.PCI.Api.Client.Models.Responses;

namespace VW.PCI.Api.Client
{
    /// <summary>
    /// PCI API client — per-user token, shared infrastructure.
    ///
    /// Setup 1 lần tại app startup:
    ///   PCIServiceClient.Configure(logger, loggingService);
    ///
    /// Mỗi request, dùng credentials của user đang login:
    ///   PCIServiceClient.ForUser(new AuthTokenRequest { ApplicationId = ..., ... }).GetUsers(request);
    ///
    /// Token được cache riêng theo ApplicationId, tự refresh khi hết hạn.
    /// </summary>
    public class PCIServiceClient : VWRestClient, IPCIServiceClient
    {
        private const string ApiSource      = "pci";
        private const string ApiSettingFile = "pciSettings.xml";

        // ---------------------------------------------------------------------
        // Shared infrastructure — set once at startup via Configure()
        // ---------------------------------------------------------------------

        private static ILogger          _sharedLogger;
        private static ILoggingService  _sharedLoggingService;

        public static void Configure(ILogger logger, ILoggingService loggingService)
        {
            _sharedLogger         = logger         ?? throw new ArgumentNullException(nameof(logger));
            _sharedLoggingService = loggingService  ?? throw new ArgumentNullException(nameof(loggingService));
        }

        // ---------------------------------------------------------------------
        // Per-user factory
        // ---------------------------------------------------------------------

        public static IPCIServiceClient ForUser(AuthTokenRequest credentials)
        {
            if (_sharedLogger == null)
                throw new InvalidOperationException(
                    "PCIServiceClient has not been configured. Call PCIServiceClient.Configure() at application startup.");

            if (credentials == null) throw new ArgumentNullException(nameof(credentials));
            if (string.IsNullOrWhiteSpace(credentials.ApplicationId))
                throw new ArgumentException("ApplicationId is required.", nameof(credentials));

            return new PCIServiceClient(_sharedLogger, _sharedLoggingService, credentials);
        }

        // ---------------------------------------------------------------------
        // Per-user token cache keyed by ApplicationId
        // ---------------------------------------------------------------------

        private static readonly ConcurrentDictionary<string, PCITokenInfo> _tokenCache
            = new ConcurrentDictionary<string, PCITokenInfo>();

        private static readonly object _lock = new object();

        // ---------------------------------------------------------------------
        // Instance state
        // ---------------------------------------------------------------------

        private readonly AuthTokenRequest _credentials;

        private PCIServiceClient(ILogger logger, ILoggingService loggingService, AuthTokenRequest credentials)
            : base(logger, loggingService, ApiSource, ApiSettingFile, new RestClientSettings())
        {
            _credentials = credentials;
        }

        // ---------------------------------------------------------------------
        // Token injection
        // ---------------------------------------------------------------------

        protected override void InterceptRequest(string trackingId, ApiSetting apiSetting, IRestRequest request)
        {
            if (apiSetting.Name == "auth/token")
                return;

            request.AddHeader("Authorization", $"Bearer {GetValidToken()}");
        }

        protected override void InterceptResponse(string trackingId, ApiSetting apiSetting, IRestRequest request, IRestResponse response, out bool shouldRetryPrevRequest)
        {
            shouldRetryPrevRequest = false;
        }

        private string GetValidToken()
        {
            var key = _credentials.ApplicationId;

            if (_tokenCache.TryGetValue(key, out var tokenInfo) && tokenInfo.IsValid())
                return tokenInfo.AccessToken;

            lock (_lock)
            {
                if (_tokenCache.TryGetValue(key, out tokenInfo) && tokenInfo.IsValid())
                    return tokenInfo.AccessToken;

                Logger.Debug($"PCIServiceClient: Fetching new token for ApplicationId={key}...");

                var apiSetting = this.GetApiSetting("auth/token");
                var response   = this.Post<AuthTokenRequest, AuthTokenResponse>(apiSetting, body: _credentials);

                if (response == null || string.IsNullOrWhiteSpace(response.AccessToken))
                    throw new InvalidOperationException($"PCIServiceClient: Failed to retrieve access token for ApplicationId={key}.");

                var newToken = new PCITokenInfo
                {
                    AccessToken = response.AccessToken,
                    TokenType   = response.TokenType,
                    ExpireAt    = DateTime.UtcNow.AddMinutes(response.ExpireMinutes - 1)
                };

                _tokenCache[key] = newToken;
                Logger.Debug($"PCIServiceClient: Token saved for ApplicationId={key}. Expires at {newToken.ExpireAt:O} UTC.");

                return newToken.AccessToken;
            }
        }

        // ---------------------------------------------------------------------
        // API Methods
        // ---------------------------------------------------------------------

        public PCIApiResponse<CreateUserResponse> CreateUser(CreateUserRequest request)
            => Execute<CreateUserRequest, CreateUserResponse>("user/CreateUser", request);

        public PCIApiResponse<UpdateUserResponse> UpdateUser(UpdateUserRequest request)
            => Execute<UpdateUserRequest, UpdateUserResponse>("user/UpdateUser", request);

        public PCIApiResponse<GetMasterMerchantResponse> GetMasterMerchant(GetMasterMerchantRequest request)
            => Execute<GetMasterMerchantRequest, GetMasterMerchantResponse>("user/GetMasterMerchant", request);

        public PCIApiResponse<GetHierarchyIDResponse> GetHierarchyID(GetHierarchyIDRequest request)
            => Execute<GetHierarchyIDRequest, GetHierarchyIDResponse>("user/GetHierarchyID", request);

        public PCIApiResponse<UpdSecRoleByUserIDResponse> UpdSecRoleByUserID(UpdSecRoleByUserIDRequest request)
            => Execute<UpdSecRoleByUserIDRequest, UpdSecRoleByUserIDResponse>("user/UpdSecRoleByUserID", request);

        public PCIApiResponse<UpdateOptInOutResponse> UpdateOptInOut(UpdateOptInOutRequest request)
            => Execute<UpdateOptInOutRequest, UpdateOptInOutResponse>("user/UpdateOptInOut", request);

        public PCIApiResponse<GetAllHierarchyForAOResponse> GetAllHierarchyForAO(GetAllHierarchyForAORequest request)
            => Execute<GetAllHierarchyForAORequest, GetAllHierarchyForAOResponse>("user/GetAllHierarchyForAO", request);

        public PCIApiResponse<GetUsersResponse> GetUsers(GetUsersRequest request)
            => Execute<GetUsersRequest, GetUsersResponse>("user/GetUsers", request);

        // ---------------------------------------------------------------------
        // Private helpers
        // ---------------------------------------------------------------------

        private PCIApiResponse<TResult> Execute<TBody, TResult>(string path, TBody body, string trackingId = null)
            where TBody : class, new()
            where TResult : class, new()
        {
            try
            {
                var apiSetting = this.GetApiSetting(path);
                var tid        = trackingId ?? Guid.NewGuid().ToString("N");
                var response   = this.PostForRestResponse<TBody, TResult>(apiSetting, body, trackingId: tid);

                if (response.StatusCode == HttpStatusCode.OK)
                    return new PCIApiResponse<TResult>
                    {
                        IsSuccess  = true,
                        StatusCode = response.StatusCode,
                        TrackingId = tid,
                        Data       = response.Data
                    };

                var error = TryDeserializeError(response.Content);
                return new PCIApiResponse<TResult>
                {
                    IsSuccess    = false,
                    StatusCode   = response.StatusCode,
                    TrackingId   = error?.TrackId ?? tid,
                    ErrorMessage = error?.ErrorMessage ?? response.StatusDescription
                };
            }
            catch (ApiException ex)
            {
                Logger.Error(ex);
                return new PCIApiResponse<TResult>
                {
                    IsSuccess    = false,
                    StatusCode   = ex.StatusCode,
                    TrackingId   = ex.TrackingId,
                    ErrorMessage = ex.Message
                };
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return new PCIApiResponse<TResult>
                {
                    IsSuccess    = false,
                    StatusCode   = HttpStatusCode.InternalServerError,
                    ErrorMessage = ex.Message
                };
            }
        }

        private PCIErrorResponse TryDeserializeError(string content)
        {
            try { return Newtonsoft.Json.JsonConvert.DeserializeObject<PCIErrorResponse>(content); }
            catch { return null; }
        }
    }
}
