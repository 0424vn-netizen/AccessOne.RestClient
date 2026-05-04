using Aperia.AO.Api.External.Client.Common;
using Aperia.AO.Api.External.Client.Providers;
using Aperia.AO.Api.External.Client.Settings;
using Aperia.AO.Api.External.Model.AGT;
using Aperia.AO.Api.External.Model.Common;
using Aperia.AO.Api.External.Model.Constants;
using AS.AO.Api.RestClient;
using AS.AO.Api.RestClient.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Web;
using System.Xml;

namespace Aperia.AO.Api.External.Client
{
    public class StatementClient : StatementBaseClient
    {
        private const string ApiSource = "agnostic";
        private const string ApiSettingFile = "agnostic.xml";

        public StatementClient() :
            base(ApiSource, ApiSettingFile)
        {
        }

        protected static readonly Lazy<StatementClient> _lazyInstance = new Lazy<StatementClient>(() => new StatementClient());
        public static StatementClient Instance
        {
            get { return _lazyInstance.Value; }
        }
    }

    public class StatementTestClient : StatementBaseClient
    {
        private const string ApiSource = "agnosticTest";
        private const string ApiSettingFile = "agnosticTest.xml";

        public StatementTestClient() :
            base(ApiSource, ApiSettingFile)
        {
        }

        protected static readonly Lazy<StatementTestClient> _lazyInstance = new Lazy<StatementTestClient>(() => new StatementTestClient());
        public static StatementTestClient Instance
        {
            get { return _lazyInstance.Value; }
        }
    }

    public class StatementBaseClient : AORestClient, IAgtServiceClient
    {
        protected string LIMIT_CALL_MESSAGE = "The APi is limited. please contract adminitrator fo the system.";

        protected GbsTokenProvider gbsTokenProvider { get; set; }

        public AgtRuntimeApiSetting RuntimeApiSetting { get; set; }


        public StatementBaseClient(string source, string settingFile)
            : base(AOLogger.Instance, ApiLoggingService.Instance, source, settingFile, AgtClientSettings.Instance)
        {
            gbsTokenProvider = new GbsTokenProvider(base.Logger, this);
        }


        protected override void InterceptResponse(string trackingId, ApiSetting apiSetting, IRestRequest request, IRestResponse response, out bool shouldRetryPrevRequest)
        {
            shouldRetryPrevRequest = false;
        }

        public AgtClientSettings GetClientSettings()
        {
            return this.RestClientSettings as AgtClientSettings;
        }
        public ApiResponse<AgtAuthenticateResponse> Authenticate(AgtAuthenticateRequest body, AgtHeader header, bool forceGetNewToken = false)
        {
            LogDebug("Authenticate::Start function");
            if (forceGetNewToken)
            {
                LogDebug("Authenticate::Renew token");
            }

            // Force remove merchantID, AuxilaryUserData 
            header.MerchantId = string.Empty;

            // Set param
            var postParam = new AgtPostParam<AgtAuthenticateRequest, AgtAuthenticateResponse>()
            {
                TrackingId = Utils.GetTrackingId(),
                Path = "authenticate",
                Body = body,
                Parameters = null,
                IsLimitCallResponse = (apiResults) =>
                {
                    return apiResults?.Data?.ResponseBody?.FixedArea?.LogonStatusCode == AgtResponseCode.EXCUTE_LIMIT_CALL;
                }
            };
            postParam.ApiSetting = GetApiSetting(postParam.Path);            
            postParam.Headers = Utils.ObjToDictionary(header);
            postParam.Headers.Add("sharedTokenIndicator", "Y");

            // Call API
            var result = TryPostWithLimitCall<AgtAuthenticateRequest, AgtAuthenticateResponse>(postParam);

            LogDebug("Authenticate::End function");
            return result;
        }
        


