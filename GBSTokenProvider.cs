using Aperia.AO.Api.External.Client.Common;
using Aperia.AO.Api.External.Model.AGT;
using Aperia.AO.Api.External.Model.AGT.GBS;
using Aperia.AO.Api.External.Model.Common;
using Aperia.AO.Api.External.Model.Constants;
using AS.AO.Api.RestClient;
using AS.AO.Api.RestClient.Models;
using AS.Common.DBManager;
using AS.Core.Common.Utilities;
using DDS.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Aperia.AO.Api.External.Client.Providers
{
    public  class GbsTokenProvider
    {
        protected readonly ILogger Logger;
        protected readonly IAgtServiceClient ServiceClient;

        public GbsTokenProvider(ILogger logger, IAgtServiceClient serviceClient)
        {
            Logger = logger;
            ServiceClient = serviceClient;
        }

        public Task<FunctionResult<AgtHeader>> GetToken(BaseRequest request, int tokenPlatform = AgtPlatform.All)
        {
            LogDebug("GetToken:: Start function");

            if (tokenPlatform == AgtPlatform.All)
                tokenPlatform = request.PlatformId;

            // Get Token from DB
            var token = GetTokenFromDB(request.ApiUser, tokenPlatform, request.MerchantId).Result;

            // Token expired
            if (!token.IsSuccess())
            {
                // Backup request platform
                var requestPlatform = request.PlatformId;
                request.PlatformId = tokenPlatform;

                // Call API Get Token
                token = GetAuthenApi(token, request).Result;

                // Set request platform
                request.PlatformId = requestPlatform;

                // Save Token to DB
                if (token.IsSuccess() && token.token != null)
                {
                    token.token = InsertTokenToDB(token.token).Result;
                }
            }

            var result = new FunctionResult<AgtHeader>()
            {
                IsSuccess = token.IsSuccess(),
                Messages = token.ApiMessages,
                Data = GetAGTHeader(token.token, request)
            };

            LogDebug("GetToken:: End function");
            return Task.FromResult(result);
        }

        public AgtHeader ForceResetToken( BaseRequest request, int tokenPlatform = AgtPlatform.All)
        {
            LogDebug("ResetToken:: Start function");

            if (tokenPlatform == AgtPlatform.All)
                tokenPlatform = request.PlatformId;

            // Get Token from DB
            var token = GetTokenFromDB(request.ApiUser, tokenPlatform, request.MerchantId).Result;

            // Backup request platform
            var requestPlatform = request.PlatformId;
            request.PlatformId = tokenPlatform;
            request.ForceGetNewToken = true;

            // Call API Get Token
            token = GetAuthenApi(token, request).Result;

            // Set request platform
            request.PlatformId = requestPlatform;

            // Save Token to DB
            if (token.IsSuccess() && token.token != null)
            {
                token.token = InsertTokenToDB(token.token).Result;
            }

            var result = new FunctionResult<AgtHeader>()
            {
                IsSuccess = token.IsSuccess(),
                Messages = token.ApiMessages,
                Data = GetAGTHeader(token.token, request)
            };

            LogDebug("Reset Token:: End function");

            return Task.FromResult(result).Result.Data;
        }


        private Task<GbsToken> GetTokenFromDB(string username, int platformId, string merchantNumber)
        {
            LogDebug("GetTokenFromDB:: Start function");

            var parameters = new FilterParameterCollection();
            parameters.Add(new FilterParameter("@UserId", username, DbType.String));
            parameters.Add(new FilterParameter("@PlatformId", platformId, DbType.Int32));
            parameters.Add(new FilterParameter("@MerchantNumber", merchantNumber, DbType.String));

            var token = new GbsTokenInfo();
            var table = ServiceClient.GetClientSettings().ReportServices.GetReports("spa_SEC_GetGBSToken_4Merchant", parameters);

            if (table != null && table.Rows.Count > 0)
                token = table.To<GbsTokenInfo>().FirstOrDefault();

            LogDebug("GetTokenFromDB:: End function");
            return Task.FromResult(new GbsToken
            {
                token = token,
                StatusCode = token != null && token.IsValid() ? AgtResponseCode.OK : string.Empty
            });
        }
        private Task<GbsTokenInfo> InsertTokenToDB(GbsTokenInfo token)
        {
            LogDebug("InsertTokenToDB:: Start function");

            var parameters = new FilterParameterCollection();
            parameters.Add(new FilterParameter("@RecordId", token.RecordId, DbType.Int64));
            parameters.Add(new FilterParameter("@PrimaryUserid", token.PrimaryUserId, DbType.String));
            parameters.Add(new FilterParameter("@ApiUserId", token.ApiUserId, DbType.String));
            parameters.Add(new FilterParameter("@MerchantNumber", token.MerchantNumber, DbType.String));
            parameters.Add(new FilterParameter("@PlatformId", token.PlatformId, DbType.Int32));
            parameters.Add(new FilterParameter("@Tier2Token", token.Tier2Token, DbType.String));
            parameters.Add(new FilterParameter("@Tier2ExpiredTime", token.Tier2ExpiredTime, DbType.DateTime));
            parameters.Add(new FilterParameter("@ClientIP", token.IPAddress, DbType.String));
            parameters.Add(new FilterParameter("@CreatedBy", token.UserId, DbType.String));

            var result = new GbsTokenInfo();
            var table = ServiceClient.GetClientSettings().ReportServices.GetReports("spa_SEC_InsertGBSToken_4Merchant", parameters);

            if (table != null && table.Rows.Count > 0)
                result = table.To<GbsTokenInfo>().FirstOrDefault();

            LogDebug("InsertTokenToDB:: End function");
            return Task.FromResult(result);
        }
        private Task<GbsToken> GetAuthenApi(GbsToken tokenRs, BaseRequest request)
        {
            LogDebug("GetAuthenApi::Start function.");

            var headers = GetAGTHeader(tokenRs.token, request);

            // Validate UserId
            if (headers.UserId.IsNullOrEmpty())
            {
                LogDebug("GetAuthenApi::The UserId is null or empty.");
                return Task.FromResult(ErrorToken("The UserId is null or empty."));
            }

            // Set AuxilaryUserData
            headers.AuxilaryUserData = string.Empty;
            if (request.PlatformId == AgtPlatform.Omaha && tokenRs.token != null)
            {
                headers.AuxilaryUserData = tokenRs.token.X500ID;
            }

            // call API Authen AGT            
            var authenticateRs = ServiceClient.Authenticate(new AgtAuthenticateRequest(), headers, request.ForceGetNewToken);

            // Success case
            if (authenticateRs?.Data?.ResponseBody?.FixedArea?.LogonStatusCode == AgtResponseCode.OK)
            {
                LogDebug($"GetAuthenApi::Get GBS BE Token for : {request.ApiUser} - sucessfully.");
                var gbsToken = new GbsToken
                {
                    token = new GbsTokenInfo
                    {
                        UserId = request.ApiUser,
                        PlatformId = request.PlatformId,
                        PrimaryUserId = tokenRs.token.PrimaryUserId,
                        ApiUserId = tokenRs.token.ApiUserId,
                        MerchantNumber = tokenRs.token.MerchantNumber,
                        Tier2Token = authenticateRs?.Data.ResponseBody.NewTokenInfo.NewToken,
                        Tier2ExpiredTime = ConvertEstToUtc(authenticateRs?.Data.ResponseBody.NewTokenInfo.TokenExpireDate, authenticateRs?.Data.ResponseBody.NewTokenInfo.TokenExpireTime),
                    },

                    Message = authenticateRs?.Data.ResponseBody.FixedArea.MsgText,
                    StatusCode = authenticateRs?.Data.ResponseBody.FixedArea.LogonStatusCode,
                    ApiMessages = authenticateRs?.Messages
                };

                LogDebug($"GetAuthenApi::Set GBS BE Token for : {request.ApiUser} - use for real api.");
                return Task.FromResult(gbsToken);
            }

            // fail case
            LogDebug($"GetAuthenApi::Get GBS BE Token for : {request.ApiUser} - failed.");
            return Task.FromResult(new GbsToken
            {
                token = new GbsTokenInfo { },
                Message = authenticateRs?.Data?.ResponseBody?.FixedArea?.MsgText,
                StatusCode = authenticateRs?.Data?.ResponseBody?.FixedArea?.LogonStatusCode,
                ApiMessages = authenticateRs?.Messages
            });
        }
        private AgtHeader GetAGTHeader(GbsTokenInfo token, BaseRequest request)
        {
            LogDebug("GetAGTHeader::Start function.");

            var headers = new AgtHeader
            {
                BePlatformCode = GetBePlatformCode(request.PlatformId),
                MerchantId = request.MerchantId,
            };

            if (token != null)
            {
                LogDebug("GetAGTHeader::Token is not null.");
                switch (request.PlatformId)
                {
                    case AgtPlatform.Omaha:
                    case AgtPlatform.North:
                    case AgtPlatform.Memphis:
                        headers.UserId = token.ApiUserId;
                        headers.AuxilaryUserData = token.AuxilaryUserData;
                        break;
                    default:
                        headers.UserId = string.Empty;
                        break;
                }

                if (token.IsValid())
                    headers.SecToken = token.Tier2Token;
                else //in case use gbs password to call authentication api to get gbs token
                    headers.SecToken = ServiceClient.GetClientSettings().ReportServices.DecryptText(token.Password);
            }

            LogDebug("GetAGTHeader::End function.");
            return headers;
        }


        private GbsToken ErrorToken(string errorMessage)
        {
            var apiMessages = new List<ApiMessage>();
            apiMessages.Add(new ApiMessage
            {
                Code = HttpStatusCode.BadRequest.ToString(),
                Description = errorMessage
            });

            return new GbsToken
            {
                token = new GbsTokenInfo { },
                Message = null,
                StatusCode = null,
                ApiMessages = apiMessages
            };
        }

        private string GetBePlatformCode(int platformId)
        {
            if (platformId == AgtPlatform.Omaha)
                return "FD";
            else if (platformId == AgtPlatform.North)
                return "NO";
            else if (platformId == AgtPlatform.Memphis)
                return "MP";

            return string.Empty;
        }

        private DateTime ConvertEstToUtc(string date, string time)
        {
            DateTime dateTime = DateTime.Parse($"{date} {time.Replace(".", ":")}");
            string easternZoneId = "Eastern Standard Time";
            TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(easternZoneId);
            return TimeZoneInfo.ConvertTimeToUtc(dateTime, timeZoneInfo);
        }


        private void LogDebug(string messsage)
        {
            Logger.Debug($"GBSTokenProvider::{messsage}");
        }
    }
}
