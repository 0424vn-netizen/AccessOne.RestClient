using RestSharp;
using System;
using System.Collections.Generic;
using VW.Api.RestClient;
using VW.Api.RestClient.Models;
using VW.PCI.Api.Client.Models.Common;
using VW.PCI.Api.Client.Models.Requests;
using VW.PCI.Api.Client.Models.Responses;

namespace VW.PCI.Api.Client
{
    /// <summary>
    /// Client tích hợp toàn bộ PCI APIs.
    /// Kế thừa VWRestClient — tự động xử lý token, logging, error handling.
    /// </summary>
    public class PCIServiceClient : VWRestClient, IPCIServiceClient
    {
        private const string ApiSource = "pci";
        private const string ApiSettingFile = "pciSettings.xml";

        private readonly PCITokenProvider _tokenProvider;

        public PCIServiceClient(ILogger logger, ILoggingService loggingService, PCITokenProvider tokenProvider)
            : base(logger, loggingService, ApiSource, ApiSettingFile, new RestClientSettings())
        {
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
        }

        #region Token injection

        /// <summary>
        /// Tự động gắn Bearer token vào mọi request trước khi gửi.
        /// </summary>
        protected override void InterceptRequest(string trackingId, ApiSetting apiSetting, IRestRequest request)
        {
            // Auth endpoint không cần token
            if (apiSetting.Name == "auth/token")
                return;

            var token = _tokenProvider.GetAccessToken(FetchToken);
            request.AddHeader("Authorization", $"Bearer {token}");
        }

        /// <summary>
        /// Hàm gọi API lấy token — được truyền vào PCITokenProvider.
        /// </summary>
        private AuthTokenResponse FetchToken(AuthTokenRequest credentials)
        {
            var apiSetting = this.GetApiSetting("auth/token");
            return this.Post<AuthTokenRequest, AuthTokenResponse>(apiSetting, body: credentials);
        }

        #endregion

        #region API Methods

        public PCIApiResponse<CreateUserResponse> CreateUser(CreateUserRequest request)
        {
            return Execute<CreateUserRequest, CreateUserResponse>("user/CreateUser", request);
        }

        public PCIApiResponse<UpdateUserResponse> UpdateUser(UpdateUserRequest request)
        {
            return Execute<UpdateUserRequest, UpdateUserResponse>("user/UpdateUser", request);
        }

        public PCIApiResponse<GetMasterMerchantResponse> GetMasterMerchant(GetMasterMerchantRequest request)
        {
            return Execute<GetMasterMerchantRequest, GetMasterMerchantResponse>("user/GetMasterMerchant", request);
        }

        public PCIApiResponse<GetHierarchyIDResponse> GetHierarchyID(GetHierarchyIDRequest request)
        {
            return Execute<GetHierarchyIDRequest, GetHierarchyIDResponse>("user/GetHierarchyID", request);
        }

        public PCIApiResponse<UpdSecRoleByUserIDResponse> UpdSecRoleByUserID(UpdSecRoleByUserIDRequest request)
        {
            return Execute<UpdSecRoleByUserIDRequest, UpdSecRoleByUserIDResponse>("user/UpdSecRoleByUserID", request);
        }

        public PCIApiResponse<UpdateOptInOutResponse> UpdateOptInOut(UpdateOptInOutRequest request)
        {
            return Execute<UpdateOptInOutRequest, UpdateOptInOutResponse>("user/UpdateOptInOut", request);
        }

        public PCIApiResponse<GetAllHierarchyForAOResponse> GetAllHierarchyForAO(GetAllHierarchyForAORequest request)
        {
            return Execute<GetAllHierarchyForAORequest, GetAllHierarchyForAOResponse>("user/GetAllHierarchyForAO", request);
        }

        public PCIApiResponse<GetUsersResponse> GetUsers(GetUsersRequest request)
        {
            return Execute<GetUsersRequest, GetUsersResponse>("user/GetUsers", request);
        }

        #endregion

        #region Private helpers

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
                {
                    return new PCIApiResponse<TResult>
                    {
                        IsSuccess = true,
                        StatusCode = response.StatusCode,
                        TrackingId = tid,
                        Data = response.Data
                    };
                }

                var error = TryDeserializeError(response.Content);
                return new PCIApiResponse<TResult>
                {
                    IsSuccess = false,
                    StatusCode = response.StatusCode,
                    TrackingId = error?.TrackId ?? tid,
                    ErrorMessage = error?.ErrorMessage ?? response.StatusDescription
                };
            }
            catch (ApiException ex)
            {
                Logger.Error(ex);
                return new PCIApiResponse<TResult>
                {
                    IsSuccess = false,
                    StatusCode = ex.StatusCode,
                    TrackingId = ex.TrackingId,
                    ErrorMessage = ex.Message
                };
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return new PCIApiResponse<TResult>
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.InternalServerError,
                    ErrorMessage = ex.Message
                };
            }
        }

        private PCIErrorResponse TryDeserializeError(string content)
        {
            try
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject<PCIErrorResponse>(content);
            }
            catch
            {
                return null;
            }
        }

        #endregion
    }
}