        public IApiResponse<AgtApiResponse<GetStatementDatesListResponse>> GetStatementDatesList(GetStatementDatesListRequest request)
        {
            LogDebug($"GetStatementDatesList::Start function. \nRequest: \n{JsonConvert.SerializeObject(request)}");

            var trackingId = Utils.GetTrackingId();

            // Validate base request
            var validResult = ValidateBaseRequest(request);
            if (!validResult.IsSuccess)
            {
                LogDebug("GetStatementDatesList::Validate base request fail");
                ErrorResponse<AgtApiResponse<MMAllFieldResponseFuncArea>>(trackingId, validResult.Messages);
            }

            // Get token || Set Plarform is North for get token
            var tokenResult = gbsTokenProvider.GetToken(request, AgtPlatform.North).Result;
            if (!tokenResult.IsSuccess)
            {
                LogDebug($"GetStatementDatesList::Get token fail with messsage: {JsonConvert.SerializeObject(tokenResult.Messages)}");
                return ErrorResponse<AgtApiResponse<GetStatementDatesListResponse>>(trackingId, tokenResult.Messages);
            }

            var path = "statement-dates";
            var agtHeader = tokenResult.Data;
            agtHeader.BePlatformCode = GetBePlatformCodeForStatement(agtHeader.BePlatformCode);
            agtHeader.SysId = GetSysId(agtHeader.BePlatformCode);
            agtHeader.MerchantId = agtHeader.BePlatformCode == "O" ? agtHeader.MerchantId.PadRight(16, '0') : agtHeader.MerchantId;

            request.StatementType = GetStatementType(agtHeader.BePlatformCode, request.StatementType);

            // Set param
            var getParam = new AgtGetParam<AgtApiResponse<GetStatementDatesListResponse>>()
            {
                TrackingId = trackingId,
                Path = path,
                Parameters = Utils.ObjToDictionaryParamater(request, true),
                Headers = Utils.ObjToDictionary(agtHeader),
                ApiSetting = GetApiSettingWithCustomMockName(path),
                BaseRequest = request,
                AGTHeader = agtHeader,
                IsLimitCallResponse = (apiResults) =>
                {
                    return apiResults?.Data?.ResponseBody?.ResponseStandardArea?.responseStatusCode == AgtResponseCode.EXCUTE_LIMIT_CALL;
                }
            };

            // Call API
            var apiResponse = TryGetWithLimitCall<AgtApiResponse<GetStatementDatesListResponse>>(getParam);

            // Check responseStatusCode
            var responseStatusCode = apiResponse?.Data?.ResponseBody?.ResponseStandardArea?.responseStatusCode?.Replace("0", "") ?? "-1";
            if (apiResponse?.Status == ApiResponseStatus.Success && responseStatusCode != "")
            {
                LogDebug("GetStatementDatesList::responseStatusCode is not 00..");
                apiResponse.Status = ApiResponseStatus.BusinessError;
            }

            LogDebug("GetStatementDatesList::End function.");
            return apiResponse;
        }
        public IApiResponse<AgtApiResponse<GetStatementPdfRetrievalResponse>> GetStatementPdfRetrieval(GetStatementPdfRetrievalRequest request)
        {
            LogDebug($"GetStatementPdfRetrieval::Start function. \nRequest: \n{JsonConvert.SerializeObject(request)}");

            var trackingId = Utils.GetTrackingId();

            // Validate base request
            var validResult = ValidateBaseRequest(request);
            if (!validResult.IsSuccess)
            {
                LogDebug("GetStatementPdfRetrieval::Validate base request fail");
                ErrorResponse<AgtApiResponse<MMAllFieldResponseFuncArea>>(trackingId, validResult.Messages);
            }

            // Get token || Set Plarform is North for get token
            var tokenResult = gbsTokenProvider.GetToken(request, AgtPlatform.North).Result;
            if (!tokenResult.IsSuccess)
            {
                LogDebug($"GetStatementPdfRetrieval::Get token fail with messsage: {JsonConvert.SerializeObject(tokenResult.Messages)}");
                return ErrorResponse<AgtApiResponse<GetStatementPdfRetrievalResponse>>(trackingId, tokenResult.Messages);
            }

            var path = "statement-pdf";
            var agtHeader = tokenResult.Data;
            agtHeader.BePlatformCode = GetBePlatformCodeForStatement(agtHeader.BePlatformCode);
            agtHeader.SysId = GetSysId(agtHeader.BePlatformCode);
            agtHeader.MerchantId = agtHeader.BePlatformCode == "O" ? agtHeader.MerchantId.PadRight(16, '0') : agtHeader.MerchantId;


            // Set param
            var getParam = new AgtGetParam<AgtApiResponse<GetStatementPdfRetrievalResponse>>()
            {
                TrackingId = trackingId,
                Path = path,
                Parameters = Utils.ObjToDictionaryParamater(request),
                Headers = Utils.ObjToDictionary(agtHeader),
                ApiSetting = GetApiSettingWithCustomMockName(path),
                BaseRequest = request,
                AGTHeader = agtHeader,
                IsLimitCallResponse = (apiResults) =>
                {
                    return apiResults?.Data?.ResponseBody?.ResponseStandardArea?.responseStatusCode == AgtResponseCode.EXCUTE_LIMIT_CALL;
                }
            };

            // Call API
            var apiResponse = TryGetWithLimitCall<AgtApiResponse<GetStatementPdfRetrievalResponse>>(getParam);

            // Check responseStatusCode
            var responseStatusCode = apiResponse?.Data?.ResponseBody?.ResponseStandardArea?.responseStatusCode?.Replace("0", "") ?? "-1";
            if (apiResponse?.Status == ApiResponseStatus.Success && responseStatusCode != "")
            {
                LogDebug("GetStatementPdfRetrieval::responseStatusCode is not 00..");
                apiResponse.Status = ApiResponseStatus.BusinessError;
            }

            LogDebug("GetStatementPdfRetrieval::End function.");
            return apiResponse;
        }



