using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using UnityEngine;

namespace Runware
{

    public class RunwareApiClient : IApiClient, IDisposable
    {
        public readonly HttpClient _httpClient;
        private readonly string apiKey;
        public const string BaseUrlConst = "https://api.runware.ai/v1";
        public string BaseUrl => BaseUrlConst;

        /// <summary>
        /// The HttpClient instance used for API requests.
        /// </summary>
        public HttpClient httpClient => _httpClient;

        public RunwareApiClient(string apiKey)
        {
            this.apiKey = apiKey;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            _httpClient.DefaultRequestHeaders
                  .Accept
                  .Add(new MediaTypeWithQualityHeaderValue("application/json"));
                  //ACCEPT header
            _httpClient.BaseAddress = new Uri(BaseUrlConst);
        }

        public Dictionary<string, string> ToCustomHeader()
        {
            var headers = new Dictionary<string, string>();
            foreach (var header in httpClient.DefaultRequestHeaders)
            {
                headers[header.Key] = string.Join(";", header.Value);
            }
            headers.Add("Content-type", "application/json; charset=UTF-8");
            return headers;
        }

        public void Dispose()
        {
            _httpClient.Dispose();
            GC.SuppressFinalize(this);
        }

        public async Task<TextToImageResponseDataArrayModel> CreateTextToImageAsync(List<TextToImageRequestModel> t2iRequests)
        {
            return await PostAsync<List<TextToImageRequestModel>, TextToImageResponseDataArrayModel>("", t2iRequests, nameof(CreateTextToImageAsync));
        }


        #region GENERIC_API_CALLS

        /// <summary>
        /// Generic POST request method for Runware API calls.
        /// </summary>
        /// <typeparam name="TRequest">Type of the request object.</typeparam>
        /// <typeparam name="TResponse">Type of the response object.</typeparam>
        /// <param name="endpoint">API endpoint (relative to BaseUrl).</param>
        /// <param name="request">Request object to serialize.</param>
        /// <param name="logPrefix">Optional prefix for logging messages.</param>
        /// <returns>Deserialized response object.</returns>
        private async Task<TResponse> PostAsync<TRequest, TResponse>(string endpoint, TRequest request, string logPrefix = null)
        {
            // NOTE: (1) JsonSerializer.Serialize either requires: all values {get ; set; }, or the attribute [JsonInclude] in every field that is to be included in the Serialize
            // NOTE: (2) Runware does not approve null values in their API, hence the DefaultIgnoreCondition below
            var requestJson = JsonSerializer.Serialize(request, new JsonSerializerOptions
            {
                IncludeFields = true,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            });
            
            string prefix = string.IsNullOrEmpty(logPrefix) ? nameof(RunwareApiClient) : logPrefix;
            UnityEngine.Debug.Log($"[{prefix}] - PostAsync Request\n{requestJson}");
            
            // NOTE: (1) _httpClient.PostAsJsonAsync does not work because Runware API requires the header "Content-type", "application/json; charset=UTF-8"
            // NOTE:    and in httpClient we cannot pass it directly (only with the StringContent below - it generates that header)
            // NOTE: (2) After transforming the request in StringContent - the json has been formed as string - so PostAsync works 
            var content = new StringContent(requestJson, Encoding.UTF8, "application/json");
            var fullUrl = BaseUrl.TrimEnd('/');
            fullUrl += string.IsNullOrEmpty(endpoint) ? "" : "/" + endpoint.TrimStart('/');
            var response = await _httpClient.PostAsync(fullUrl, content);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                UnityEngine.Debug.LogError($"[{prefix}] - PostAsync ERROR: API request failed with status code {response.StatusCode}. Response content: {errorContent}");
                // NOTE: make sure the exception retrieves the full string content
                throw new HttpRequestException(errorContent);
            }

            return await response.Content.ReadFromJsonAsync<TResponse>();
        }

