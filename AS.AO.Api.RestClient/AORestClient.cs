using AS.AO.Api.RestClient.Models;
using AS.AO.Api.RestClient.Serializers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AS.AO.Api.RestClient
{
    /// <summary>
    /// The base rest client
    /// </summary>
    /// <seealso cref="IAORestClient" />
    public abstract class AORestClient : IAORestClient
    {
        #region Properties

        /// <summary>
        /// The API consuming error
        /// </summary>
        protected const string ApiError = "An error occurred while consuming to resource: {0}. Response status code: {1}. Response content: {2}. Tracking Id: {3}";

        /// <summary>
        /// The API call has been disabled
        /// </summary>
        protected const string ApiCallHasBeenDisabled = "The api call to {0} has been disabled. Please contact the administrator. Tracking Id: {1}";

        /// <summary>
        /// The API call has been disabled
        /// </summary>
        protected const string ApiCallHasBeenDisabledAndLoadDataFromMockFile = "The api call to {0} has been disabled. Loading data from file: [{1}]. Tracking Id: {2}";

        /// <summary>
        /// Gets or sets the source.
        /// </summary>
        protected string Source { get; set; }

        /// <summary>
        /// Gets or sets the setting file.
        /// </summary>
        protected string SettingFile { get; set; }

        /// <summary>
        /// Gets or sets the rest client settings.
        /// </summary>
        protected RestClientSettings RestClientSettings { get; set; }

        /// <summary>
        /// Gets or sets the logger.
        /// </summary>
        protected ILogger Logger { get; set; }

        /// <summary>
        /// Gets or sets the logging service.
        /// </summary>
        protected ILoggingService LoggingService { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AORestClient" /> class.
        /// </summary>
        /// <param name="loggingService">The logging service.</param>
        /// <param name="source">The source.</param>
        /// <param name="settingFile">The setting file.</param>
        /// <param name="restClientSettings">The rest client settings.</param>
        public AORestClient(ILogger logger, ILoggingService loggingService, string source, string settingFile, RestClientSettings restClientSettings)
        {
            this.Logger = logger;
            this.LoggingService = loggingService;
            this.Source = source;
            this.SettingFile = settingFile;
            this.RestClientSettings = restClientSettings;
        }

        #endregion

        #region Send synchronous

        /// <summary>
        /// Gets the specified API setting.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="apiSetting">The API setting.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="headers">The request headers.</param>
        /// <param name="requestFormat">The request format.</param>
        /// <param name="trackingId">The tracking identifier.</param>
        /// <returns></returns>
        public TResult Get<TResult>(ApiSetting apiSetting, IDictionary<string, string> parameters = null, IDictionary<string, string> headers = null, DataFormat requestFormat = DataFormat.Json, string trackingId = null)
            where TResult : class, new()
        {
            string resource = null;
            Exception exception = null;
            IRestRequest request = null;
            IRestResponse<TResult> response = null;

            if (apiSetting.IsDisabled.HasValue && apiSetting.IsDisabled.Value)
            {
                if (apiSetting.MockService.Enabled.HasValue && apiSetting.MockService.Enabled.Value)
                {
                    return CreateResponseForApiCallWithMockService<TResult>(apiSetting, Method.GET, null, parameters, headers, requestFormat, trackingId);
                }
                else
                {
                    return this.CreateResponseForDisabledApiCall<TResult>(trackingId, apiSetting, Method.GET, parameters, headers);
                }
            }

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                request = this.PrepareRequest(apiSetting, Method.GET, requestFormat, parameters, headers);

                this.InterceptRequest(trackingId, apiSetting, request);

                var client = this.PrepareClient(apiSetting);
                resource = client.BuildUri(request)?.ToString();
                response = client.Execute<TResult>(request);

                this.InterceptResponse(trackingId, apiSetting, request, response, out var shouldRetryPrevRequest);
                this.Logger.Debug("response has null ? : " + response.Data == null);
                return shouldRetryPrevRequest ? this.Get<TResult>(apiSetting, parameters, headers, requestFormat, trackingId) :
                                                this.CreateApiResponse(trackingId, apiSetting, response);
            }
            catch (ApiException apiException)
            {
                exception = apiException.InnerException ?? apiException;
                throw;
            }
            catch (Exception ex)
            {
                exception = ex;
                this.Logger.Error(exception);
                this.Logger.Debug("Happen Error in Core: " + exception);
                this.ThrowApiException(trackingId, apiSetting, response, exception, HttpStatusCode.InternalServerError);

                return null;
            }
            finally
            {
                stopwatch.Stop();
                this.LogApiRequestWithOtherInfo(trackingId, this.Source, resource ?? $"{apiSetting.BaseUrl}/{apiSetting.Path}", request, response, stopwatch.Elapsed.TotalMilliseconds, exception, null, apiSetting.IncludeRequestHeadersInTrackingLog, apiSetting.IncludeResponseHeadersInTrackingLog, GetApiTrackingOtherInfo(apiSetting));
            }
        }

        /// <summary>
        /// Gets the specified base URL.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="baseUrl">The base URL.</param>
        /// <param name="path">The path.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="headers">The request headers.</param>
        /// <param name="requestFormat">The request format.</param>
        /// <param name="trackingId">The tracking identifier.</param>
        /// <returns></returns>
        public virtual TResult Get<TResult>(string baseUrl, string path, IDictionary<string, string> parameters = null, IDictionary<string, string> headers = null, DataFormat requestFormat = DataFormat.Json, string trackingId = null)
            where TResult : class, new()
        {
            string resource = null;
            Exception exception = null;
            IRestRequest request = null;
            IRestResponse<TResult> response = null;

            var apiSetting = this.GetApiSetting(path);
            if (apiSetting.IsDisabled.HasValue && apiSetting.IsDisabled.Value)
            {
                if (apiSetting.MockService.Enabled.HasValue && apiSetting.MockService.Enabled.Value)
                {
                    return CreateResponseForApiCallWithMockService<TResult>(apiSetting, Method.GET, null, parameters, headers, requestFormat, trackingId);
                }
                return this.CreateResponseForDisabledApiCall<TResult>(trackingId, apiSetting, Method.GET, null, parameters, headers);
            }



            var stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                request = this.PrepareRequest(apiSetting, Method.GET, requestFormat, parameters, headers);

                this.InterceptRequest(trackingId, apiSetting, request);

                var client = this.PrepareClient(apiSetting);
                resource = client.BuildUri(request)?.ToString();
                response = client.Execute<TResult>(request);

                this.InterceptResponse(trackingId, apiSetting, request, response, out var shouldRetryPrevRequest);

                return shouldRetryPrevRequest ? this.Get<TResult>(baseUrl, path, parameters, headers, requestFormat, trackingId) :
                                                this.CreateApiResponse(trackingId, apiSetting, response);
            }
            catch (ApiException apiException)
            {
                exception = apiException.InnerException ?? apiException;
                throw;
            }
            catch (Exception ex)
            {
                exception = ex;
                this.Logger.Error(exception);
                this.ThrowApiException(trackingId, baseUrl, path, response, exception, HttpStatusCode.InternalServerError);

                return null;
            }
            finally
            {
                stopwatch.Stop();
                this.LogApiRequestWithOtherInfo(trackingId, this.Source, resource ?? $"{baseUrl}/{path}", request, response, stopwatch.Elapsed.TotalMilliseconds, exception, null, apiSetting.IncludeRequestHeadersInTrackingLog, apiSetting.IncludeResponseHeadersInTrackingLog, GetApiTrackingOtherInfo(apiSetting));
            }
        }

        /// <summary>
        /// Consumes a Restful API as HTTP GET method.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="apiSetting">The API setting.</param>
        /// <param name="parameters">The request query parameters.</param>
        /// <param name="headers">The request headers.</param>
        /// <param name="requestFormat">The request data format.</param>
        /// <param name="trackingId">The tracking identifier.</param>
        /// <returns></returns>
        public IRestResponse<TResult> GetForRestResponse<TResult>(ApiSetting apiSetting, IDictionary<string, string> parameters = null, IDictionary<string, string> headers = null, DataFormat requestFormat = DataFormat.Json, string trackingId = null) where TResult : class, new()
        {
            string resource = null;
            Exception exception = null;
            IRestRequest request = null;
            IRestResponse<TResult> response = null;

            if (apiSetting.IsDisabled.HasValue && apiSetting.IsDisabled.Value)
            {
                if (apiSetting.MockService.Enabled.HasValue && apiSetting.MockService.Enabled.Value)
                {
                    return CreateRestResponseForApiCallWithMockService<TResult>(apiSetting, Method.GET, null, parameters, headers, requestFormat, trackingId);
                }
                return this.CreateRestResponseForDisabledApiCall<TResult>(trackingId, apiSetting, Method.GET, null, parameters, headers);
            }



            var stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                request = this.PrepareRequest(apiSetting, Method.GET, requestFormat, parameters, headers);

                this.InterceptRequest(trackingId, apiSetting, request);

                var client = this.PrepareClient(apiSetting);
                resource = client.BuildUri(request)?.ToString();
                response = client.Execute<TResult>(request);

                this.InterceptResponse(trackingId, apiSetting, request, response, out var shouldRetryPrevRequest);

                return shouldRetryPrevRequest ? this.GetForRestResponse<TResult>(apiSetting, parameters, headers, requestFormat, trackingId) : response;
            }
            catch (Exception ex)
            {
                exception = ex;
                this.Logger.Error(exception);

                throw;
            }
            finally
            {
                stopwatch.Stop();
                this.LogApiRequestWithOtherInfo(trackingId, this.Source, resource ?? $"{apiSetting.BaseUrl}/{apiSetting.Path}", request, response, stopwatch.Elapsed.TotalMilliseconds, exception, null, apiSetting.IncludeRequestHeadersInTrackingLog, apiSetting.IncludeResponseHeadersInTrackingLog, GetApiTrackingOtherInfo(apiSetting));
            }
        }

        /// <summary>
        /// Consumes a Restful API as HTTP GET method.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="baseUrl">The base URL.</param>
        /// <param name="path">The api path.</param>
        /// <param name="parameters">The request query parameters.</param>
        /// <param name="headers">The request headers.</param>
        /// <param name="requestFormat">The request data format.</param>
        /// <param name="trackingId">The tracking identifier.</param>
        /// <returns></returns>
        public virtual IRestResponse<TResult> GetForRestResponse<TResult>(string baseUrl, string path, IDictionary<string, string> parameters = null, IDictionary<string, string> headers = null, DataFormat requestFormat = DataFormat.Json, string trackingId = null) where TResult : class, new()
        {
            string resource = null;
            Exception exception = null;
            IRestRequest request = null;
            IRestResponse<TResult> response = null;

            var apiSetting = this.GetApiSetting(path);
            if (apiSetting.IsDisabled.HasValue && apiSetting.IsDisabled.Value)
            {
                if (apiSetting.MockService.Enabled.HasValue && apiSetting.MockService.Enabled.Value)
                {
                    return CreateRestResponseForApiCallWithMockService<TResult>(apiSetting, Method.GET, null, parameters, headers, requestFormat, trackingId);
                }
                return this.CreateRestResponseForDisabledApiCall<TResult>(trackingId, apiSetting, Method.GET, null, parameters, headers);
            }


            var stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                request = this.PrepareRequest(apiSetting, Method.GET, requestFormat, parameters, headers);

                this.InterceptRequest(trackingId, apiSetting, request);

                var client = this.PrepareClient(apiSetting);
                resource = client.BuildUri(request)?.ToString();
                response = client.Execute<TResult>(request);

                this.InterceptResponse(trackingId, apiSetting, request, response, out var shouldRetryPrevRequest);

                return shouldRetryPrevRequest ? this.GetForRestResponse<TResult>(baseUrl, path, parameters, headers, requestFormat, trackingId) : response;
            }
            catch (Exception ex)
            {
                exception = ex;
                this.Logger.Error(exception);

                throw;
            }
            finally
            {
                stopwatch.Stop();
                this.LogApiRequestWithOtherInfo(trackingId, this.Source, resource ?? $"{baseUrl}/{path}", request, response, stopwatch.Elapsed.TotalMilliseconds, exception, null, apiSetting.IncludeRequestHeadersInTrackingLog, apiSetting.IncludeResponseHeadersInTrackingLog, GetApiTrackingOtherInfo(apiSetting));
            }
        }

        /// <summary>
        /// Posts the specified API setting.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="apiSetting">The API setting.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="headers">The request headers.</param>
        /// <param name="requestFormat">The request format.</param>
        /// <param name="trackingId">The tracking identifier.</param>
        /// <returns></returns>
        public TResult Post<TResult>(ApiSetting apiSetting, IDictionary<string, string> parameters = null, IDictionary<string, string> headers = null, DataFormat requestFormat = DataFormat.Json, string trackingId = null)
            where TResult : class, new()
        {
            string resource = null;
            Exception exception = null;
            IRestRequest request = null;
            IRestResponse<TResult> response = null;

            if (apiSetting.IsDisabled.HasValue && apiSetting.IsDisabled.Value)
            {
                if (apiSetting.MockService.Enabled.HasValue && apiSetting.MockService.Enabled.Value)
                {
                    return CreateResponseForApiCallWithMockService<TResult>(apiSetting, Method.POST, null, parameters, headers, requestFormat, trackingId);
                }
                return this.CreateResponseForDisabledApiCall<TResult>(trackingId, apiSetting, Method.POST, null, parameters, headers);
            }

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                request = this.PrepareRequest(apiSetting, Method.POST, requestFormat, parameters, headers);

                this.InterceptRequest(trackingId, apiSetting, request);

                var client = this.PrepareClient(apiSetting);
                resource = client.BuildUri(request)?.ToString();
                response = client.Execute<TResult>(request);

                this.InterceptResponse(trackingId, apiSetting, request, response, out var shouldRetryPrevRequest);

                return shouldRetryPrevRequest ? this.Post<TResult>(apiSetting, parameters, headers, requestFormat, trackingId) :
                                                this.CreateApiResponse(trackingId, apiSetting, response);
            }
            catch (ApiException apiException)
            {
                exception = apiException.InnerException ?? apiException;
                throw;
            }
            catch (Exception ex)
            {
                exception = ex;
                this.Logger.Error(exception);
                this.ThrowApiException(trackingId, apiSetting, response, exception, HttpStatusCode.InternalServerError);

                return null;
            }
            finally
            {
                stopwatch.Stop();
                this.LogApiRequestWithOtherInfo(trackingId, this.Source, resource ?? $"{apiSetting.BaseUrl}/{apiSetting.Path}", request, response, stopwatch.Elapsed.TotalMilliseconds, exception, null, apiSetting.IncludeRequestHeadersInTrackingLog, apiSetting.IncludeResponseHeadersInTrackingLog, GetApiTrackingOtherInfo(apiSetting));
            }
        }

        /// <summary>
        /// Posts the specified base URL.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="baseUrl">The base URL.</param>
        /// <param name="path">The path.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="headers">The request headers.</param>
        /// <param name="requestFormat">The request format.</param>
        /// <param name="trackingId">The tracking identifier.</param>
        /// <returns></returns>
        public virtual TResult Post<TResult>(string baseUrl, string path, IDictionary<string, string> parameters = null, IDictionary<string, string> headers = null, DataFormat requestFormat = DataFormat.Json, string trackingId = null)
            where TResult : class, new()
        {
            string resource = null;
            Exception exception = null;
            IRestRequest request = null;
            IRestResponse<TResult> response = null;

            var apiSetting = this.GetApiSetting(path);
            if (apiSetting.IsDisabled.HasValue && apiSetting.IsDisabled.Value)
            {
                if (apiSetting.MockService.Enabled.HasValue && apiSetting.MockService.Enabled.Value)
                {
                    return CreateResponseForApiCallWithMockService<TResult>(apiSetting, Method.POST, null, parameters, headers, requestFormat, trackingId);
                }
                return this.CreateResponseForDisabledApiCall<TResult>(trackingId, apiSetting, Method.POST, null, parameters, headers);
            }

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                request = this.PrepareRequest(apiSetting, Method.POST, requestFormat, parameters, headers);

                this.InterceptRequest(trackingId, apiSetting, request);

                var client = this.PrepareClient(apiSetting);
                resource = client.BuildUri(request)?.ToString();
                response = client.Execute<TResult>(request);

                this.InterceptResponse(trackingId, apiSetting, request, response, out var shouldRetryPrevRequest);

                return shouldRetryPrevRequest ? this.Post<TResult>(baseUrl, path, parameters, headers, requestFormat, trackingId) :
                                                this.CreateApiResponse(trackingId, apiSetting, response);
            }
            catch (ApiException apiException)
            {
                exception = apiException.InnerException ?? apiException;
                throw;
            }
            catch (Exception ex)
            {
                exception = ex;
                this.Logger.Error(exception);
                this.ThrowApiException(trackingId, baseUrl, path, response, exception, HttpStatusCode.InternalServerError);

                return null;
            }
            finally
            {
                stopwatch.Stop();
                this.LogApiRequestWithOtherInfo(trackingId, this.Source, resource ?? $"{baseUrl}/{path}", request, response, stopwatch.Elapsed.TotalMilliseconds, null, null, apiSetting.IncludeRequestHeadersInTrackingLog, apiSetting.IncludeResponseHeadersInTrackingLog, GetApiTrackingOtherInfo(apiSetting));
            }
        }

        /// <summary>
        /// Posts the specified API setting.
        /// </summary>
        /// <typeparam name="T">The type of request body</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="apiSetting">The API setting.</param>
        /// <param name="body">The body.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="headers">The request headers.</param>
        /// <param name="requestFormat">The request format.</param>
        /// <param name="trackingId">The tracking identifier.</param>
        /// <returns></returns>
        public TResult Post<T, TResult>(ApiSetting apiSetting, T body, IDictionary<string, string> parameters = null, IDictionary<string, string> headers = null, DataFormat requestFormat = DataFormat.Json, string trackingId = null)
            where T : class, new()
            where TResult : class, new()
        {
            string resource = null;
            Exception exception = null;
            IRestRequest request = null;
            IRestResponse<TResult> response = null;

            if (apiSetting.IsDisabled.HasValue && apiSetting.IsDisabled.Value)
            {
                if (apiSetting.MockService.Enabled.HasValue && apiSetting.MockService.Enabled.Value)
                {
                    return CreateResponseForApiCallWithMockService<TResult>(apiSetting, Method.POST, body, parameters, headers, requestFormat, trackingId);
                }
                return this.CreateResponseForDisabledApiCall<TResult>(trackingId, apiSetting, Method.POST, body, parameters, headers);
            }

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                request = this.PrepareRequest(apiSetting, Method.POST, requestFormat, body, parameters, headers);

                this.InterceptRequest(trackingId, apiSetting, request);

                var client = this.PrepareClient(apiSetting);
                resource = client.BuildUri(request)?.ToString();
                response = client.Execute<TResult>(request);

                this.InterceptResponse(trackingId, apiSetting, request, response, out var shouldRetryPrevRequest);

                return shouldRetryPrevRequest ? this.Post<T, TResult>(apiSetting, body, parameters, headers, requestFormat, trackingId) :
                                                this.CreateApiResponse(trackingId, apiSetting, response);
            }
            catch (ApiException apiException)
            {
                exception = apiException.InnerException ?? apiException;
                throw;
            }
            catch (Exception ex)
            {
                exception = ex;
                this.Logger.Error(exception);
                this.ThrowApiException(trackingId, apiSetting, response, exception, HttpStatusCode.InternalServerError);

                return null;
            }
            finally
            {
                stopwatch.Stop();
                this.LogApiRequestWithOtherInfo(trackingId, this.Source, resource ?? $"{apiSetting.BaseUrl}/{apiSetting.Path}", request, response, stopwatch.Elapsed.TotalMilliseconds, exception, null, apiSetting.IncludeRequestHeadersInTrackingLog, apiSetting.IncludeResponseHeadersInTrackingLog, GetApiTrackingOtherInfo(apiSetting));
            }
        }

        /// <summary>
        /// Posts the specified base URL.
        /// </summary>
        /// <typeparam name="T">The type of request body</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="baseUrl">The base URL.</param>
        /// <param name="path">The path.</param>
        /// <param name="body">The body.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="headers">The request headers.</param>
        /// <param name="requestFormat">The request format.</param>
        /// <param name="trackingId">The tracking identifier.</param>
        /// <returns></returns>
        public virtual TResult Post<T, TResult>(string baseUrl, string path, T body, IDictionary<string, string> parameters = null, IDictionary<string, string> headers = null, DataFormat requestFormat = DataFormat.Json, string trackingId = null)
            where T : class, new()
            where TResult : class, new()
        {
            string resource = null;
            Exception exception = null;
            IRestRequest request = null;
            IRestResponse<TResult> response = null;

            var apiSetting = this.GetApiSetting(path);
            if (apiSetting.IsDisabled.HasValue && apiSetting.IsDisabled.Value)
            {
                if (apiSetting.MockService.Enabled.HasValue && apiSetting.MockService.Enabled.Value)
                {
                    return CreateResponseForApiCallWithMockService<TResult>(apiSetting, Method.POST, body, parameters, headers, requestFormat, trackingId);
                }
                return this.CreateResponseForDisabledApiCall<TResult>(trackingId, apiSetting, Method.POST, body, parameters, headers);
            }

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                request = this.PrepareRequest(apiSetting, Method.POST, requestFormat, body, parameters, headers);

                this.InterceptRequest(trackingId, apiSetting, request);

                var client = this.PrepareClient(apiSetting);
                resource = client.BuildUri(request)?.ToString();
                response = client.Execute<TResult>(request);

                this.InterceptResponse(trackingId, apiSetting, request, response, out var shouldRetryPrevRequest);

                return shouldRetryPrevRequest ? this.Post<T, TResult>(baseUrl, path, body, parameters, headers, requestFormat, trackingId) :
                                                this.CreateApiResponse(trackingId, apiSetting, response);
            }
            catch (ApiException apiException)
            {
                exception = apiException.InnerException ?? apiException;
                throw;
            }
            catch (Exception ex)
            {
                exception = ex;
                this.Logger.Error(exception);
                this.ThrowApiException(trackingId, baseUrl, path, response, exception, HttpStatusCode.InternalServerError);

                return null;
            }
            finally
            {
                stopwatch.Stop();
                this.LogApiRequestWithOtherInfo(trackingId, this.Source, resource ?? $"{baseUrl}/{path}", request, response, stopwatch.Elapsed.TotalMilliseconds, exception, null, apiSetting.IncludeRequestHeadersInTrackingLog, apiSetting.IncludeResponseHeadersInTrackingLog, GetApiTrackingOtherInfo(apiSetting));
            }
        }

        /// <summary>
        /// Consumes a Restful API as HTTP POST method.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="apiSetting">The API setting.</param>
        /// <param name="parameters">The request query parameters.</param>
        /// <param name="headers">The request headers.</param>
        /// <param name="requestFormat">The request data format.</param>
        /// <param name="trackingId">The tracking identifier.</param>
        /// <returns></returns>
        public IRestResponse<TResult> PostForRestResponse<TResult>(ApiSetting apiSetting, IDictionary<string, string> parameters = null, IDictionary<string, string> headers = null, DataFormat requestFormat = DataFormat.Json, string trackingId = null) where TResult : class, new()
        {
            string resource = null;
            Exception exception = null;
            IRestRequest request = null;
            IRestResponse<TResult> response = null;

            if (apiSetting.IsDisabled.HasValue && apiSetting.IsDisabled.Value)
            {
                if (apiSetting.MockService.Enabled.HasValue && apiSetting.MockService.Enabled.Value)
                {
                    return CreateRestResponseForApiCallWithMockService<TResult>(apiSetting, Method.POST, null, parameters, headers, requestFormat, trackingId);
                }
                return this.CreateRestResponseForDisabledApiCall<TResult>(trackingId, apiSetting, Method.POST, null, parameters, headers);
            }



            var stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                request = this.PrepareRequest(apiSetting, Method.POST, requestFormat, parameters, headers);

                this.InterceptRequest(trackingId, apiSetting, request);

                var client = this.PrepareClient(apiSetting);
                resource = client.BuildUri(request)?.ToString();
                response = client.Execute<TResult>(request);

                this.InterceptResponse(trackingId, apiSetting, request, response, out var shouldRetryPrevRequest);

                return shouldRetryPrevRequest ? this.PostForRestResponse<TResult>(apiSetting, parameters, headers, requestFormat, trackingId) : response;
            }
            catch (Exception ex)
            {
                exception = ex;
                this.Logger.Error(exception);
                throw;
            }
            finally
            {
                stopwatch.Stop();
                this.LogApiRequestWithOtherInfo(trackingId, this.Source, resource ?? $"{apiSetting.BaseUrl}/{apiSetting.Path}", request, response, stopwatch.Elapsed.TotalMilliseconds, exception, null, apiSetting.IncludeRequestHeadersInTrackingLog, apiSetting.IncludeResponseHeadersInTrackingLog, GetApiTrackingOtherInfo(apiSetting));
            }
        }

        /// <summary>
        /// Consumes a Restful API as HTTP POST method.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="baseUrl">The base URL.</param>
        /// <param name="path">The api path.</param>
        /// <param name="parameters">The request query parameters.</param>
        /// <param name="headers">The request headers.</param>
        /// <param name="requestFormat">The request data format.</param>
        /// <param name="trackingId">The tracking identifier.</param>
        /// <returns></returns>
        public virtual IRestResponse<TResult> PostForRestResponse<TResult>(string baseUrl, string path, IDictionary<string, string> parameters = null, IDictionary<string, string> headers = null, DataFormat requestFormat = DataFormat.Json, string trackingId = null) where TResult : class, new()
        {
            string resource = null;
            Exception exception = null;
            IRestRequest request = null;
            IRestResponse<TResult> response = null;

            var apiSetting = this.GetApiSetting(path);
            if (apiSetting.IsDisabled.HasValue && apiSetting.IsDisabled.Value)
            {
                if (apiSetting.MockService.Enabled.HasValue && apiSetting.MockService.Enabled.Value)
                {
                    return CreateRestResponseForApiCallWithMockService<TResult>(apiSetting, Method.POST, null, parameters, headers, requestFormat, trackingId);
                }
                return this.CreateRestResponseForDisabledApiCall<TResult>(trackingId, apiSetting, Method.POST, null, parameters, headers);
            }

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                request = this.PrepareRequest(apiSetting, Method.POST, requestFormat, parameters, headers);

                this.InterceptRequest(trackingId, apiSetting, request);

                var client = this.PrepareClient(apiSetting);
                resource = client.BuildUri(request)?.ToString();
                response = client.Execute<TResult>(request);

                this.InterceptResponse(trackingId, apiSetting, request, response, out var shouldRetryPrevRequest);

                return shouldRetryPrevRequest ? this.PostForRestResponse<TResult>(baseUrl, path, parameters, headers, requestFormat, trackingId) : response;
            }
            catch (Exception ex)
            {
                exception = ex;
                this.Logger.Error(exception);

                throw;
            }
            finally
            {
                stopwatch.Stop();
                this.LogApiRequestWithOtherInfo(trackingId, this.Source, resource ?? $"{baseUrl}/{path}", request, response, stopwatch.Elapsed.TotalMilliseconds, null, null, apiSetting.IncludeRequestHeadersInTrackingLog, apiSetting.IncludeResponseHeadersInTrackingLog, GetApiTrackingOtherInfo(apiSetting));
            }
        }

        /// <summary>
        /// Consumes a Restful API as HTTP POST method.
        /// </summary>
        /// <typeparam name="T">The type of request body</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="apiSetting">The API setting.</param>
        /// <param name="body">The body.</param>
        /// <param name="parameters">The request query parameters.</param>
        /// <param name="headers">The request headers.</param>
        /// <param name="requestFormat">The request data format.</param>
        /// <param name="trackingId">The tracking identifier.</param>
        /// <returns></returns>
        public IRestResponse<TResult> PostForRestResponse<T, TResult>(ApiSetting apiSetting, T body, IDictionary<string, string> parameters = null, IDictionary<string, string> headers = null, DataFormat requestFormat = DataFormat.Json, string trackingId = null)
            where T : class, new()
            where TResult : class, new()
        {
            string resource = null;
            Exception exception = null;
            IRestRequest request = null;
            IRestResponse<TResult> response = null;

            if (apiSetting.IsDisabled.HasValue && apiSetting.IsDisabled.Value)
            {
                if (apiSetting.MockService.Enabled.HasValue && apiSetting.MockService.Enabled.Value)
                {
                    return CreateRestResponseForApiCallWithMockService<TResult>(apiSetting, Method.POST, body, parameters, headers, requestFormat, trackingId);
                }
                return this.CreateRestResponseForDisabledApiCall<TResult>(trackingId, apiSetting, Method.POST, body, parameters, headers);
            }

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                request = this.PrepareRequest(apiSetting, Method.POST, requestFormat, body, parameters, headers);

                this.InterceptRequest(trackingId, apiSetting, request);

                var client = this.PrepareClient(apiSetting);
                resource = client.BuildUri(request)?.ToString();
                response = client.Execute<TResult>(request);

                this.InterceptResponse(trackingId, apiSetting, request, response, out var shouldRetryPrevRequest);

                return shouldRetryPrevRequest ? this.PostForRestResponse<T, TResult>(apiSetting, body, parameters, headers, requestFormat, trackingId) : response;
            }
            catch (Exception ex)
            {
                exception = ex;
                this.Logger.Error(exception);

                throw;
            }
            finally
            {
                stopwatch.Stop();
                this.LogApiRequestWithOtherInfo(trackingId, this.Source, resource ?? $"{apiSetting.BaseUrl}/{apiSetting.Path}", request, response, stopwatch.Elapsed.TotalMilliseconds, exception, null, apiSetting.IncludeRequestHeadersInTrackingLog, apiSetting.IncludeResponseHeadersInTrackingLog, GetApiTrackingOtherInfo(apiSetting));
            }
        }

        /// <summary>
        /// Consumes a Restful API as HTTP POST method.
        /// </summary>
        /// <typeparam name="T">The type of request body</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="baseUrl">The base URL.</param>
        /// <param name="path">The api path.</param>
        /// <param name="body">The body.</param>
        /// <param name="parameters">The request query parameters.</param>
        /// <param name="headers">The request headers.</param>
        /// <param name="requestFormat">The request data format.</param>
        /// <param name="trackingId">The tracking identifier.</param>
        /// <returns></returns>
        public virtual IRestResponse<TResult> PostForRestResponse<T, TResult>(string baseUrl, string path, T body, IDictionary<string, string> parameters = null, IDictionary<string, string> headers = null, DataFormat requestFormat = DataFormat.Json, string trackingId = null)
            where T : class, new()
            where TResult : class, new()
        {
            string resource = null;
            Exception exception = null;
            IRestRequest request = null;
            IRestResponse<TResult> response = null;

            var apiSetting = this.GetApiSetting(path);
            if (apiSetting.IsDisabled.HasValue && apiSetting.IsDisabled.Value)
            {
                if (apiSetting.MockService.Enabled.HasValue && apiSetting.MockService.Enabled.Value)
                {
                    return CreateRestResponseForApiCallWithMockService<TResult>(apiSetting, Method.POST, body, parameters, headers, requestFormat, trackingId);
                }
                return this.CreateRestResponseForDisabledApiCall<TResult>(trackingId, apiSetting, Method.POST, body, parameters, headers);
            }

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                request = this.PrepareRequest(apiSetting, Method.POST, requestFormat, body, parameters, headers);

                this.InterceptRequest(trackingId, apiSetting, request);

                var client = this.PrepareClient(apiSetting);
                resource = client.BuildUri(request)?.ToString();
                response = client.Execute<TResult>(request);

                this.InterceptResponse(trackingId, apiSetting, request, response, out var shouldRetryPrevRequest);

                return shouldRetryPrevRequest ? this.PostForRestResponse<T, TResult>(baseUrl, path, body, parameters, headers, requestFormat, trackingId) : response;
            }
            catch (Exception ex)
            {
                exception = ex;
                this.Logger.Error(exception);

                throw;
            }
            finally
            {
                stopwatch.Stop();
                this.LogApiRequestWithOtherInfo(trackingId, this.Source, resource ?? $"{baseUrl}/{path}", request, response, stopwatch.Elapsed.TotalMilliseconds, exception, null, apiSetting.IncludeRequestHeadersInTrackingLog, apiSetting.IncludeResponseHeadersInTrackingLog, GetApiTrackingOtherInfo(apiSetting));
            }
        }

        /// <summary>
        /// Consumes a Restful API as HTTP PUT method.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="apiSetting">The API setting.</param>
        /// <param name="parameters">The request query parameters.</param>
        /// <param name="headers">The request headers.</param>
        /// <param name="requestFormat">The request data format.</param>
        /// <param name="trackingId">The tracking identifier.</param>
        /// <returns></returns>
        public IRestResponse<TResult> PutForRestResponse<TResult>(ApiSetting apiSetting, IDictionary<string, string> parameters = null, IDictionary<string, string> headers = null, DataFormat requestFormat = DataFormat.Json, string trackingId = null) where TResult : class, new()
        {
            string resource = null;
            Exception exception = null;
            IRestRequest request = null;
            IRestResponse<TResult> response = null;

            if (apiSetting.IsDisabled.HasValue && apiSetting.IsDisabled.Value)
            {
                if (apiSetting.MockService.Enabled.HasValue && apiSetting.MockService.Enabled.Value)
                {
                    return CreateRestResponseForApiCallWithMockService<TResult>(apiSetting, Method.POST, null, parameters, headers, requestFormat, trackingId);
                }
                return this.CreateRestResponseForDisabledApiCall<TResult>(trackingId, apiSetting, Method.PUT, null, parameters, headers);
            }



            var stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                request = this.PrepareRequest(apiSetting, Method.PUT, requestFormat, parameters, headers);

                this.InterceptRequest(trackingId, apiSetting, request);

                var client = this.PrepareClient(apiSetting);
                resource = client.BuildUri(request)?.ToString();
                response = client.Execute<TResult>(request);

                this.InterceptResponse(trackingId, apiSetting, request, response, out var shouldRetryPrevRequest);

                return shouldRetryPrevRequest ? this.PutForRestResponse<TResult>(apiSetting, parameters, headers, requestFormat, trackingId) : response;
            }
            catch (Exception ex)
            {
                exception = ex;
                this.Logger.Error(exception);
                throw;
            }
            finally
            {
                stopwatch.Stop();
                this.LogApiRequestWithOtherInfo(trackingId, this.Source, resource ?? $"{apiSetting.BaseUrl}/{apiSetting.Path}", request, response, stopwatch.Elapsed.TotalMilliseconds, exception, null, apiSetting.IncludeRequestHeadersInTrackingLog, apiSetting.IncludeResponseHeadersInTrackingLog, GetApiTrackingOtherInfo(apiSetting));
            }
        }

        /// <summary>
        /// Consumes a Restful API as HTTP PUT method.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="baseUrl">The base URL.</param>
        /// <param name="path">The api path.</param>
        /// <param name="parameters">The request query parameters.</param>
        /// <param name="headers">The request headers.</param>
        /// <param name="requestFormat">The request data format.</param>
        /// <param name="trackingId">The tracking identifier.</param>
        /// <returns></returns>
        public virtual IRestResponse<TResult> PutForRestResponse<TResult>(string baseUrl, string path, IDictionary<string, string> parameters = null, IDictionary<string, string> headers = null, DataFormat requestFormat = DataFormat.Json, string trackingId = null) where TResult : class, new()
        {
            string resource = null;
            Exception exception = null;
            IRestRequest request = null;
            IRestResponse<TResult> response = null;

            var apiSetting = this.GetApiSetting(path);
            if (apiSetting.IsDisabled.HasValue && apiSetting.IsDisabled.Value)
            {
                if (apiSetting.MockService.Enabled.HasValue && apiSetting.MockService.Enabled.Value)
                {
                    return CreateRestResponseForApiCallWithMockService<TResult>(apiSetting, Method.POST, null, parameters, headers, requestFormat, trackingId);
                }
                return this.CreateRestResponseForDisabledApiCall<TResult>(trackingId, apiSetting, Method.PUT, null, parameters, headers);
            }

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                request = this.PrepareRequest(apiSetting, Method.PUT, requestFormat, parameters, headers);

                this.InterceptRequest(trackingId, apiSetting, request);

                var client = this.PrepareClient(apiSetting);
                resource = client.BuildUri(request)?.ToString();
                response = client.Execute<TResult>(request);

                this.InterceptResponse(trackingId, apiSetting, request, response, out var shouldRetryPrevRequest);

                return shouldRetryPrevRequest ? this.PutForRestResponse<TResult>(baseUrl, path, parameters, headers, requestFormat, trackingId) : response;
            }
            catch (Exception ex)
            {
                exception = ex;
                this.Logger.Error(exception);

                throw;
            }
            finally
            {
                stopwatch.Stop();
                this.LogApiRequestWithOtherInfo(trackingId, this.Source, resource ?? $"{baseUrl}/{path}", request, response, stopwatch.Elapsed.TotalMilliseconds, null, null, apiSetting.IncludeRequestHeadersInTrackingLog, apiSetting.IncludeResponseHeadersInTrackingLog, GetApiTrackingOtherInfo(apiSetting));
            }
        }

        /// <summary>
        /// Consumes a Restful API as HTTP PUT method.
        /// </summary>
        /// <typeparam name="T">The type of request body</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="apiSetting">The API setting.</param>
        /// <param name="body">The body.</param>
        /// <param name="parameters">The request query parameters.</param>
        /// <param name="headers">The request headers.</param>
        /// <param name="requestFormat">The request data format.</param>
        /// <param name="trackingId">The tracking identifier.</param>
        /// <returns></returns>
        public IRestResponse<TResult> PutForRestResponse<T, TResult>(ApiSetting apiSetting, T body, IDictionary<string, string> parameters = null, IDictionary<string, string> headers = null, DataFormat requestFormat = DataFormat.Json, string trackingId = null)
            where T : class, new()
            where TResult : class, new()
        {
            string resource = null;
            Exception exception = null;
            IRestRequest request = null;
            IRestResponse<TResult> response = null;

            if (apiSetting.IsDisabled.HasValue && apiSetting.IsDisabled.Value)
            {
                if (apiSetting.MockService.Enabled.HasValue && apiSetting.MockService.Enabled.Value)
                {
                    return CreateRestResponseForApiCallWithMockService<TResult>(apiSetting, Method.POST, body, parameters, headers, requestFormat, trackingId);
                }
                return this.CreateRestResponseForDisabledApiCall<TResult>(trackingId, apiSetting, Method.PUT, body, parameters, headers);
            }

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                request = this.PrepareRequest(apiSetting, Method.PUT, requestFormat, body, parameters, headers);

                this.InterceptRequest(trackingId, apiSetting, request);

                var client = this.PrepareClient(apiSetting);
                resource = client.BuildUri(request)?.ToString();
                response = client.Execute<TResult>(request);

                this.InterceptResponse(trackingId, apiSetting, request, response, out var shouldRetryPrevRequest);

                return shouldRetryPrevRequest ? this.PutForRestResponse<T, TResult>(apiSetting, body, parameters, headers, requestFormat, trackingId) : response;
            }
            catch (Exception ex)
            {
                exception = ex;
                this.Logger.Error(exception);

                throw;
            }
            finally
            {
                stopwatch.Stop();
                this.LogApiRequestWithOtherInfo(trackingId, this.Source, resource ?? $"{apiSetting.BaseUrl}/{apiSetting.Path}", request, response, stopwatch.Elapsed.TotalMilliseconds, exception, null, apiSetting.IncludeRequestHeadersInTrackingLog, apiSetting.IncludeResponseHeadersInTrackingLog, GetApiTrackingOtherInfo(apiSetting));
            }
        }

        /// <summary>
        /// Consumes a Restful API as HTTP PUT method.
        /// </summary>
        /// <typeparam name="T">The type of request body</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="baseUrl">The base URL.</param>
        /// <param name="path">The api path.</param>
        /// <param name="body">The body.</param>
        /// <param name="parameters">The request query parameters.</param>
        /// <param name="headers">The request headers.</param>
        /// <param name="requestFormat">The request data format.</param>
        /// <param name="trackingId">The tracking identifier.</param>
        /// <returns></returns>
        public virtual IRestResponse<TResult> PutForRestResponse<T, TResult>(string baseUrl, string path, T body, IDictionary<string, string> parameters = null, IDictionary<string, string> headers = null, DataFormat requestFormat = DataFormat.Json, string trackingId = null)
            where T : class, new()
            where TResult : class, new()
        {
            string resource = null;
            Exception exception = null;
            IRestRequest request = null;
            IRestResponse<TResult> response = null;

            var apiSetting = this.GetApiSetting(path);
            if (apiSetting.IsDisabled.HasValue && apiSetting.IsDisabled.Value)
            {
                if (apiSetting.MockService.Enabled.HasValue && apiSetting.MockService.Enabled.Value)
                {
                    return CreateRestResponseForApiCallWithMockService<TResult>(apiSetting, Method.POST, body, parameters, headers, requestFormat, trackingId);
                }
                return this.CreateRestResponseForDisabledApiCall<TResult>(trackingId, apiSetting, Method.PUT, body, parameters, headers);
            }

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                request = this.PrepareRequest(apiSetting, Method.PUT, requestFormat, body, parameters, headers);

                this.InterceptRequest(trackingId, apiSetting, request);

                var client = this.PrepareClient(apiSetting);
                resource = client.BuildUri(request)?.ToString();
                response = client.Execute<TResult>(request);

                this.InterceptResponse(trackingId, apiSetting, request, response, out var shouldRetryPrevRequest);

                return shouldRetryPrevRequest ? this.PutForRestResponse<T, TResult>(baseUrl, path, body, parameters, headers, requestFormat, trackingId) : response;
            }
            catch (Exception ex)
            {
                exception = ex;
                this.Logger.Error(exception);

                throw;
            }
            finally
            {
                stopwatch.Stop();
                this.LogApiRequestWithOtherInfo(trackingId, this.Source, resource ?? $"{baseUrl}/{path}", request, response, stopwatch.Elapsed.TotalMilliseconds, exception, null, apiSetting.IncludeRequestHeadersInTrackingLog, apiSetting.IncludeResponseHeadersInTrackingLog, GetApiTrackingOtherInfo(apiSetting));
            }
        }

        /// <summary>
        /// Consumes a Restful API as HTTP DELETE method.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="apiSetting">The API setting.</param>
        /// <param name="parameters">The request query parameters.</param>
        /// <param name="headers">The request headers.</param>
        /// <param name="requestFormat">The request data format.</param>
        /// <param name="trackingId">The tracking identifier.</param>
        /// <returns></returns>
        public IRestResponse<TResult> DeleteForRestResponse<TResult>(ApiSetting apiSetting, IDictionary<string, string> parameters = null, IDictionary<string, string> headers = null, DataFormat requestFormat = DataFormat.Json, string trackingId = null) where TResult : class, new()
        {
            string resource = null;
            Exception exception = null;
            IRestRequest request = null;
            IRestResponse<TResult> response = null;

            if (apiSetting.IsDisabled.HasValue && apiSetting.IsDisabled.Value)
            {
                if (apiSetting.MockService.Enabled.HasValue && apiSetting.MockService.Enabled.Value)
                {
                    return CreateRestResponseForApiCallWithMockService<TResult>(apiSetting, Method.POST, null, parameters, headers, requestFormat, trackingId);
                }
                return this.CreateRestResponseForDisabledApiCall<TResult>(trackingId, apiSetting, Method.DELETE, null, parameters, headers);
            }



            var stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                request = this.PrepareRequest(apiSetting, Method.DELETE, requestFormat, parameters, headers);

                this.InterceptRequest(trackingId, apiSetting, request);

                var client = this.PrepareClient(apiSetting);
                resource = client.BuildUri(request)?.ToString();
                response = client.Execute<TResult>(request);

                this.InterceptResponse(trackingId, apiSetting, request, response, out var shouldRetryPrevRequest);

                return shouldRetryPrevRequest ? this.DeleteForRestResponse<TResult>(apiSetting, parameters, headers, requestFormat, trackingId) : response;
            }
            catch (Exception ex)
            {
                exception = ex;
                this.Logger.Error(exception);
                throw;
            }
            finally
            {
                stopwatch.Stop();
                this.LogApiRequestWithOtherInfo(trackingId, this.Source, resource ?? $"{apiSetting.BaseUrl}/{apiSetting.Path}", request, response, stopwatch.Elapsed.TotalMilliseconds, exception, null, apiSetting.IncludeRequestHeadersInTrackingLog, apiSetting.IncludeResponseHeadersInTrackingLog, GetApiTrackingOtherInfo(apiSetting));
            }
        }

        /// <summary>
        /// Consumes a Restful API as HTTP DELETE method.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="baseUrl">The base URL.</param>
        /// <param name="path">The api path.</param>
        /// <param name="parameters">The request query parameters.</param>
        /// <param name="headers">The request headers.</param>
        /// <param name="requestFormat">The request data format.</param>
        /// <param name="trackingId">The tracking identifier.</param>
        /// <returns></returns>
        public virtual IRestResponse<TResult> DeleteForRestResponse<TResult>(string baseUrl, string path, IDictionary<string, string> parameters = null, IDictionary<string, string> headers = null, DataFormat requestFormat = DataFormat.Json, string trackingId = null) where TResult : class, new()
        {
            string resource = null;
            Exception exception = null;
            IRestRequest request = null;
            IRestResponse<TResult> response = null;

            var apiSetting = this.GetApiSetting(path);
            if (apiSetting.IsDisabled.HasValue && apiSetting.IsDisabled.Value)
            {
                if (apiSetting.MockService.Enabled.HasValue && apiSetting.MockService.Enabled.Value)
                {
                    return CreateRestResponseForApiCallWithMockService<TResult>(apiSetting, Method.POST, null, parameters, headers, requestFormat, trackingId);
                }
                return this.CreateRestResponseForDisabledApiCall<TResult>(trackingId, apiSetting, Method.DELETE, null, parameters, headers);
            }

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                request = this.PrepareRequest(apiSetting, Method.DELETE, requestFormat, parameters, headers);

                this.InterceptRequest(trackingId, apiSetting, request);

                var client = this.PrepareClient(apiSetting);
                resource = client.BuildUri(request)?.ToString();
                response = client.Execute<TResult>(request);

                this.InterceptResponse(trackingId, apiSetting, request, response, out var shouldRetryPrevRequest);

                return shouldRetryPrevRequest ? this.DeleteForRestResponse<TResult>(baseUrl, path, parameters, headers, requestFormat, trackingId) : response;
            }
            catch (Exception ex)
            {
                exception = ex;
                this.Logger.Error(exception);

                throw;
            }
            finally
            {
                stopwatch.Stop();
                this.LogApiRequestWithOtherInfo(trackingId, this.Source, resource ?? $"{baseUrl}/{path}", request, response, stopwatch.Elapsed.TotalMilliseconds, null, null, apiSetting.IncludeRequestHeadersInTrackingLog, apiSetting.IncludeResponseHeadersInTrackingLog, GetApiTrackingOtherInfo(apiSetting));
            }
        }

        /// <summary>
        /// Consumes a Restful API as HTTP DELETE method.
        /// </summary>
        /// <typeparam name="T">The type of request body</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="apiSetting">The API setting.</param>
        /// <param name="body">The body.</param>
        /// <param name="parameters">The request query parameters.</param>
        /// <param name="headers">The request headers.</param>
        /// <param name="requestFormat">The request data format.</param>
        /// <param name="trackingId">The tracking identifier.</param>
        /// <returns></returns>
        public IRestResponse<TResult> DeleteForRestResponse<T, TResult>(ApiSetting apiSetting, T body, IDictionary<string, string> parameters = null, IDictionary<string, string> headers = null, DataFormat requestFormat = DataFormat.Json, string trackingId = null)
            where T : class, new()
            where TResult : class, new()
        {
            string resource = null;
            Exception exception = null;
            IRestRequest request = null;
            IRestResponse<TResult> response = null;

            if (apiSetting.IsDisabled.HasValue && apiSetting.IsDisabled.Value)
            {
                if (apiSetting.MockService.Enabled.HasValue && apiSetting.MockService.Enabled.Value)
                {
                    return CreateRestResponseForApiCallWithMockService<TResult>(apiSetting, Method.POST, body, parameters, headers, requestFormat, trackingId);
                }
                return this.CreateRestResponseForDisabledApiCall<TResult>(trackingId, apiSetting, Method.DELETE, body, parameters, headers);
            }

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                request = this.PrepareRequest(apiSetting, Method.DELETE, requestFormat, body, parameters, headers);

                this.InterceptRequest(trackingId, apiSetting, request);

                var client = this.PrepareClient(apiSetting);
                resource = client.BuildUri(request)?.ToString();
                response = client.Execute<TResult>(request);

                this.InterceptResponse(trackingId, apiSetting, request, response, out var shouldRetryPrevRequest);

                return shouldRetryPrevRequest ? this.DeleteForRestResponse<T, TResult>(apiSetting, body, parameters, headers, requestFormat, trackingId) : response;
            }
            catch (Exception ex)
            {
                exception = ex;
                this.Logger.Error(exception);

                throw;
            }
            finally
            {
                stopwatch.Stop();
                this.LogApiRequestWithOtherInfo(trackingId, this.Source, resource ?? $"{apiSetting.BaseUrl}/{apiSetting.Path}", request, response, stopwatch.Elapsed.TotalMilliseconds, exception, null, apiSetting.IncludeRequestHeadersInTrackingLog, apiSetting.IncludeResponseHeadersInTrackingLog, GetApiTrackingOtherInfo(apiSetting));
            }
        }

        /// <summary>
        /// Consumes a Restful API as HTTP DELETE method.
        /// </summary>
        /// <typeparam name="T">The type of request body</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="baseUrl">The base URL.</param>
        /// <param name="path">The api path.</param>
        /// <param name="body">The body.</param>
        /// <param name="parameters">The request query parameters.</param>
        /// <param name="headers">The request headers.</param>
        /// <param name="requestFormat">The request data format.</param>
        /// <param name="trackingId">The tracking identifier.</param>
        /// <returns></returns>
        public virtual IRestResponse<TResult> DeleteForRestResponse<T, TResult>(string baseUrl, string path, T body, IDictionary<string, string> parameters = null, IDictionary<string, string> headers = null, DataFormat requestFormat = DataFormat.Json, string trackingId = null)
            where T : class, new()
            where TResult : class, new()
        {
            string resource = null;
            Exception exception = null;
            IRestRequest request = null;
            IRestResponse<TResult> response = null;

            var apiSetting = this.GetApiSetting(path);
            if (apiSetting.IsDisabled.HasValue && apiSetting.IsDisabled.Value)
            {
                if (apiSetting.MockService.Enabled.HasValue && apiSetting.MockService.Enabled.Value)
                {
                    return CreateRestResponseForApiCallWithMockService<TResult>(apiSetting, Method.POST, body, parameters, headers, requestFormat, trackingId);
                }
                return this.CreateRestResponseForDisabledApiCall<TResult>(trackingId, apiSetting, Method.DELETE, body, parameters, headers);
            }

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                request = this.PrepareRequest(apiSetting, Method.DELETE, requestFormat, body, parameters, headers);

                this.InterceptRequest(trackingId, apiSetting, request);

                var client = this.PrepareClient(apiSetting);
                resource = client.BuildUri(request)?.ToString();
                response = client.Execute<TResult>(request);

                this.InterceptResponse(trackingId, apiSetting, request, response, out var shouldRetryPrevRequest);

                return shouldRetryPrevRequest ? this.DeleteForRestResponse<T, TResult>(baseUrl, path, body, parameters, headers, requestFormat, trackingId) : response;
            }
            catch (Exception ex)
            {
                exception = ex;
                this.Logger.Error(exception);

                throw;
            }
            finally
            {
                stopwatch.Stop();
                this.LogApiRequestWithOtherInfo(trackingId, this.Source, resource ?? $"{baseUrl}/{path}", request, response, stopwatch.Elapsed.TotalMilliseconds, exception, null, apiSetting.IncludeRequestHeadersInTrackingLog, apiSetting.IncludeResponseHeadersInTrackingLog, GetApiTrackingOtherInfo(apiSetting));
            }
        }

        #endregion

        #region FormPost

        /// <summary>
        /// Consumes a Restful API as Form Data.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="apiSetting">The API setting.</param>
        /// <param name="parameters">The request query parameters.</param>
        /// <param name="headers">The request headers.</param>
        /// <param name="trackingId">The tracking identifier.</param>
        /// <returns></returns>
        public TResult PostAsForm<TResult>(ApiSetting apiSetting, IDictionary<string, string> parameters = null, IDictionary<string, string> headers = null, string trackingId = null)
            where TResult : class, new()
        {
            string resource = null;
            Exception exception = null;
            IRestRequest request = null;
            IRestResponse<TResult> response = null;

            if (apiSetting.IsDisabled.HasValue && apiSetting.IsDisabled.Value)
            {
                return this.CreateResponseForDisabledApiCall<TResult>(trackingId, apiSetting, Method.POST, null, parameters, headers);
            }

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                request = this.PrepareFormRequest(apiSetting, parameters, headers);

                this.InterceptRequest(trackingId, apiSetting, request);

                var client = this.PrepareClient(apiSetting);
                resource = client.BuildUri(request)?.ToString();
                response = client.Execute<TResult>(request);

                this.InterceptResponse(trackingId, apiSetting, request, response, out var shouldRetryPrevRequest);

                return shouldRetryPrevRequest ? this.PostAsForm<TResult>(apiSetting, parameters, headers, trackingId) :
                                                this.CreateApiResponse(trackingId, apiSetting, response);
            }
            catch (ApiException apiException)
            {
                exception = apiException.InnerException ?? apiException;
                throw;
            }
            catch (Exception ex)
            {
                exception = ex;
                this.Logger.Error(exception);
                this.ThrowApiException(trackingId, apiSetting, response, exception, HttpStatusCode.InternalServerError);

                return null;
            }
            finally
            {
                stopwatch.Stop();
                this.LogApiRequestWithOtherInfo(trackingId, this.Source, resource ?? $"{apiSetting.BaseUrl}/{apiSetting.Path}", request, response, stopwatch.Elapsed.TotalMilliseconds, exception, null, apiSetting.IncludeRequestHeadersInTrackingLog, apiSetting.IncludeResponseHeadersInTrackingLog, GetApiTrackingOtherInfo(apiSetting));
            }
        }

        /// <summary>
        /// Consumes a Restful API as Form Data.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="baseUrl">The base URL.</param>
        /// <param name="path">The api path.</param>
        /// <param name="parameters">The request query parameters.</param>
        /// <param name="headers">The request headers.</param>
        /// <param name="trackingId">The tracking identifier.</param>
        /// <returns></returns>
        public virtual TResult PostAsForm<TResult>(string baseUrl, string path, IDictionary<string, string> parameters = null, IDictionary<string, string> headers = null, string trackingId = null)
            where TResult : class, new()
        {
            string resource = null;
            Exception exception = null;
            IRestRequest request = null;
            IRestResponse<TResult> response = null;

            var apiSetting = this.GetApiSetting(path);
            if (apiSetting.IsDisabled.HasValue && apiSetting.IsDisabled.Value)
            {
                return this.CreateResponseForDisabledApiCall<TResult>(trackingId, apiSetting, Method.POST, null, parameters, headers);
            }

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                request = this.PrepareFormRequest(apiSetting, parameters, headers);

                this.InterceptRequest(trackingId, apiSetting, request);

                var client = this.PrepareClient(apiSetting);
                resource = client.BuildUri(request)?.ToString();
                response = client.Execute<TResult>(request);

                this.InterceptResponse(trackingId, apiSetting, request, response, out var shouldRetryPrevRequest);

                return shouldRetryPrevRequest ? this.PostAsForm<TResult>(baseUrl, path, parameters, headers, trackingId) :
                                                this.CreateApiResponse(trackingId, apiSetting, response);
            }
            catch (ApiException apiException)
            {
                exception = apiException.InnerException ?? apiException;
                throw;
            }
            catch (Exception ex)
            {
                exception = ex;
                this.Logger.Error(exception);
                this.ThrowApiException(trackingId, baseUrl, path, response, exception, HttpStatusCode.InternalServerError);

                return null;
            }
            finally
            {
                stopwatch.Stop();
                this.LogApiRequestWithOtherInfo(trackingId, this.Source, resource ?? $"{baseUrl}/{path}", request, response, stopwatch.Elapsed.TotalMilliseconds, exception, null, apiSetting.IncludeRequestHeadersInTrackingLog, apiSetting.IncludeResponseHeadersInTrackingLog, GetApiTrackingOtherInfo(apiSetting));
            }
        }

        /// <summary>
        /// Consumes a Restful API as Form Data.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="apiSetting">The API setting.</param>
        /// <param name="parameters">The request query parameters.</param>
        /// <param name="headers">The request headers.</param>
        /// <param name="trackingId">The tracking identifier.</param>
        /// <returns></returns>
        public IRestResponse<TResult> PostAsFormForRestResponse<TResult>(ApiSetting apiSetting, IDictionary<string, string> parameters = null, IDictionary<string, string> headers = null, string trackingId = null) where TResult : class, new()
        {
            string resource = null;
            Exception exception = null;
            IRestRequest request = null;
            IRestResponse<TResult> response = null;

            if (apiSetting.IsDisabled.HasValue && apiSetting.IsDisabled.Value)
            {
                return this.CreateRestResponseForDisabledApiCall<TResult>(trackingId, apiSetting, Method.POST, null, parameters, headers);
            }

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                request = this.PrepareFormRequest(apiSetting, parameters, headers);

                this.InterceptRequest(trackingId, apiSetting, request);

                var client = this.PrepareClient(apiSetting);
                resource = client.BuildUri(request)?.ToString();
                response = client.Execute<TResult>(request);

                this.InterceptResponse(trackingId, apiSetting, request, response, out var shouldRetryPrevRequest);

                return shouldRetryPrevRequest ? this.PostAsFormForRestResponse<TResult>(apiSetting, parameters, headers, trackingId) : response;
            }
            catch (Exception ex)
            {
                exception = ex;
                this.Logger.Error(exception);

                throw;
            }
            finally
            {
                stopwatch.Stop();
                this.LogApiRequestWithOtherInfo(trackingId, this.Source, resource ?? $"{apiSetting.BaseUrl}/{apiSetting.Path}", request, response, stopwatch.Elapsed.TotalMilliseconds, exception, null, apiSetting.IncludeRequestHeadersInTrackingLog, apiSetting.IncludeResponseHeadersInTrackingLog, GetApiTrackingOtherInfo(apiSetting));
            }
        }

        /// <summary>
        /// Consumes a Restful API as Form Data.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="baseUrl">The base URL.</param>
        /// <param name="path">The api path.</param>
        /// <param name="parameters">The request query parameters.</param>
        /// <param name="headers">The request headers.</param>
        /// <param name="trackingId">The tracking identifier.</param>
        /// <returns></returns>
        public virtual IRestResponse<TResult> PostAsFormForRestResponse<TResult>(string baseUrl, string path, IDictionary<string, string> parameters = null, IDictionary<string, string> headers = null, string trackingId = null) where TResult : class, new()
        {
            string resource = null;
            Exception exception = null;
            IRestRequest request = null;
            IRestResponse<TResult> response = null;

            var apiSetting = this.GetApiSetting(path);
            if (apiSetting.IsDisabled.HasValue && apiSetting.IsDisabled.Value)
            {
                return this.CreateRestResponseForDisabledApiCall<TResult>(trackingId, apiSetting, Method.POST, null, parameters, headers);
            }

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                request = this.PrepareFormRequest(apiSetting, parameters, headers);

                this.InterceptRequest(trackingId, apiSetting, request);

                var client = this.PrepareClient(apiSetting);
                resource = client.BuildUri(request)?.ToString();
                response = client.Execute<TResult>(request);

                this.InterceptResponse(trackingId, apiSetting, request, response, out var shouldRetryPrevRequest);

                return shouldRetryPrevRequest ? this.PostAsFormForRestResponse<TResult>(baseUrl, path, parameters, headers, trackingId) : response;
            }
            catch (Exception ex)
            {
                exception = ex;
                this.Logger.Error(exception);

                throw;
            }
            finally
            {
                stopwatch.Stop();
                this.LogApiRequestWithOtherInfo(trackingId, this.Source, resource ?? $"{baseUrl}/{path}", request, response, stopwatch.Elapsed.TotalMilliseconds, exception, null, apiSetting.IncludeRequestHeadersInTrackingLog, apiSetting.IncludeResponseHeadersInTrackingLog, GetApiTrackingOtherInfo(apiSetting));
            }
        }

        #endregion

        #region Send asynchronous

        /// <summary>
        /// Gets the asynchronous.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="apiSetting">The API setting.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="headers">The request headers.</param>
        /// <param name="requestFormat">The request format.</param>
        /// <param name="trackingId">The tracking identifier.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<TResult> GetAsync<TResult>(ApiSetting apiSetting, IDictionary<string, string> parameters = null, IDictionary<string, string> headers = null, DataFormat requestFormat = DataFormat.Json, string trackingId = null)
            where TResult : class, new()
        {
            Exception exception = null;
            IRestRequest request = null;
            IRestResponse<TResult> response = null;

            if (apiSetting.IsDisabled.HasValue && apiSetting.IsDisabled.Value)
            {
                return this.CreateResponseForDisabledApiCall<TResult>(trackingId, apiSetting, Method.GET, null, parameters, headers);
            }

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                request = this.PrepareRequest(apiSetting, Method.GET, requestFormat, parameters, headers);
                this.InterceptRequest(trackingId, apiSetting, request);
                var client = this.PrepareClient(apiSetting);

                response = await client.ExecuteGetTaskAsync<TResult>(request);
                this.InterceptResponse(trackingId, apiSetting, request, response, out var shouldRetryPrevRequest);

                return await (shouldRetryPrevRequest ? this.GetAsync<TResult>(apiSetting, parameters, headers, requestFormat, trackingId) :
                                               Task.FromResult<TResult>(this.CreateApiResponse(trackingId, apiSetting, response)));
            }
            catch (ApiException apiException)
            {
                exception = apiException.InnerException ?? apiException;
                throw;
            }
            catch (Exception ex)
            {
                exception = ex;
                this.Logger.Error(exception);
                this.ThrowApiException(trackingId, apiSetting, response, exception, HttpStatusCode.InternalServerError);

                return null;
            }
            finally
            {
                stopwatch.Stop();
                this.LogApiRequestWithOtherInfo(trackingId, this.Source, $"{apiSetting.BaseUrl}/{apiSetting.Path}", request, response, stopwatch.Elapsed.TotalMilliseconds, exception, null, apiSetting.IncludeRequestHeadersInTrackingLog, apiSetting.IncludeResponseHeadersInTrackingLog, GetApiTrackingOtherInfo(apiSetting));
            }
        }

        /// <summary>
        /// Gets the asynchronous.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="baseUrl">The base URL.</param>
        /// <param name="path">The path.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="headers">The request headers.</param>
        /// <param name="requestFormat">The request format.</param>
        /// <param name="trackingId">The tracking identifier.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public virtual async Task<TResult> GetAsync<TResult>(string baseUrl, string path, IDictionary<string, string> parameters = null, IDictionary<string, string> headers = null, DataFormat requestFormat = DataFormat.Json, string trackingId = null)
            where TResult : class, new()
        {
            string resource = null;
            Exception exception = null;
            IRestRequest request = null;
            IRestResponse<TResult> response = null;

            var apiSetting = this.GetApiSetting(path);
            if (apiSetting.IsDisabled.HasValue && apiSetting.IsDisabled.Value)
            {
                return this.CreateResponseForDisabledApiCall<TResult>(trackingId, apiSetting, Method.GET, null, parameters, headers);
            }

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                request = this.PrepareRequest(apiSetting, Method.GET, requestFormat, parameters, headers);

                this.InterceptRequest(trackingId, apiSetting, request);

                var client = this.PrepareClient(apiSetting);
                resource = client.BuildUri(request)?.ToString();
                response = await client.ExecuteGetTaskAsync<TResult>(request);

                this.InterceptResponse(trackingId, apiSetting, request, response, out var shouldRetryPrevRequest);

                return await (shouldRetryPrevRequest ? this.GetAsync<TResult>(baseUrl, path, parameters, headers, requestFormat, trackingId) :
                                                Task.FromResult<TResult>(this.CreateApiResponse(trackingId, apiSetting, response)));
            }
            catch (ApiException apiException)
            {
                exception = apiException.InnerException ?? apiException;
                throw;
            }
            catch (Exception ex)
            {
                exception = ex;
                this.Logger.Error(exception);
                this.ThrowApiException(trackingId, baseUrl, path, response, exception, HttpStatusCode.InternalServerError);

                return null;
            }
            finally
            {
                stopwatch.Stop();
                this.LogApiRequestWithOtherInfo(trackingId, this.Source, resource ?? $"{baseUrl}/{path}", request, response, stopwatch.Elapsed.TotalMilliseconds, exception, null, apiSetting.IncludeRequestHeadersInTrackingLog, apiSetting.IncludeResponseHeadersInTrackingLog, GetApiTrackingOtherInfo(apiSetting));
            }
        }

        /// <summary>
        /// Posts the asynchronous.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="apiSetting">The API setting.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="headers">The request headers.</param>
        /// <param name="requestFormat">The request format.</param>
        /// <param name="trackingId">The tracking identifier.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<TResult> PostAsync<TResult>(ApiSetting apiSetting, IDictionary<string, string> parameters = null, IDictionary<string, string> headers = null, DataFormat requestFormat = DataFormat.Json, string trackingId = null)
            where TResult : class, new()
        {
            Exception exception = null;
            IRestRequest request = null;
            IRestResponse<TResult> response = null;

            if (apiSetting.IsDisabled.HasValue && apiSetting.IsDisabled.Value)
            {
                return this.CreateResponseForDisabledApiCall<TResult>(trackingId, apiSetting, Method.GET, null, parameters, headers);
            }

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                request = this.PrepareRequest(apiSetting, Method.POST, requestFormat, parameters, headers);
                this.InterceptRequest(trackingId, apiSetting, request);
                var client = this.PrepareClient(apiSetting);

                response = await client.ExecutePostTaskAsync<TResult>(request);
                this.InterceptResponse(trackingId, apiSetting, request, response, out var shouldRetryPrevRequest);

                return await (shouldRetryPrevRequest ? this.PostAsync<TResult>(apiSetting, parameters, headers, requestFormat, trackingId) :
                                                Task.FromResult<TResult>(this.CreateApiResponse(trackingId, apiSetting, response)));
            }
            catch (ApiException apiException)
            {
                exception = apiException.InnerException ?? apiException;
                throw;
            }
            catch (Exception ex)
            {
                exception = ex;
                this.Logger.Error(exception);
                this.ThrowApiException(trackingId, apiSetting, response, exception, HttpStatusCode.InternalServerError);

                return null;
            }
            finally
            {
                stopwatch.Stop();
                this.LogApiRequestWithOtherInfo(trackingId, this.Source, $"{apiSetting.BaseUrl}/{apiSetting.Path}", request, response, stopwatch.Elapsed.TotalMilliseconds, exception, null, apiSetting.IncludeRequestHeadersInTrackingLog, apiSetting.IncludeResponseHeadersInTrackingLog, GetApiTrackingOtherInfo(apiSetting));
            }
        }

        /// <summary>
        /// Posts the asynchronous.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="baseUrl">The base URL.</param>
        /// <param name="path">The path.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="headers">The request headers.</param>
        /// <param name="requestFormat">The request format.</param>
        /// <param name="trackingId">The tracking identifier.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public virtual async Task<TResult> PostAsync<TResult>(string baseUrl, string path, IDictionary<string, string> parameters = null, IDictionary<string, string> headers = null, DataFormat requestFormat = DataFormat.Json, string trackingId = null)
            where TResult : class, new()
        {
            string resource = null;
            Exception exception = null;
            IRestRequest request = null;
            IRestResponse<TResult> response = null;

            var apiSetting = this.GetApiSetting(path);
            if (apiSetting.IsDisabled.HasValue && apiSetting.IsDisabled.Value)
            {
                return this.CreateResponseForDisabledApiCall<TResult>(trackingId, apiSetting, Method.POST, null, parameters, headers);
            }

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                request = this.PrepareRequest(apiSetting, Method.POST, requestFormat, parameters, headers);

                this.InterceptRequest(trackingId, apiSetting, request);

                var client = this.PrepareClient(apiSetting);
                resource = client.BuildUri(request)?.ToString();
                response = await client.ExecutePostTaskAsync<TResult>(request);

                this.InterceptResponse(trackingId, apiSetting, request, response, out var shouldRetryPrevRequest);

                return await (shouldRetryPrevRequest ? this.PostAsync<TResult>(baseUrl, path, parameters, headers, requestFormat, trackingId) :
                                                Task.FromResult<TResult>(this.CreateApiResponse(trackingId, apiSetting, response)));
            }
            catch (ApiException apiException)
            {
                exception = apiException.InnerException ?? apiException;
                throw;
            }
            catch (Exception ex)
            {
                exception = ex;
                this.Logger.Error(exception);
                this.ThrowApiException(trackingId, baseUrl, path, response, exception, HttpStatusCode.InternalServerError);

                return null;
            }
            finally
            {
                stopwatch.Stop();
                this.LogApiRequestWithOtherInfo(trackingId, this.Source, resource ?? $"{baseUrl}/{path}", request, response, stopwatch.Elapsed.TotalMilliseconds, null, null, apiSetting.IncludeRequestHeadersInTrackingLog, apiSetting.IncludeResponseHeadersInTrackingLog, GetApiTrackingOtherInfo(apiSetting));
            }
        }

        /// <summary>
        /// Posts the asynchronous.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="apiSetting">The API setting.</param>
        /// <param name="body">The body.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="headers">The request headers.</param>
        /// <param name="requestFormat">The request format.</param>
        /// <param name="trackingId">The tracking identifier.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<TResult> PostAsync<T, TResult>(ApiSetting apiSetting, T body, IDictionary<string, string> parameters = null, IDictionary<string, string> headers = null, DataFormat requestFormat = DataFormat.Json, string trackingId = null)
            where T : class, new()
            where TResult : class, new()
        {
            Exception exception = null;
            IRestRequest request = null;
            IRestResponse<TResult> response = null;

            if (apiSetting.IsDisabled.HasValue && apiSetting.IsDisabled.Value)
            {
                return this.CreateResponseForDisabledApiCall<TResult>(trackingId, apiSetting, Method.POST, null, parameters, headers);
            }

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                request = this.PrepareRequest(apiSetting, Method.POST, requestFormat, body, parameters, headers);

                this.InterceptRequest(trackingId, apiSetting, request);
                var client = this.PrepareClient(apiSetting);

                response = await client.ExecutePostTaskAsync<TResult>(request);
                this.InterceptResponse(trackingId, apiSetting, request, response, out var shouldRetryPrevRequest);

                return await (shouldRetryPrevRequest ? this.PostAsync<T, TResult>(apiSetting, body, parameters, headers, requestFormat, trackingId) :
                                               Task.FromResult<TResult>(this.CreateApiResponse(trackingId, apiSetting, response)));
            }
            catch (ApiException apiException)
            {
                exception = apiException.InnerException ?? apiException;
                throw;
            }
            catch (Exception ex)
            {
                exception = ex;
                this.Logger.Error(exception);
                this.ThrowApiException(trackingId, apiSetting, response, exception, HttpStatusCode.InternalServerError);

                return null;
            }
            finally
            {
                stopwatch.Stop();
                this.LogApiRequestWithOtherInfo(trackingId, this.Source, $"{apiSetting.BaseUrl}/{apiSetting.Path}", request, response, stopwatch.Elapsed.TotalMilliseconds, exception, null, apiSetting.IncludeRequestHeadersInTrackingLog, apiSetting.IncludeResponseHeadersInTrackingLog, GetApiTrackingOtherInfo(apiSetting));
            }
        }

        /// <summary>
        /// Posts the asynchronous.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="baseUrl">The base URL.</param>
        /// <param name="path">The path.</param>
        /// <param name="body">The body.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="headers">The request headers.</param>
        /// <param name="requestFormat">The request format.</param>
        /// <param name="trackingId">The tracking identifier.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public virtual async Task<TResult> PostAsync<T, TResult>(string baseUrl, string path, T body, IDictionary<string, string> parameters = null, IDictionary<string, string> headers = null, DataFormat requestFormat = DataFormat.Json, string trackingId = null)
            where T : class, new()
            where TResult : class, new()
        {
            string resource = null;
            Exception exception = null;
            IRestRequest request = null;
            IRestResponse<TResult> response = null;

            var apiSetting = this.GetApiSetting(path);
            if (apiSetting.IsDisabled.HasValue && apiSetting.IsDisabled.Value)
            {
                return this.CreateResponseForDisabledApiCall<TResult>(trackingId, apiSetting, Method.POST, body, parameters, headers);
            }

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                request = this.PrepareRequest(apiSetting, Method.POST, requestFormat, body, parameters, headers);

                this.InterceptRequest(trackingId, apiSetting, request);

                var client = this.PrepareClient(apiSetting);
                resource = client.BuildUri(request)?.ToString();
                response = await client.ExecutePostTaskAsync<TResult>(request);

                this.InterceptResponse(trackingId, apiSetting, request, response, out var shouldRetryPrevRequest);

                return await (shouldRetryPrevRequest ? this.PostAsync<T, TResult>(baseUrl, path, body, parameters, headers, requestFormat, trackingId) :
                                                Task.FromResult<TResult>(this.CreateApiResponse(trackingId, apiSetting, response)));
            }
            catch (ApiException apiException)
            {
                exception = apiException.InnerException ?? apiException;
                throw;
            }
            catch (Exception ex)
            {
                exception = ex;
                this.Logger.Error(exception);
                this.ThrowApiException(trackingId, baseUrl, path, response, exception, HttpStatusCode.InternalServerError);

                return null;
            }
            finally
            {
                stopwatch.Stop();
                this.LogApiRequestWithOtherInfo(trackingId, this.Source, resource ?? $"{baseUrl}/{path}", request, response, stopwatch.Elapsed.TotalMilliseconds, exception, null, apiSetting.IncludeRequestHeadersInTrackingLog, apiSetting.IncludeResponseHeadersInTrackingLog, GetApiTrackingOtherInfo(apiSetting));
            }
        }

        /// <summary>
        /// Posts As Form the asynchronous.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="apiSetting">The API setting.</param>
        /// <param name="body">The body.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="headers">The request headers.</param>
        /// <param name="requestFormat">The request format.</param>
        /// <param name="trackingId">The tracking identifier.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<TResult> PostAsFormAsync<TResult>(ApiSetting apiSetting, IDictionary<string, string> parameters = null, IDictionary<string, string> headers = null, string trackingId = null) where TResult : class, new()
        {
            Exception exception = null;
            IRestRequest request = null;
            IRestResponse<TResult> response = null;

            if (apiSetting.IsDisabled.HasValue && apiSetting.IsDisabled.Value)
            {
                return this.CreateResponseForDisabledApiCall<TResult>(trackingId, apiSetting, Method.POST, null, parameters, headers);
            }

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                request = this.PrepareFormRequest(apiSetting, parameters, headers);
                this.InterceptRequest(trackingId, apiSetting, request);
                var client = this.PrepareClient(apiSetting);

                response = await client.ExecutePostTaskAsync<TResult>(request);
                this.InterceptResponse(trackingId, apiSetting, request, response, out var shouldRetryPrevRequest);

                return await (shouldRetryPrevRequest ? this.PostAsFormAsync<TResult>(apiSetting, parameters, headers, trackingId) :
                                                Task.FromResult<TResult>(this.CreateApiResponse(trackingId, apiSetting, response)));
            }
            catch (ApiException apiException)
            {
                exception = apiException.InnerException ?? apiException;
                throw;
            }
            catch (Exception ex)
            {
                exception = ex;
                this.Logger.Error(exception);
                this.ThrowApiException(trackingId, apiSetting, response, exception, HttpStatusCode.InternalServerError);

                return null;
            }
            finally
            {
                stopwatch.Stop();
                this.LogApiRequestWithOtherInfo(trackingId, this.Source, $"{apiSetting.BaseUrl}/{apiSetting.Path}", request, response, stopwatch.Elapsed.TotalMilliseconds, exception, null, apiSetting.IncludeRequestHeadersInTrackingLog, apiSetting.IncludeResponseHeadersInTrackingLog, GetApiTrackingOtherInfo(apiSetting));
            }
        }

        /// <summary>
        /// Posts As Form the asynchronous.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="baseUrl">The base URL.</param>
        /// <param name="path">The path.</param>
        /// <param name="body">The body.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="headers">The request headers.</param>
        /// <param name="requestFormat">The request format.</param>
        /// <param name="trackingId">The tracking identifier.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public virtual async Task<TResult> PostAsFormAsync<TResult>(string baseUrl, string path, IDictionary<string, string> parameters = null, IDictionary<string, string> headers = null, string trackingId = null) where TResult : class, new()
        {
            string resource = null;
            Exception exception = null;
            IRestRequest request = null;
            IRestResponse<TResult> response = null;

            var apiSetting = this.GetApiSetting(path);
            if (apiSetting.IsDisabled.HasValue && apiSetting.IsDisabled.Value)
            {
                return this.CreateResponseForDisabledApiCall<TResult>(trackingId, apiSetting, Method.POST, null, parameters, headers);
            }

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                request = this.PrepareFormRequest(apiSetting, parameters, headers);

                this.InterceptRequest(trackingId, apiSetting, request);

                var client = this.PrepareClient(apiSetting);
                resource = client.BuildUri(request)?.ToString();
                response = await client.ExecutePostTaskAsync<TResult>(request);

                this.InterceptResponse(trackingId, apiSetting, request, response, out var shouldRetryPrevRequest);

                return await (shouldRetryPrevRequest ? this.PostAsFormAsync<TResult>(baseUrl, path, parameters, headers, trackingId) :
                                                Task.FromResult<TResult>(this.CreateApiResponse(trackingId, apiSetting, response)));
            }
            catch (ApiException apiException)
            {
                exception = apiException.InnerException ?? apiException;
                throw;
            }
            catch (Exception ex)
            {
                exception = ex;
                this.Logger.Error(exception);
                this.ThrowApiException(trackingId, baseUrl, path, response, exception, HttpStatusCode.InternalServerError);

                return null;
            }
            finally
            {
                stopwatch.Stop();
                this.LogApiRequestWithOtherInfo(trackingId, this.Source, resource ?? $"{baseUrl}/{path}", request, response, stopwatch.Elapsed.TotalMilliseconds, exception, null, apiSetting.IncludeRequestHeadersInTrackingLog, apiSetting.IncludeResponseHeadersInTrackingLog, GetApiTrackingOtherInfo(apiSetting));
            }
        }

        /// <summary>
        /// Consumes a Restful API as Form Data the asynchronous.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="apiSetting">The API setting.</param>
        /// <param name="parameters">The request query parameters.</param>
        /// <param name="headers">The request headers.</param>
        /// <param name="trackingId">The tracking identifier.</param>
        /// <returns></returns>
        public async Task<IRestResponse<TResult>> PostAsFormForRestResponseAsync<TResult>(ApiSetting apiSetting, IDictionary<string, string> parameters = null, IDictionary<string, string> headers = null, string trackingId = null) where TResult : class, new()
        {
            string resource = null;
            Exception exception = null;
            IRestRequest request = null;
            IRestResponse<TResult> response = null;

            if (apiSetting.IsDisabled.HasValue && apiSetting.IsDisabled.Value)
            {
                return this.CreateRestResponseForDisabledApiCall<TResult>(trackingId, apiSetting, Method.POST, null, parameters, headers);
            }

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                request = this.PrepareFormRequest(apiSetting, parameters, headers);

                this.InterceptRequest(trackingId, apiSetting, request);

                var client = this.PrepareClient(apiSetting);
                resource = client.BuildUri(request)?.ToString();
                response = await client.ExecutePostTaskAsync<TResult>(request);

                this.InterceptResponse(trackingId, apiSetting, request, response, out var shouldRetryPrevRequest);

                return await (shouldRetryPrevRequest ? this.PostAsFormForRestResponseAsync<TResult>(apiSetting, parameters, headers, trackingId)
                                                        : Task.FromResult<IRestResponse<TResult>>(response));
            }
            catch (Exception ex)
            {
                exception = ex;
                this.Logger.Error(exception);

                throw;
            }
            finally
            {
                stopwatch.Stop();
                this.LogApiRequestWithOtherInfo(trackingId, this.Source, resource ?? $"{apiSetting.BaseUrl}/{apiSetting.Path}", request, response, stopwatch.Elapsed.TotalMilliseconds, exception, null, apiSetting.IncludeRequestHeadersInTrackingLog, apiSetting.IncludeResponseHeadersInTrackingLog, GetApiTrackingOtherInfo(apiSetting));
            }
        }

        /// <summary>
        /// Consumes a Restful API as Form Data the asynchronous.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="baseUrl">The base URL.</param>
        /// <param name="path">The api path.</param>
        /// <param name="parameters">The request query parameters.</param>
        /// <param name="headers">The request headers.</param>
        /// <param name="trackingId">The tracking identifier.</param>
        /// <returns></returns>
        public virtual async Task<IRestResponse<TResult>> PostAsFormForRestResponseAsync<TResult>(string baseUrl, string path, IDictionary<string, string> parameters = null, IDictionary<string, string> headers = null, string trackingId = null) where TResult : class, new()
        {
            string resource = null;
            Exception exception = null;
            IRestRequest request = null;
            IRestResponse<TResult> response = null;

            var apiSetting = this.GetApiSetting(path);
            if (apiSetting.IsDisabled.HasValue && apiSetting.IsDisabled.Value)
            {
                return this.CreateRestResponseForDisabledApiCall<TResult>(trackingId, apiSetting, Method.POST, null, parameters, headers);
            }

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                request = this.PrepareFormRequest(apiSetting, parameters, headers);

                this.InterceptRequest(trackingId, apiSetting, request);

                var client = this.PrepareClient(apiSetting);
                resource = client.BuildUri(request)?.ToString();
                response = await client.ExecutePostTaskAsync<TResult>(request);

                this.InterceptResponse(trackingId, apiSetting, request, response, out var shouldRetryPrevRequest);

                return await (shouldRetryPrevRequest ? this.PostAsFormForRestResponseAsync<TResult>(baseUrl, path, parameters, headers, trackingId)
                                                        : Task.FromResult<IRestResponse<TResult>>(response));
            }
            catch (Exception ex)
            {
                exception = ex;
                this.Logger.Error(exception);

                throw;
            }
            finally
            {
                stopwatch.Stop();
                this.LogApiRequestWithOtherInfo(trackingId, this.Source, resource ?? $"{baseUrl}/{path}", request, response, stopwatch.Elapsed.TotalMilliseconds, exception, null, apiSetting.IncludeRequestHeadersInTrackingLog, apiSetting.IncludeResponseHeadersInTrackingLog, GetApiTrackingOtherInfo(apiSetting));
            }
        }
        #endregion

        #region Protected methods

        /// <summary>
        /// Gets the API setting.
        /// </summary>
        /// <param name="path">The request.</param>
        /// <returns></returns>
        public virtual ApiSetting GetApiSetting(string path)
        {
            return ApiSettingsManager.GetApiSetting(this.SettingFile, this.Source, path);
        }

        /// <summary>
        /// Prepares the client.
        /// </summary>
        /// <param name="apiSetting">The API setting.</param>
        /// <returns></returns>
        protected virtual IRestClient PrepareClient(ApiSetting apiSetting)
        {
            if (!apiSetting.Ssl.HasValue || apiSetting.Ssl.Value)
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            }

            if (apiSetting.BypassCertVerification.HasValue && apiSetting.BypassCertVerification.Value)
            {
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            }

            var restClient = new RestSharp.RestClient(apiSetting.BaseUrl);
            if (apiSetting.AuthenticationInfo != null && this.RestClientSettings.AuthenticatorProvider != null)
            {
                var authenticator = this.RestClientSettings.AuthenticatorProvider.GetAuthenticator(apiSetting);
                if (authenticator != null)
                {
                    restClient.Authenticator = authenticator;
                }
            }
            
            var clientCertificates = this.GetClientCertificates(apiSetting.ClientCertificateFile, apiSetting.ClientCertificatePassword);
            if (clientCertificates != null)
            {
                restClient.ClientCertificates = clientCertificates;
            }

            var proxySetting = apiSetting.ProxySetting;
            if (proxySetting != null && proxySetting.IsEnable && !string.IsNullOrEmpty(proxySetting.Address))
            {
                var proxy = new WebProxy(proxySetting.Address)
                {
                    BypassProxyOnLocal = proxySetting.BypassOnlocal
                };

                if (proxySetting.BypassList != null && proxySetting.BypassList.Length > 0)
                {
                    proxy.BypassList = proxySetting.BypassList;
                }

                if (!string.IsNullOrEmpty(proxySetting.UserName))
                {
                    proxy.Credentials = string.IsNullOrEmpty(proxySetting.Domain) ?
                        new NetworkCredential(proxySetting.UserName, proxySetting.Password) :
                        new NetworkCredential(proxySetting.UserName, proxySetting.Password, proxySetting.Domain);
                }

                restClient.Proxy = proxy;
            }


            // Replace JsonSerilaze for Reposne
            if (apiSetting.JsonSerializerProvider == JsonSerializerProvider.Newtonsoft)
            {
                // Override with Newtonsoft JSON Handler
                restClient.AddHandler("application/json", JsonRestSerializer.Default);
                restClient.AddHandler("text/json", JsonRestSerializer.Default);
                restClient.AddHandler("text/x-json", JsonRestSerializer.Default);
                restClient.AddHandler("text/javascript", JsonRestSerializer.Default);
                restClient.AddHandler("*+json", JsonRestSerializer.Default);
            }

            return restClient;
        }

        /// <summary>
        /// Prepares the form request.
        /// </summary>
        /// <param name="baseUrl">The base URL.</param>
        /// <param name="path">The path.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        protected virtual IRestRequest PrepareFormRequest(ApiSetting apiSetting, IDictionary<string, string> parameters = null, IDictionary<string, string> headers = null)
        {
            var request = new RestRequest(apiSetting.Path, Method.POST);

            if (apiSetting.JsonSerializerProvider == JsonSerializerProvider.Newtonsoft)
            {
                request.JsonSerializer = JsonRestSerializer.Default;
            }

            if (headers != null && headers.Count > 0)
            {
                foreach (var header in headers)
                {
                    request.AddHeader(header.Key, header.Value);
                }
            }

            if (parameters != null && parameters.Count > 0)
            {
                foreach (var parameter in parameters)
                {
                    request.AddParameter(parameter.Key, parameter.Value, ParameterType.GetOrPost);
                }
            }

            return request;
        }

        /// <summary>
        /// Prepares the request.
        /// </summary>
        /// <param name="baseUrl">The base URL.</param>
        /// <param name="path">The path.</param>
        /// <param name="method">The method.</param>
        /// <param name="requestFormat">The request format.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        protected virtual IRestRequest PrepareRequest(ApiSetting apiSetting, Method method, DataFormat requestFormat = DataFormat.Json, IDictionary<string, string> parameters = null, IDictionary<string, string> headers = null)
        {
            AdjustApiSettingBeforePrepareRequest(apiSetting, headers, parameters);

            var request = new RestRequest(apiSetting.Path, method)
            {
                RequestFormat = requestFormat
            };

            if (apiSetting.JsonSerializerProvider == JsonSerializerProvider.Newtonsoft)
            {
                request.JsonSerializer = JsonRestSerializer.Default;
            }

            if (headers != null && headers.Count > 0)
            {
                foreach (var header in headers)
                {
                    request.AddHeader(header.Key, header.Value);
                }
            }

            if (parameters != null && parameters.Count > 0)
            {
                foreach (var parameter in parameters)
                {
                    if (method == Method.GET)
                    {
                        request.AddQueryParameter(parameter.Key, parameter.Value);
                    }
                    else
                    {
                        request.AddParameter(parameter.Key, parameter.Value);
                    }
                }
            }

            return request;
        }

        /// <summary>
        /// Prepares the request.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="baseUrl">The base URL.</param>
        /// <param name="path">The path.</param>
        /// <param name="method">The method.</param>
        /// <param name="requestFormat">The request format.</param>
        /// <param name="body">The body.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        protected virtual IRestRequest PrepareRequest<T>(ApiSetting apiSetting, Method method, DataFormat requestFormat = DataFormat.Json, T body = default(T), IDictionary<string, string> parameters = null, IDictionary<string, string> headers = null)
        {
            AdjustApiSettingBeforePrepareRequest(apiSetting, headers, parameters);

            var request = new RestRequest(apiSetting.Path, method)
            {
                RequestFormat = requestFormat
            };

            if (apiSetting.JsonSerializerProvider == JsonSerializerProvider.Newtonsoft)
            {
                request.JsonSerializer = JsonRestSerializer.Default;
            }

            if (headers != null && headers.Count > 0)
            {
                foreach (var header in headers)
                {
                    request.AddHeader(header.Key, header.Value);
                }
            }

            if (parameters != null && parameters.Count > 0)
            {
                foreach (var parameter in parameters)
                {
                    if (method == Method.GET)
                    {
                        request.AddQueryParameter(parameter.Key, parameter.Value);
                    }
                    else
                    {
                        request.AddParameter(parameter.Key, parameter.Value);
                    }
                }
            }

            if (body != null)
            {
                switch (requestFormat)
                {
                    case DataFormat.Json:
                        request.AddJsonBody(body);
                        break;

                    case DataFormat.Xml:
                        request.AddXmlBody(body);
                        break;

                    default:
                        request.AddObject(body);
                        break;
                }
            }

            return request;
        }

        /// <summary>
        /// Intercepts the request.
        /// </summary>
        /// <param name="trackingId">The tracking identifier.</param>
        /// <param name="apiSetting">The API setting.</param>
        /// <param name="request">The request.</param>
        protected virtual void InterceptRequest(string trackingId, ApiSetting apiSetting, IRestRequest request)
        {
        }

        /// <summary>
        /// Intercepts the response.
        /// </summary>
        /// <param name="trackingId">The tracking identifier.</param>
        /// <param name="apiSetting">The API setting.</param>
        /// <param name="response">The response.</param>
        /// <param name="shouldRetryPrevRequest">if set to <c>true</c> [should retry previous request].</param>
        protected abstract void InterceptResponse(string trackingId, ApiSetting apiSetting, IRestRequest request, IRestResponse response, out bool shouldRetryPrevRequest);

        /// <summary>
        /// Creates the API response.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="trackingId">The tracking identifier.</param>
        /// <param name="apiSetting">The API setting.</param>
        /// <param name="response">The response.</param>
        /// <returns></returns>
        protected virtual TResult CreateApiResponse<TResult>(string trackingId, ApiSetting apiSetting, IRestResponse<TResult> response)
            where TResult : class, new()
        {
            return response.Data;
        }

        /// <summary>
        /// Creates the response for disabled API call.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="trackingId">The tracking identifier.</param>
        /// <param name="apiSetting">The API setting.</param>
        /// <param name="method">The method.</param>
        /// <param name="body">The body.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="headers">The headers.</param>
        /// <returns>TResult.</returns>
        /// <exception cref="FileNotFoundException">Could not find the mock file</exception>
        protected virtual TResult CreateResponseForDisabledApiCall<TResult>(string trackingId, ApiSetting apiSetting, Method method,
            object body = null, IDictionary<string, string> parameters = null, IDictionary<string, string> headers = null)
            where TResult : class, new()
        {
            var mockFile = apiSetting.MockFile;
            if (string.IsNullOrWhiteSpace(mockFile))
            {
                this.Logger.Debug(string.Format(ApiCallHasBeenDisabled, $"{apiSetting.BaseUrl}/{apiSetting.Path}", trackingId));
                return default(TResult);
            }

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var mockFileName = ApiSettingsManager.GetMockFileName(mockFile);
            this.Logger.Debug(string.Format(ApiCallHasBeenDisabledAndLoadDataFromMockFile, $"{apiSetting.BaseUrl}/{apiSetting.Path}", mockFileName, trackingId));

            var cacheKey = $"MockFileCacheKey_{Path.GetFileNameWithoutExtension(mockFile)}";
            var content = FileCachingManager.Get<string>(cacheKey);
            if (content == null)
            {
                if (!File.Exists(mockFile))
                {
                    throw new FileNotFoundException($"Could not find the mock file [{mockFileName}].", mockFileName);
                }

                content = File.ReadAllText(mockFile);
                FileCachingManager.Set(cacheKey, content, mockFile);
            }

            TResult data = null;
            MockResponse<TResult> mockResponse = new MockResponse<TResult>();
            if (!string.IsNullOrWhiteSpace(content))
            {
                Exception exception = null;
                try
                {
                    mockResponse.DeserializeObject(content);

                    data = mockResponse.ResponseData;
                }
                catch (Exception ex)
                {
                    exception = ex;
                    throw ex;
                }
                finally
                {
                    stopwatch.Stop();

                    if (this.LoggingService != null)
                    {
                        var trackingInfo = this.GetApiTrackingInfo(trackingId, apiSetting, mockFileName, method, body, parameters, headers, content, exception, stopwatch.Elapsed.TotalMilliseconds);

                        trackingInfo.OtherInfo = GetApiTrackingOtherInfo(apiSetting);

                        trackingInfo.ResponseStatusCode = mockResponse.StatusCode;
                        this.LoggingService.LogRequest(trackingInfo);
                    }
                }
            }

            return data;
        }



        /// <summary>
        /// Creates the rest response for disabled API call.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="trackingId">The tracking identifier.</param>
        /// <param name="apiSetting">The API setting.</param>
        /// <param name="method">The method.</param>
        /// <param name="body">The body.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="headers">The headers.</param>
        /// <returns>IRestResponse&lt;TResult&gt;.</returns>
        /// <exception cref="FileNotFoundException">Could not find the mock file</exception>
        protected virtual IRestResponse<TResult> CreateRestResponseForDisabledApiCall<TResult>(string trackingId, ApiSetting apiSetting, Method method,
            object body = null, IDictionary<string, string> parameters = null, IDictionary<string, string> headers = null)
            where TResult : class, new()
        {
            TResult data = null;
            MockResponse<TResult> mockResponse = new MockResponse<TResult>();
            Exception exception = null;
            var mockFile = apiSetting.MockFile;

            if (string.IsNullOrWhiteSpace(mockFile))
            {
                this.Logger.Debug(string.Format(ApiCallHasBeenDisabled, $"{apiSetting.BaseUrl}/{apiSetting.Path}", trackingId));
                data = default(TResult);
            }
            else
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                var mockFileName = ApiSettingsManager.GetMockFileName(mockFile);
                this.Logger.Debug(string.Format(ApiCallHasBeenDisabledAndLoadDataFromMockFile, $"{apiSetting.BaseUrl}/{apiSetting.Path}", mockFileName, trackingId));

                var cacheKey = $"MockFileCacheKey_{Path.GetFileNameWithoutExtension(mockFile)}";
                var content = FileCachingManager.Get<string>(cacheKey);

                if (content == null)
                {
                    if (!File.Exists(mockFile))
                    {
                        throw new FileNotFoundException($"Could not find the mock file [{mockFileName}].", mockFileName);
                    }

                    content = File.ReadAllText(mockFile);
                    FileCachingManager.Set(cacheKey, content, mockFile);
                }

                if (!string.IsNullOrWhiteSpace(content))
                {
                    try
                    {
                        mockResponse.DeserializeObject(content);
                        data = mockResponse.ResponseData;
                    }
                    catch (Exception ex)
                    {
                        exception = ex;
                        throw ex;
                    }
                    finally
                    {
                        stopwatch.Stop();

                        if (this.LoggingService != null)
                        {
                            var trackingInfo = this.GetApiTrackingInfo(trackingId, apiSetting, mockFileName, method, body, parameters, headers, content, exception, stopwatch.Elapsed.TotalMilliseconds);

                            trackingInfo.OtherInfo = GetApiTrackingOtherInfo(apiSetting);
                            trackingInfo.ResponseStatusCode = mockResponse.StatusCode;
                            this.LoggingService.LogRequest(trackingInfo);
                        }
                    }
                }
            }

            return new RestResponse<TResult>
            {
                ResponseStatus = ResponseStatus.Completed,
                StatusCode = exception == null ? HttpStatusCode.OK : HttpStatusCode.InternalServerError,
                Data = data,
            };
        }

        protected virtual TResult CreateResponseForApiCallWithMockService<TResult>(ApiSetting apiSetting, Method method, object body, IDictionary<string, string> parameters = null, IDictionary<string, string> headers = null, DataFormat requestFormat = DataFormat.Json, string trackingId = null)
           where TResult : class, new()
        {
            string resource = null;
            Exception exception = null;
            IRestRequest request = null;
            IRestResponse<TResult> response = null;


            var stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                AdjustApiSettingBeforePrepareRequest(apiSetting, headers, parameters);

                if (method == Method.GET)
                {
                    request = this.PrepareRequest(apiSetting, method, requestFormat, parameters, headers);
                    request.Resource = "Get";
                }
                else
                {
                    request = this.PrepareRequest(apiSetting, method, requestFormat, body, parameters, headers);
                    request.Resource = "Post";
                }

                apiSetting.MockService?.AddHeaderRequest(request);

                this.InterceptRequest(trackingId, apiSetting, request);

                var client = this.PrepareClient(apiSetting);
                client.BaseUrl = new Uri(apiSetting.MockService.Url);
                resource = client.BuildUri(request)?.ToString();
                response = client.Execute<TResult>(request);
                
                this.InterceptResponse(trackingId, apiSetting, request, response, out var shouldRetryPrevRequest);
                this.Logger.Debug("response has null ? : " + response.Data == null);
                return this.CreateApiResponse(trackingId, apiSetting, response);
            }
            catch (ApiException apiException)
            {
                exception = apiException.InnerException ?? apiException;
                throw;
            }
            catch (Exception ex)
            {
                exception = ex;
                this.Logger.Error(exception);
                this.Logger.Debug("Happen Error in Core: " + exception);
                this.ThrowApiException(trackingId, apiSetting, response, exception, HttpStatusCode.InternalServerError);

                return null;
            }
            finally
            {
                stopwatch.Stop();
                this.LogApiRequestWithOtherInfo(trackingId, this.Source, resource ?? $"{apiSetting.BaseUrl}/{apiSetting.Path}", request, response, stopwatch.Elapsed.TotalMilliseconds, exception, null, apiSetting.IncludeRequestHeadersInTrackingLog, apiSetting.IncludeResponseHeadersInTrackingLog, GetApiTrackingOtherInfo(apiSetting));
            }
        }

        protected virtual IRestResponse<TResult> CreateRestResponseForApiCallWithMockService<TResult>(ApiSetting apiSetting, Method method, object body, IDictionary<string, string> parameters = null, IDictionary<string, string> headers = null, DataFormat requestFormat = DataFormat.Json, string trackingId = null)
           where TResult : class, new()
        {
            string resource = null;
            Exception exception = null;
            IRestRequest request = null;
            IRestResponse<TResult> response = null;


            var stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                AdjustApiSettingBeforePrepareRequest(apiSetting, headers, parameters);

                if (method == Method.GET)
                {
                    request = this.PrepareRequest(apiSetting, method, requestFormat, parameters, headers);
                    request.Resource = "Get";
                }
                else
                {
                    request = this.PrepareRequest(apiSetting, method, requestFormat, body, parameters, headers);
                    request.Resource = "Post";
                }

                apiSetting.MockService?.AddHeaderRequest(request);

                this.InterceptRequest(trackingId, apiSetting, request);

                var client = this.PrepareClient(apiSetting);
                client.BaseUrl = new Uri(apiSetting.MockService.Url);
                resource = client.BuildUri(request)?.ToString();
                response = client.Execute<TResult>(request);

                if (response != null && response.Data == null && !string.IsNullOrEmpty(response.Content))
                {
                    if (apiSetting.JsonSerializerProvider == JsonSerializerProvider.Newtonsoft)
                    {
                        response.Data = JsonConvert.DeserializeObject<TResult>(response.Content);
                    }
                    else
                    {
                        //
                    }
                }

                this.InterceptResponse(trackingId, apiSetting, request, response, out var shouldRetryPrevRequest);
                this.Logger.Debug("response has null ? : " + response.Data == null);
                return response;
            }
            catch (ApiException apiException)
            {
                exception = apiException.InnerException ?? apiException;
                throw;
            }
            catch (Exception ex)
            {
                exception = ex;
                this.Logger.Error(exception);
                this.Logger.Debug("Happen Error in Core: " + exception);
                this.ThrowApiException(trackingId, apiSetting, response, exception, HttpStatusCode.InternalServerError);

                return null;
            }
            finally
            {
                stopwatch.Stop();
                this.LogApiRequestWithOtherInfo(trackingId, this.Source, resource ?? $"{apiSetting.BaseUrl}/{apiSetting.Path}", request, response, stopwatch.Elapsed.TotalMilliseconds, exception, null, apiSetting.IncludeRequestHeadersInTrackingLog, apiSetting.IncludeResponseHeadersInTrackingLog, GetApiTrackingOtherInfo(apiSetting));
            }
        }

        /// <summary>
        /// Throws the API exception.
        /// </summary>
        /// <param name="trackingId">The tracking identifier.</param>
        /// <param name="apiSetting">The API setting.</param>
        /// <param name="response">The response.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="statusCode">The status code.</param>
        /// <exception cref="ApiException"></exception>
        protected virtual void ThrowApiException(string trackingId, ApiSetting apiSetting, IRestResponse response, Exception exception = null, HttpStatusCode? statusCode = null)
        {
            var responseStatusCode = statusCode ?? response?.StatusCode;
            var responseException = exception ?? response?.ErrorException;
            var message = string.Format(ApiError, $"{apiSetting.BaseUrl}/{apiSetting.Path}", responseStatusCode, response?.Content, trackingId);

            throw new ApiException(trackingId, message, responseException, responseStatusCode);
        }

        /// <summary>
        /// Throws the API exception.
        /// </summary>
        /// <param name="trackingId">The tracking identifier.</param>
        /// <param name="baseUrl">The base URL.</param>
        /// <param name="path">The path.</param>
        /// <param name="response">The response.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="statusCode">The status code.</param>
        /// <exception cref="ApiException"></exception>
        protected virtual void ThrowApiException(string trackingId, string baseUrl, string path, IRestResponse response, Exception exception = null, HttpStatusCode? statusCode = null)
        {
            var responseStatusCode = statusCode ?? response?.StatusCode;
            var responseException = exception ?? response?.ErrorException;
            var message = string.Format(ApiError, $"{baseUrl}/{path}", responseStatusCode, response?.Content, trackingId);

            throw new ApiException(trackingId, message, responseException, responseStatusCode);
        }

        /// <summary>
        /// Logs the API request.
        /// </summary>
        /// <param name="trackingId">The tracking identifier.</param>
        /// <param name="source">The source.</param>
        /// <param name="resource">The resource.</param>
        /// <param name="request">The request.</param>
        /// <param name="response">The response.</param>
        /// <param name="duration">The duration.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="statusCode">The status code.</param>
        /// <param name="includeRequestHeaders">if set to <c>true</c> [include request headers].</param>
        /// <param name="includeResponseHeaders">if set to <c>true</c> [include response headers].</param>
        protected virtual void LogApiRequestWithOtherInfo(string trackingId, string source, string resource, IRestRequest request, IRestResponse response, double? duration, Exception exception = null, HttpStatusCode? statusCode = null, bool? includeRequestHeaders = false, bool? includeResponseHeaders = false, string otherInfo = null)
        {
            if (this.LoggingService == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(otherInfo))
            {
                this.LogApiRequest(trackingId, source, resource, request, response, duration, exception, statusCode, includeRequestHeaders, includeResponseHeaders);
            }
            else
            {
                var trackingInfo = this.GetApiTrackingInfo(trackingId, source, resource, request, response, duration, exception, statusCode, includeRequestHeaders, includeResponseHeaders);
                trackingInfo.OtherInfo = otherInfo;
                this.LoggingService.LogRequest(trackingInfo);
            }


        }

        /// <summary>
        /// Logs the API request.
        /// </summary>
        /// <param name="trackingId">The tracking identifier.</param>
        /// <param name="source">The source.</param>
        /// <param name="resource">The resource.</param>
        /// <param name="request">The request.</param>
        /// <param name="response">The response.</param>
        /// <param name="duration">The duration.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="statusCode">The status code.</param>
        /// <param name="includeRequestHeaders">if set to <c>true</c> [include request headers].</param>
        /// <param name="includeResponseHeaders">if set to <c>true</c> [include response headers].</param>
        protected virtual void LogApiRequest(string trackingId, string source, string resource, IRestRequest request, IRestResponse response, double? duration,
                                                Exception exception = null, HttpStatusCode? statusCode = null, bool? includeRequestHeaders = false, bool? includeResponseHeaders = false)
        {
            if (this.LoggingService == null)
            {
                return;
            }

            var trackingInfo = this.GetApiTrackingInfo(trackingId, source, resource, request, response, duration, exception, statusCode, includeRequestHeaders, includeResponseHeaders);
            this.LoggingService.LogRequest(trackingInfo);
        }

        /// <summary>
        /// Gets the API tracking information.
        /// </summary>
        /// <param name="trackingId">The tracking identifier.</param>
        /// <param name="source">The source.</param>
        /// <param name="resource">The resource.</param>
        /// <param name="request">The request.</param>
        /// <param name="response">The response.</param>
        /// <param name="duration">The duration.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="statusCode">The status code.</param>
        /// <param name="includeRequestHeaders">The include request headers.</param>
        /// <param name="includeResponseHeaders">The include response headers.</param>
        /// <returns>ApiTrackingInfo.</returns>
        protected virtual ApiTrackingInfo GetApiTrackingInfo(string trackingId, string source, string resource, IRestRequest request, IRestResponse response, double? duration,
                                                                Exception exception = null, HttpStatusCode? statusCode = null, bool? includeRequestHeaders = false, bool? includeResponseHeaders = false)
        {
            if (this.LoggingService == null)
            {
                return null;
            }

            var trackingInfo = new ApiTrackingInfo
            {
                Duration = duration,
                Method = request?.Method.ToString(),
                Resource = resource,
                Source = source,
                TrackingId = trackingId ?? Guid.NewGuid().ToString(),
                Exception = (exception ?? response?.ErrorException).ToErrorString()
            };

            if (request?.Parameters != null && request.Parameters.Count > 0)
            {
                var requestParameters = new List<ApiTrackingParameter>();

                var getOrPostParameters = request.Parameters.FindAll(p => p.Type == ParameterType.GetOrPost);
                if (getOrPostParameters != null && getOrPostParameters.Count > 0)
                {
                    var list = new List<string>();
                    foreach (var parameter in getOrPostParameters)
                    {
                        list.Add($"{parameter.Name}=\"{DataMaskingManager.MaskJsonData(parameter.Name, parameter.Value?.ToString())}\"");
                    }

                    requestParameters.Add(new ApiTrackingParameter
                    {
                        Type = ParameterType.GetOrPost.ToString(),
                        Value = string.Join("&", list)
                    });
                }

                var bodyParameters = request.Parameters.FindAll(p => p.Type == ParameterType.RequestBody);
                if (bodyParameters != null && bodyParameters.Count > 0)
                {
                    foreach (var parameter in bodyParameters)
                    {
                        var value = DataMaskingManager.MaskJsonDataObject(parameter.Value);
                        requestParameters.Add(new ApiTrackingParameter
                        {
                            Type = ParameterType.RequestBody.ToString(),
                            Value = value
                        });
                    }
                }

                trackingInfo.RequestParameters = requestParameters.Count == 0 ? null : Regex.Unescape(JsonConvert.SerializeObject(requestParameters));

                if (includeRequestHeaders.HasValue && includeRequestHeaders.Value)
                {
                    string requestHeader = null;
                    var requestHeaders = request.Parameters.FindAll(p => p.Type == ParameterType.HttpHeader);
                    if (requestHeaders != null && requestHeaders.Count > 0)
                    {
                        var list = new List<string>();
                        foreach (var parameter in requestHeaders)
                        {
                            list.Add($"{parameter.Name}=\"{DataMaskingManager.MaskJsonData(parameter.Name, parameter.Value?.ToString())}\"");
                        }

                        requestHeader = string.Join("&", list);
                    }

                    trackingInfo.RequestHeaders = requestHeader;
                }
            }

            if (response != null)
            {
                trackingInfo.ErrorMessage = response.ErrorMessage;
                trackingInfo.ResponseStatusCode = statusCode ?? response.StatusCode;
                trackingInfo.ResponseContent = DataMaskingManager.MaskJsonString(response.Content);

                if (includeResponseHeaders.HasValue && includeResponseHeaders.Value && response.Headers != null && response.Headers.Count > 0)
                {
                    var list = new List<string>();
                    foreach (var item in response.Headers)
                    {
                        list.Add($"{item.Name}=\"{DataMaskingManager.MaskJsonData(item.Name, item.Value?.ToString())}\"");
                    }

                    trackingInfo.ResponseHeaders = string.Join("&", list);
                }

            }

            return trackingInfo;
        }

        /// <summary>
        /// Gets the API tracking information.
        /// </summary>
        /// <param name="trackingId">The tracking identifier.</param>
        /// <param name="apiSetting">The API setting.</param>
        /// <param name="resource">The resource.</param>
        /// <param name="method">The method.</param>
        /// <param name="body">The body.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="headers">The headers.</param>
        /// <param name="content">The content.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="duration">The duration.</param>
        /// <returns>ApiTrackingInfo.</returns>
        protected virtual ApiTrackingInfo GetApiTrackingInfo(string trackingId, ApiSetting apiSetting, string resource, Method method, object body = null,
                                                                IDictionary<string, string> parameters = null, IDictionary<string, string> headers = null, string content = null, Exception exception = null, double? duration = null)
        {
            if (this.LoggingService == null)
            {
                return null;
            }

            var trackingInfo = new ApiTrackingInfo
            {
                TrackingId = trackingId ?? Guid.NewGuid().ToString(),
                Source = apiSetting.Source,
                Resource = resource,
                Method = method.ToString(),
                ResponseStatusCode = exception == null ? HttpStatusCode.OK : HttpStatusCode.InternalServerError,
                ResponseHeaders = null,
                ResponseContent = DataMaskingManager.MaskJsonString(content),
                Exception = exception.ToErrorString(),
                Duration = duration
            };

            var requestParameters = new List<ApiTrackingParameter>();
            if (parameters != null && parameters.Count > 0)
            {
                var list = new List<string>();
                foreach (var parameter in parameters)
                {
                    list.Add($"{parameter.Key}=\"{DataMaskingManager.MaskJsonData(parameter.Key, parameter.Value?.ToString())}\"");
                }

                requestParameters.Add(new ApiTrackingParameter
                {
                    Type = ParameterType.GetOrPost.ToString(),
                    Value = string.Join("&", list)
                });
            }

            if (body != null)
            {
                var value = DataMaskingManager.MaskJsonDataObject(body);
                requestParameters.Add(new ApiTrackingParameter
                {
                    Type = ParameterType.RequestBody.ToString(),
                    Value = value
                });
            }

            trackingInfo.RequestParameters = requestParameters.Count == 0 ? null : Regex.Unescape(JsonConvert.SerializeObject(requestParameters));

            var includeRequestHeaders = apiSetting.IncludeRequestHeadersInTrackingLog;
            if (includeRequestHeaders.HasValue && includeRequestHeaders.Value
                                               && headers != null && headers.Count > 0)
            {
                string requestHeader = null;
                var list = new List<string>();
                foreach (var header in headers)
                {
                    list.Add($"{header.Key}=\"{DataMaskingManager.MaskJsonData(header.Key, header.Value?.ToString())}\"");
                }

                requestHeader = string.Join("&", list);

                trackingInfo.RequestHeaders = requestHeader;
            }

            return trackingInfo;
        }

        /// <summary>
        /// Gets the API tracking information.
        /// </summary>
        /// <param name="apiSetting">The API setting.</param>
        /// <returns>ApiTrackingInfo.</returns>
        protected virtual string GetApiTrackingOtherInfo(ApiSetting apiSetting)
        {
            if (this.LoggingService == null)
            {
                return null;
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets the client certificates.
        /// </summary>
        /// <param name="certificateFile">The certificate file.</param>
        /// <param name="password">The certificate password.</param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException">Could not find the certificate file</exception>
        protected X509Certificate2Collection GetClientCertificates(string certificateFile, string password)
        {
            if (string.IsNullOrWhiteSpace(certificateFile))
            {
                return null;
            }

            var cacheKey = $"CertificateFileCacheKey_{Path.GetFileNameWithoutExtension(certificateFile)}";
            var x509Certificate2 = FileCachingManager.Get<X509Certificate2>(cacheKey);
            if (x509Certificate2 == null)
            {
                if (!File.Exists(certificateFile))
                {
                    certificateFile = ApiSettingsManager.GetClientCertificateFileName(certificateFile);

                    throw new FileNotFoundException($"Could not find the certificate file [{certificateFile}]", certificateFile);
                }
                
                x509Certificate2 = string.IsNullOrWhiteSpace(password) ? new X509Certificate2(certificateFile) : new X509Certificate2(certificateFile, password);
                FileCachingManager.Set(cacheKey, x509Certificate2, certificateFile);
            }

            return new X509Certificate2Collection { x509Certificate2 };
        }

        /// <summary>
        ///  This function will be trigger all default operation below: Get, GetAsync, Post, PostAsync to modify apiSetting and request option base on the logic of RestCient
        /// </summary>
        /// <param name="apiSetting"></param>
        /// <param name="requestOptions"></param>
        protected virtual void AdjustApiSettingBeforePrepareRequest(ApiSetting apiSetting, IDictionary<string, string> headers, IDictionary<string, string> parameters)
        {
            // re-write url
            if (!string.IsNullOrEmpty(apiSetting.ReWriteUrl))
            {
                string reUrl = apiSetting.ReWriteUrl;
                MatchCollection matches = Regex.Matches(reUrl, "{(.*?):(.*?)}");
                if (matches != null)
                {
                    bool hasAdjust = false;
                    foreach (Match matchItem in matches)
                    {
                        var matchValue = matchItem.Value.Substring(1, matchItem.Value.Length - 1);
                        matchValue = matchValue.Substring(0, matchValue.Length - 1);
                        string[] arrInfos = matchValue.Split(new char[] { ':' });
                        string key = arrInfos[0];
                        string paramName = arrInfos[1];

                        if (key == "header" && headers != null && headers.ContainsKey(paramName))
                        {
                            hasAdjust = true;
                            reUrl = reUrl.Replace("{" + matchValue + "}", headers[paramName]);
                            headers.Remove(paramName);
                        }
                        else if (key == "paramater" && parameters != null && parameters.ContainsKey(paramName))
                        {
                            hasAdjust = true;
                            reUrl = reUrl.Replace("{" + matchValue + "}", parameters[paramName]);
                            parameters.Remove(paramName);
                        }
                    }

                    if (hasAdjust)
                    {
                        apiSetting.Path = reUrl;
                    }
                }
            }
        }

        protected TResult GetsSettingDefaultBlock<TResult>(string key, string operatonName = "default")
        {
            ApiSetting apiSetting = GetApiSetting(operatonName);
            if (apiSetting != null)
            {
                ApiSettingSection apiSettingSection = apiSetting.GetApiSettingSection("settingDefaultBlock");
                if (apiSettingSection != null && apiSettingSection.Settings != null && apiSettingSection.Settings.ContainsKey(key))
                {
                    return (TResult)Convert.ChangeType(apiSettingSection.Settings[key].Value, typeof(TResult));
                }
            }
            return default(TResult);
        }

        public string ReplacePayload(string payload, Dictionary<string, object> replaceDatas, string operatonName = "default", DataFormat dataFormat = DataFormat.Json)
        {
            if (payload != null && replaceDatas != null && dataFormat == DataFormat.Json)
            {
                JObject body = JsonConvert.DeserializeObject<JObject>(payload);
                ApiSetting apiSetting = GetApiSetting(operatonName);
                if (apiSetting != null)
                {
                    ApiSettingSection apiSection = apiSetting.GetApiSettingSection("replacePayload");
                    if (apiSection != null)
                    {
                        foreach (var setting in apiSection.Settings)
                        {
                            object value = setting.Value.Value;
                            string dataKey = setting.Value.Value;
                            if (dataKey.StartsWith("{") && dataKey.EndsWith("}"))
                            {
                                dataKey = dataKey.Replace("{", "").Replace("}", "");
                                if (replaceDatas.ContainsKey(dataKey))
                                    value = replaceDatas[dataKey];
                                else
                                    value = dataKey;
                            }

                            if (value != null)
                            {
                                string[] pathParts = setting.Key.Split(new char[] { '.' });
                                JToken currentNode = body;
                                JToken nodeValue = JContainer.FromObject(value);

                                for (int i = 0; i < pathParts.Length; i++)
                                {
                                    string pathPath = pathParts[i];
                                    bool isLast = i == pathParts.Length - 1;
                                    var partNode = currentNode.SelectToken(pathPath);
                                    //remove node has value is null
                                    if (partNode != null && !partNode.HasValues && partNode.Type == JTokenType.Null)
                                    {
                                        partNode.Parent.Remove();
                                        partNode = null;
                                    }


                                    if (partNode is null)
                                    {
                                        var nodeToAdd = isLast ? nodeValue : new JObject();
                                        ((JObject)currentNode).Add(pathPath, nodeToAdd);
                                        currentNode = currentNode.SelectToken(pathPath);
                                    }
                                    else
                                    {
                                        currentNode = partNode;

                                        if (isLast)
                                            currentNode.Replace(nodeValue);
                                    }

                                }
                            }
                        }
                    }
                }
                return JsonConvert.SerializeObject(body);
            }
            return payload;
        }

        #endregion

    }
}