        protected ApiResponse<TResult> ErrorResponse<TResult>(string trackingId, List<ApiMessage> Messages)
        {
            return new ApiResponse<TResult>
            {
                Status = ApiResponseStatus.Error,
                StatusCode = HttpStatusCode.Forbidden,
                TrackingId = trackingId,
                Messages = Messages
            };
        }
        protected ApiResponse<TResult> ErrorResponse<TResult>(string trackingId, Exception exception)
        {
            var responseStatusCode = HttpStatusCode.InternalServerError;
            var messageContent = "An error occurred.";

            if (exception is ApiException)
            {
                var apiException = exception as ApiException;
                responseStatusCode = apiException.StatusCode;
                messageContent = apiException.Message;
            }

            var message = new ApiMessage
            {
                Code = responseStatusCode.ToString(),
                MessageCode = (int)responseStatusCode,
                Description = messageContent
            };

            LogDebug($"ErrorResponse:: ATG Service client : response status code {responseStatusCode}");

            return new ApiResponse<TResult>
            {
                Data = default(TResult),
                Messages = new List<ApiMessage> { message },
                Status = ApiResponseStatus.Error,
                StatusCode = responseStatusCode,
                TrackingId = trackingId
            };
        }
        protected ApiResponse<TResult> CreateApiResponse<TResult>(string trackingId, TResult response)
        {
            if (response == null)
            {
                Logger.Debug("ATG Service client : response is null, status code is 403");
                return new ApiResponse<TResult>
                {
                    Status = ApiResponseStatus.Error,
                    StatusCode = HttpStatusCode.Forbidden,
                    TrackingId = trackingId,
                    Messages = new List<ApiMessage>()
                };
            }

            return new ApiResponse<TResult>
            {
                Status = ApiResponseStatus.Success,
                StatusCode = HttpStatusCode.OK,
                Data = response,
                TrackingId = trackingId,
                Messages = null
            };
        }
        protected ApiResponse<TResult> CreateApiResponse<TResult>(string trackingId, IRestResponse<TResult> response)
        {
            if (response != null)
            {
                Logger.Debug("Try get api status: " + (int)response.StatusCode + " / " + response.StatusDescription);
                return new ApiResponse<TResult>
                {
                    Status = ApiResponseStatus.Success,
                    StatusCode = response.StatusCode,
                    Data = response.Data,
                    TrackingId = trackingId,
                    Messages = null
                };
            }
            else
            {
                Logger.Debug("ATG Service client : response is null, status code force is 403");
                return new ApiResponse<TResult>
                {
                    Status = ApiResponseStatus.Error,
                    StatusCode = HttpStatusCode.Forbidden,
                    TrackingId = trackingId,
                    Messages = new List<ApiMessage>()
                };
            }
        }


