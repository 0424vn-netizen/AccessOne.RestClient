using System.Collections.Generic;
using System.Threading.Tasks;
using AS.AO.Api.RestClient.Models;
using RestSharp;

namespace AS.AO.Api.RestClient
{
    /// <summary>
    /// The Rest client that can be used to invoke Restful API Endpoints
    /// </summary>
    public interface IAORestClient
    {
        /// <summary>
        /// Gets the API setting.
        /// </summary>
        /// <param name="path">The request.</param>
        /// <returns></returns>
        ApiSetting GetApiSetting(string path);

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
        TResult Get<TResult>(ApiSetting apiSetting, IDictionary<string, string> parameters = null, IDictionary<string, string> headers = null, DataFormat requestFormat = DataFormat.Json, string trackingId = null)
            where TResult : class, new();

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
        TResult Get<TResult>(string baseUrl, string path, IDictionary<string, string> parameters = null, IDictionary<string, string> headers = null, DataFormat requestFormat = DataFormat.Json, string trackingId = null)
            where TResult : class, new();

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
        IRestResponse<TResult> GetForRestResponse<TResult>(ApiSetting apiSetting, IDictionary<string, string> parameters = null, IDictionary<string, string> headers = null, DataFormat requestFormat = DataFormat.Json, string trackingId = null)
            where TResult : class, new();

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
        IRestResponse<TResult> GetForRestResponse<TResult>(string baseUrl, string path, IDictionary<string, string> parameters = null, IDictionary<string, string> headers = null, DataFormat requestFormat = DataFormat.Json, string trackingId = null)
            where TResult : class, new();

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
        TResult Post<TResult>(ApiSetting apiSetting, IDictionary<string, string> parameters = null, IDictionary<string, string> headers = null, DataFormat requestFormat = DataFormat.Json, string trackingId = null)
            where TResult : class, new();

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
        TResult Post<TResult>(string baseUrl, string path, IDictionary<string, string> parameters = null, IDictionary<string, string> headers = null, DataFormat requestFormat = DataFormat.Json, string trackingId = null)
            where TResult : class, new();

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
        TResult Post<T, TResult>(ApiSetting apiSetting, T body, IDictionary<string, string> parameters = null, IDictionary<string, string> headers = null, DataFormat requestFormat = DataFormat.Json, string trackingId = null)
            where T : class, new()
            where TResult : class, new();

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
        TResult Post<T, TResult>(string baseUrl, string path, T body, IDictionary<string, string> parameters = null, IDictionary<string, string> headers = null, DataFormat requestFormat = DataFormat.Json, string trackingId = null)
            where T : class, new()
            where TResult : class, new();

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
        IRestResponse<TResult> PostForRestResponse<TResult>(ApiSetting apiSetting, IDictionary<string, string> parameters = null, IDictionary<string, string> headers = null, DataFormat requestFormat = DataFormat.Json, string trackingId = null)
            where TResult : class, new();

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
        IRestResponse<TResult> PostForRestResponse<TResult>(string baseUrl, string path, IDictionary<string, string> parameters = null, IDictionary<string, string> headers = null, DataFormat requestFormat = DataFormat.Json, string trackingId = null)
            where TResult : class, new();

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
        IRestResponse<TResult> PostForRestResponse<T, TResult>(ApiSetting apiSetting, T body, IDictionary<string, string> parameters = null, IDictionary<string, string> headers = null, DataFormat requestFormat = DataFormat.Json, string trackingId = null)
            where T : class, new()
            where TResult : class, new();

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
        IRestResponse<TResult> PostForRestResponse<T, TResult>(string baseUrl, string path, T body, IDictionary<string, string> parameters = null, IDictionary<string, string> headers = null, DataFormat requestFormat = DataFormat.Json, string trackingId = null)
            where T : class, new()
            where TResult : class, new();

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
        IRestResponse<TResult> PutForRestResponse<TResult>(ApiSetting apiSetting, IDictionary<string, string> parameters = null, IDictionary<string, string> headers = null, DataFormat requestFormat = DataFormat.Json, string trackingId = null)
            where TResult : class, new();

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
        IRestResponse<TResult> PutForRestResponse<TResult>(string baseUrl, string path, IDictionary<string, string> parameters = null, IDictionary<string, string> headers = null, DataFormat requestFormat = DataFormat.Json, string trackingId = null)
            where TResult : class, new();

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
        IRestResponse<TResult> PutForRestResponse<T, TResult>(ApiSetting apiSetting, T body, IDictionary<string, string> parameters = null, IDictionary<string, string> headers = null, DataFormat requestFormat = DataFormat.Json, string trackingId = null)
            where T : class, new()
            where TResult : class, new();

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
        IRestResponse<TResult> PutForRestResponse<T, TResult>(string baseUrl, string path, T body, IDictionary<string, string> parameters = null, IDictionary<string, string> headers = null, DataFormat requestFormat = DataFormat.Json, string trackingId = null)
            where T : class, new()
            where TResult : class, new();

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
        IRestResponse<TResult> DeleteForRestResponse<TResult>(ApiSetting apiSetting, IDictionary<string, string> parameters = null, IDictionary<string, string> headers = null, DataFormat requestFormat = DataFormat.Json, string trackingId = null)
            where TResult : class, new();

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
        IRestResponse<TResult> DeleteForRestResponse<TResult>(string baseUrl, string path, IDictionary<string, string> parameters = null, IDictionary<string, string> headers = null, DataFormat requestFormat = DataFormat.Json, string trackingId = null)
            where TResult : class, new();

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
        IRestResponse<TResult> DeleteForRestResponse<T, TResult>(ApiSetting apiSetting, T body, IDictionary<string, string> parameters = null, IDictionary<string, string> headers = null, DataFormat requestFormat = DataFormat.Json, string trackingId = null)
            where T : class, new()
            where TResult : class, new();

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
        IRestResponse<TResult> DeleteForRestResponse<T, TResult>(string baseUrl, string path, T body, IDictionary<string, string> parameters = null, IDictionary<string, string> headers = null, DataFormat requestFormat = DataFormat.Json, string trackingId = null)
            where T : class, new()
            where TResult : class, new();

