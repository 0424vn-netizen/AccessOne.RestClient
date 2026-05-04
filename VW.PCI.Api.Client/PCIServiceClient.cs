using RestSharp;
using System;
using System.Net;
using VW.Api.RestClient;
using VW.Api.RestClient.Models;
using VW.PCI.Api.Client.Models.Common;
using VW.PCI.Api.Client.Models.Requests;
using VW.PCI.Api.Client.Models.Responses;

namespace VW.PCI.Api.Client
{
    public class PCIServiceClient : VWRestClient, IPCIServiceClient
    {
        private const string ApiSource = "pci";
        private const string ApiSettingFile = "pciSettings.xml";

        private readonly PCITokenProvider _tokenProvider;

        // ---------------------------------------------------------------------
        // Singleton
        // ---------------------------------------------------------------------

        private static Lazy<PCIServiceClient> _lazyInstance;

        /// <summary>
        /// Truy cập singleton instance sau khi đã gọi Setup().
        /// </summary>
        public static PCIServiceClient Instance =>
            _lazyInstance?.Value
            ?? throw new InvalidOperationException(
                "PCIServiceClient chưa được khởi tạo. Gọi PCIServiceClient.Setup() trước.");

        /// <summary>
        /// Cấu hình và khởi tạo singleton. Gọi 1 lần duy nhất khi app start.
        /// Sau đó dùng PCIServiceClient.Instance ở bất kỳ đâu.
        /// </summary>
        public static void Setup(ILogger logger, ILoggingService loggingService, PCITokenProvider tokenProvider)
        {
            _lazyInstance = new Lazy<PCIServiceClient>(
                () => new PCIServiceClient(logger, loggingService, tokenProvider)
            );
        }

        // ---------------------------------------------------------------------
        // Constructor (private — buộc dùng qua Setup/Instance)
        // ---------------------------------------------------------------------

        private PCIServiceClient(ILogger logger, ILoggingService loggingService, PCITokenProvider tokenProvider)
            : base(logger, loggingService, ApiSource, ApiSettingFile, new RestClientSettings())
        {
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
        }

        // ---------------------------------------------------------------------
        // Token injection
        // ---------------------------------------------------------------------

        protected override void InterceptRequest(string trackingId, ApiSetting apiSetting, IRestRequest request)
        {
            if (apiSetting.Name == "auth/token")
                return;

            var token = _tokenProvider.GetAccessToken(FetchToken);
            request.AddHeader("Authorization", $"Bearer {token}");
        }

        private AuthTokenResponse FetchToken(AuthTokenRequest credentials)
        {
            var apiSetting = this.GetApiSetting("auth/token");
            return this.Post<AuthTokenRequest, AuthTokenResponse>(apiSetting, body: credentials);
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
                var tid = trackingId ?? Guid.NewGuid().ToString("N");
                var response = this.PostForRestResponse<TBody, TResult>(apiSetting, body, trackingId: tid);

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