        protected ApiResponse<TResult> TryGetWithLimitCall<TResult>(AgtGetParam<TResult> getParam)
            where TResult : class, new()
        {
            getParam.Parameters = Utils.GetCamelcaseName(getParam.Parameters);
            getParam.Headers = Utils.GetCamelcaseName(getParam.Headers);

            // Get setting for excute limit call
            LoadRuntimeApiSetting();

            return ExcuteLimitCall(() =>
            {
                try
                {
                    // calculate header
                    var standardRequestHeaders = Utils.GetStandardRequestHeaders(getParam.ApiSetting);
                    var finalHeader= standardRequestHeaders.MergeDictionary(getParam.Headers);

                    // Call API
                    var response = this.GetForRestResponse<TResult>(getParam.ApiSetting, getParam.Parameters, finalHeader, DataFormat.Json, getParam.TrackingId);
                    string mockFileOriginal = getParam.ApiSetting.MockFile;
                    if (CheckSecTokenIsExpired(response))
                    {
                        int flatFormID = getParam.BaseRequest.PlatformId;
                        if ("M".Equals(getParam.AGTHeader.BePlatformCode))
                        {
                            flatFormID = 2;
                        }

                        var newHeader = gbsTokenProvider.ForceResetToken(getParam.BaseRequest, flatFormID);
                        if (newHeader != null)
                        {
                            getParam.AGTHeader = newHeader;
                            getParam.Headers = Utils.GetCamelcaseName(Utils.ObjToDictionary(getParam.AGTHeader));
                            finalHeader.Remove("secToken");
                            finalHeader.Add("secToken", newHeader.SecToken);
                            bool isDisable = getParam.ApiSetting.IsDisabled.HasValue && getParam.ApiSetting.IsDisabled.Value;
                            if (isDisable)
                            {
                                FileInfo fileInfo = new FileInfo(mockFileOriginal);
                                getParam.ApiSetting.MockFile = $"{fileInfo.Directory}\\recall_{fileInfo.Name}";
                                response = this.GetForRestResponse<TResult>(getParam.ApiSetting, getParam.Parameters, finalHeader, DataFormat.Json, getParam.TrackingId);
                                getParam.ApiSetting.MockFile = mockFileOriginal;
                            }
                            else
                            {
                                response = this.GetForRestResponse<TResult>(getParam.ApiSetting, getParam.Parameters, finalHeader, DataFormat.Json, getParam.TrackingId);
                            }
                        }

                    }
                   
                    LogDebug("TryGetWithLimitCall::Content: " + JsonConvert.SerializeObject(response.Content));

                    return CreateApiResponse(getParam.TrackingId, response);
                }
                catch (Exception exception)
                {
                    Logger.Error(exception);

                    return ErrorResponse<TResult>(getParam.TrackingId, exception);
                }
            }, getParam.IsLimitCallResponse);
        }