        /// <summary>
        /// Consumes a Restful API as Form Data.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="apiSetting">The API setting.</param>
        /// <param name="parameters">The request query parameters.</param>
        /// <param name="headers">The request headers.</param>
        /// <param name="trackingId">The tracking identifier.</param>
        /// <returns></returns>
        TResult PostAsForm<TResult>(ApiSetting apiSetting, IDictionary<string, string> parameters = null, IDictionary<string, string> headers = null, string trackingId = null)
            where TResult : class, new();

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
        TResult PostAsForm<TResult>(string baseUrl, string path, IDictionary<string, string> parameters = null, IDictionary<string, string> headers = null, string trackingId = null)
            where TResult : class, new();

        /// <summary>
        /// Consumes a Restful API as Form Data.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="apiSetting">The API setting.</param>
        /// <param name="parameters">The request query parameters.</param>
        /// <param name="headers">The request headers.</param>
        /// <param name="trackingId">The tracking identifier.</param>
        /// <returns></returns>
        IRestResponse<TResult> PostAsFormForRestResponse<TResult>(ApiSetting apiSetting, IDictionary<string, string> parameters = null, IDictionary<string, string> headers = null, string trackingId = null)
            where TResult : class, new();

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
        IRestResponse<TResult> PostAsFormForRestResponse<TResult>(string baseUrl, string path, IDictionary<string, string> parameters = null, IDictionary<string, string> headers = null, string trackingId = null)
            where TResult : class, new();

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
        Task<TResult> GetAsync<TResult>(ApiSetting apiSetting, IDictionary<string, string> parameters = null, IDictionary<string, string> headers = null, DataFormat requestFormat = DataFormat.Json, string trackingId = null)
            where TResult : class, new();

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
        Task<TResult> GetAsync<TResult>(string baseUrl, string path, IDictionary<string, string> parameters = null, IDictionary<string, string> headers = null, DataFormat requestFormat = DataFormat.Json, string trackingId = null)
            where TResult : class, new();

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
        Task<TResult> PostAsync<TResult>(ApiSetting apiSetting, IDictionary<string, string> parameters = null, IDictionary<string, string> headers = null, DataFormat requestFormat = DataFormat.Json, string trackingId = null)
            where TResult : class, new();

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
        Task<TResult> PostAsync<TResult>(string baseUrl, string path, IDictionary<string, string> parameters = null, IDictionary<string, string> headers = null, DataFormat requestFormat = DataFormat.Json, string trackingId = null)
            where TResult : class, new();

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
        Task<TResult> PostAsync<T, TResult>(ApiSetting apiSetting, T body, IDictionary<string, string> parameters = null, IDictionary<string, string> headers = null, DataFormat requestFormat = DataFormat.Json, string trackingId = null)
            where T : class, new()
            where TResult : class, new();

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
        Task<TResult> PostAsync<T, TResult>(string baseUrl, string path, T body, IDictionary<string, string> parameters = null, IDictionary<string, string> headers = null, DataFormat requestFormat = DataFormat.Json, string trackingId = null)
            where T : class, new()
            where TResult : class, new();

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
        Task<TResult> PostAsFormAsync<TResult>(ApiSetting apiSetting, IDictionary<string, string> parameters = null, IDictionary<string, string> headers = null, string trackingId = null)
            where TResult : class, new();

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
        Task<TResult> PostAsFormAsync<TResult>(string baseUrl, string path, IDictionary<string, string> parameters = null, IDictionary<string, string> headers = null, string trackingId = null)
            where TResult : class, new();

        /// <summary>
        /// Consumes a Restful API as Form Data the asynchronous.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="apiSetting">The API setting.</param>
        /// <param name="parameters">The request query parameters.</param>
        /// <param name="headers">The request headers.</param>
        /// <param name="trackingId">The tracking identifier.</param>
        /// <returns></returns>
        Task<IRestResponse<TResult>> PostAsFormForRestResponseAsync<TResult>(ApiSetting apiSetting, IDictionary<string, string> parameters = null, IDictionary<string, string> headers = null, string trackingId = null)
            where TResult : class, new();

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
        Task<IRestResponse<TResult>> PostAsFormForRestResponseAsync<TResult>(string baseUrl, string path, IDictionary<string, string> parameters = null, IDictionary<string, string> headers = null, string trackingId = null) 
            where TResult : class, new();
    }
}