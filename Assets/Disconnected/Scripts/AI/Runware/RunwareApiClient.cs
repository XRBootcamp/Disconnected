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
    public class RunwareApiClient : IDisposable
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "https://api.runware.ai/v1";
        // TODO: add them here
        private const string Text2ImageModels = "";

        public RunwareApiClient(string apiKey)
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            _httpClient.DefaultRequestHeaders
                  .Accept
                  .Add(new MediaTypeWithQualityHeaderValue("application/json"));//ACCEPT header
            _httpClient.BaseAddress = new Uri("http://example.com/");
            
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(BaseUrl);
        }

        public void Dispose()
        {
            _httpClient.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}