        protected ApiResponse<TResult> TryPostWithLimitCall<T, TResult>(AgtPostParam<T, TResult> postParam)
            where T : class, new()
            where TResult : class, new()
        {
            postParam.Parameters = Utils.GetCamelcaseName(postParam.Parameters);
            postParam.Headers = Utils.GetCamelcaseName(postParam.Headers);

            // Get setting for excute limit call
            LoadRuntimeApiSetting();

            return ExcuteLimitCall(() =>
            {
                try
                {
                    // calculate header
                    var standardRequestHeaders = Utils.GetStandardRequestHeaders(postParam.ApiSetting);
                    var finalHeader = standardRequestHeaders.MergeDictionary(postParam.Headers);

                    // Call API
                    var response = this.Post<T, TResult>(postParam.ApiSetting, postParam.Body, postParam.Parameters, finalHeader, DataFormat.Json, postParam.TrackingId);
                   
                    return CreateApiResponse(postParam.TrackingId, response);
                }
                catch (Exception exception)
                {
                    Logger.Error(exception);

                    return ErrorResponse<TResult>(postParam.TrackingId, exception);
                }
            }, postParam.IsLimitCallResponse);
        }

        protected ApiResponse<T> ExcuteLimitCall<T>(Func<ApiResponse<T>> excutedFunc, Func<ApiResponse<T>, bool> isLimitCallResponse)
            where T : class, new()
        {
            var result = Activator.CreateInstance<ApiResponse<T>>();
            if (!RuntimeApiSetting.LimitCallOptions.RequiredCheck)
            {
                result = excutedFunc.Invoke();
                var isLimitCall = isLimitCallResponse.Invoke(result);

                if (isLimitCall)
                {
                    if (result.Messages == null)
                        result.Messages = new List<ApiMessage>();

                    result.Messages.Add(new ApiMessage
                    {
                        Code = HttpStatusCode.BadRequest.ToString(),
                        Description = LIMIT_CALL_MESSAGE
                    });
                }

                return result;
            }
            else
            {
                var excuteTime = DateTime.Now.AddMinutes(RuntimeApiSetting.LimitCallOptions.WaitingTimer);
                var isLock = true;
                while (isLock)
                {
                    result = excutedFunc.Invoke();
                    var isLimitCall = isLimitCallResponse.Invoke(result);

                    if (!isLimitCall)
                        isLock = false;
                    else
                    {
                        if (DateTime.Now >= excuteTime)
                        {
                            isLock = false;

                            if (result.Messages == null)
                                result.Messages = new List<ApiMessage>();

                            result.Messages.Add(new ApiMessage
                            {
                                Code = HttpStatusCode.BadRequest.ToString(),
                                Description = LIMIT_CALL_MESSAGE
                            });
                        }

                        Thread.Sleep(RuntimeApiSetting.LimitCallOptions.RecallTimer * 1000);
                    }
                }
            }

            return result;
        }
        protected void LoadRuntimeApiSetting()
        {
            if (this.RuntimeApiSetting != null)
                return;

            RuntimeApiSetting = new AgtRuntimeApiSetting();

            var xmlDocument = new XmlDocument();
            xmlDocument.XmlResolver = null;
            xmlDocument.Load(Utils.GetServerMapPath("App_Data\\ApiSettings\\" + this.SettingFile));
            XmlNodeList xmlNodeLists = xmlDocument.SelectNodes("//api");

            if (xmlNodeLists == null || xmlNodeLists.Count == 0)
                return;

            // Load CacheJwtToken
            var cacheJwtToken = false;
            bool.TryParse(xmlNodeLists[0].Attributes.GetTrimmedValueOrDefault("cacheJwtToken", "false"), out cacheJwtToken);
            RuntimeApiSetting.CacheJwtToken = cacheJwtToken;

            // Load limitCallOptionNode
            var limitCallOptionNode = xmlNodeLists[0].SelectSingleNode("limitCallOption");
            RuntimeApiSetting.LimitCallOptions = new LimitCallOptions();
            if (limitCallOptionNode != null)
            {
                RuntimeApiSetting.LimitCallOptions.WaitingTimer = int.Parse(limitCallOptionNode.Attributes.GetTrimmedValueOrDefault("waitingTimer", "5"));
                RuntimeApiSetting.LimitCallOptions.RecallTimer = int.Parse(limitCallOptionNode.Attributes.GetTrimmedValueOrDefault("recallTimer", "1"));

                var requiredCheck = false;
                bool.TryParse(limitCallOptionNode.Attributes.GetTrimmedValueOrDefault("requiredCheck", "false"), out requiredCheck);
                RuntimeApiSetting.LimitCallOptions.RequiredCheck = requiredCheck;
            }
        }
        protected FunctionResult ValidateBaseRequest(BaseRequest baseRequest)
        {
            LogDebug("ValidateBaseRequest::Start function");

            var messages = new List<ApiMessage>();

            if (baseRequest == null)
            {
                messages.Add(new ApiMessage
                {
                    Code = HttpStatusCode.BadRequest.ToString(),
                    Description = "The request is null."
                });
            }

            if (string.IsNullOrEmpty(baseRequest?.ApiUser))
            {
                messages.Add(new ApiMessage
                {
                    Code = HttpStatusCode.BadRequest.ToString(),
                    Description = "The Username is null or empty."
                });
            }

            if (baseRequest?.ApiSiteId == 0)
            {
                messages.Add(new ApiMessage
                {
                    Code = HttpStatusCode.BadRequest.ToString(),
                    Description = "The SiteId is null or 0"
                });
            }

            if (baseRequest?.PlatformId == 0)
            {
                messages.Add(new ApiMessage
                {
                    Code = HttpStatusCode.BadRequest.ToString(),
                    Description = "The PlatformId is null or 0"
                });
            }

            if (string.IsNullOrEmpty(baseRequest?.MerchantId))
            {
                messages.Add(new ApiMessage
                {
                    Code = HttpStatusCode.BadRequest.ToString(),
                    Description = "The MerchantId is null or empty."
                });
            }

            return new FunctionResult()
            {
                IsSuccess = !messages.Any(),
                Messages = messages
            };
        }
        protected ApiSetting GetApiSettingWithCustomMockName(string path)
        {
           var apiSetting = GetApiSetting(path);

            return apiSetting;
        }

