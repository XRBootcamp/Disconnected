using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

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
                  .Add(new MediaTypeWithQualityHeaderValue("application/json"));//ACCEPT header
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
    }
}