        /// <summary>
        /// Generic GET request method for Runware API calls.
        /// </summary>
        /// <typeparam name="TResponse">Type of the response object.</typeparam>
        /// <param name="endpoint">API endpoint (relative to BaseUrl).</param>
        /// <param name="logPrefix">Optional prefix for logging messages.</param>
        /// <returns>Deserialized response object.</returns>
        private async Task<TResponse> GetAsync<TResponse>(string endpoint, string logPrefix = null)
        {
            string prefix = string.IsNullOrEmpty(logPrefix) ? nameof(RunwareApiClient) : logPrefix;
            var fullUrl = BaseUrl.TrimEnd('/');
            fullUrl += string.IsNullOrEmpty(endpoint) ? "" : "/" + endpoint.TrimStart('/');
            var response = await _httpClient.GetAsync(fullUrl);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                UnityEngine.Debug.LogError($"[{prefix}] - GetAsync ERROR: API request failed with status code {response.StatusCode}. Response content: {errorContent}");
                throw new HttpRequestException(errorContent);
            }

            return await response.Content.ReadFromJsonAsync<TResponse>();
        }

        /// <summary>
        /// Generic PUT request method for Runware API calls.
        /// </summary>
        /// <typeparam name="TRequest">Type of the request object.</typeparam>
        /// <typeparam name="TResponse">Type of the response object.</typeparam>
        /// <param name="endpoint">API endpoint (relative to BaseUrl).</param>
        /// <param name="request">Request object to serialize.</param>
        /// <param name="logPrefix">Optional prefix for logging messages.</param>
        /// <returns>Deserialized response object.</returns>
        private async Task<TResponse> PutAsync<TRequest, TResponse>(string endpoint, TRequest request, string logPrefix = null)
        {
            // NOTE: (1) JsonSerializer.Serialize either requires: all values {get ; set; }, or the attribute [JsonInclude] in every field that is to be included in the Serialize
            // NOTE: (2) Runware does not approve null values in their API, hence the DefaultIgnoreCondition below
            var requestJson = JsonSerializer.Serialize(request, new JsonSerializerOptions
            {
                IncludeFields = true,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            });
            
            string prefix = string.IsNullOrEmpty(logPrefix) ? nameof(RunwareApiClient) : logPrefix;
            UnityEngine.Debug.Log($"[{prefix}] - Request\n{requestJson}");
            
            var content = new StringContent(requestJson, Encoding.UTF8, "application/json");
            var fullUrl = BaseUrl.TrimEnd('/');
            fullUrl += string.IsNullOrEmpty(endpoint) ? "" : "/" + endpoint.TrimStart('/');
            var response = await _httpClient.PutAsync(fullUrl, content);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                UnityEngine.Debug.LogError($"[{prefix}] - PutAsync ERROR: API request failed with status code {response.StatusCode}. Response content: {errorContent}");
                throw new HttpRequestException(errorContent);
            }

            return await response.Content.ReadFromJsonAsync<TResponse>();
        }

        /// <summary>
        /// Generic DELETE request method for Runware API calls.
        /// </summary>
        /// <typeparam name="TResponse">Type of the response object.</typeparam>
        /// <param name="endpoint">API endpoint (relative to BaseUrl).</param>
        /// <param name="logPrefix">Optional prefix for logging messages.</param>
        /// <returns>Deserialized response object.</returns>
        private async Task<TResponse> DeleteAsync<TResponse>(string endpoint, string logPrefix = null)
        {
            string prefix = string.IsNullOrEmpty(logPrefix) ? nameof(RunwareApiClient) : logPrefix;
            var fullUrl = BaseUrl.TrimEnd('/');
            fullUrl += string.IsNullOrEmpty(endpoint) ? "" : "/" + endpoint.TrimStart('/');
            var response = await _httpClient.DeleteAsync(fullUrl);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                UnityEngine.Debug.LogError($"[{prefix}] - DeleteAsync ERROR: API request failed with status code {response.StatusCode}. Response content: {errorContent}");
                throw new HttpRequestException(errorContent);
            }

            return await response.Content.ReadFromJsonAsync<TResponse>();
        }

        #endregion

    }
}