        protected string GetBePlatformCodeForStatement(string platform)
        {
            if (string.Compare("FD", platform, true) == 0)
                return "O";

            if (string.Compare("NO", platform, true) == 0)
                return "N";

            if (string.Compare("MP", platform, true) == 0)
                return "M";

            return string.Empty;
        }
        protected string GetSysId(string platform)
        {
            if (string.Compare("O", platform, true) == 0)
                return "FDOP";

            if (string.Compare("N", platform, true) == 0)
                return "SYSS";

            if (string.Compare("M", platform, true) == 0)
                return "EFSP";

            return string.Empty;
        }
        protected string GetStatementType(string platform, string statementType)
        {
            if (string.Compare("O", platform, true) == 0)
                return "LOCATION";

            if (string.Compare("M", platform, true) == 0)
                return string.Empty;

            return statementType;
        }

        protected void LogDebug(string messsage)
        {
            Logger.Debug($"AGTServiceClient::{messsage}");
        }

        protected bool CheckSecTokenIsExpired<TResult>(IRestResponse<TResult> apiResponse)
        {
           
            if (apiResponse != null 
                && apiResponse?.Data?.GetType().GetGenericTypeDefinition() == typeof(AgtApiResponse<>).GetGenericTypeDefinition())
            {
                AgtApiResponse<object> tempResponse = JsonConvert.DeserializeObject<AgtApiResponse<object>>(JsonConvert.SerializeObject(apiResponse.Data));

                if (tempResponse != null)
                {
                    return ((int)AgnoticResponseStatusCode.SESSION_HAS_EXPRIED).ToString() == tempResponse.ResponseBody?.ResponseStandardArea.responseStatusCode;
                }
            }

            return false;
        }

    }
}
