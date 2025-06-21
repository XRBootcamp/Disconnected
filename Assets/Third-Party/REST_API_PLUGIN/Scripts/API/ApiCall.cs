using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace REST_API_HANDLER {


    public class ApiCall : MonoBehaviour
    {
        public static ApiCall instance;
        private HttpClient client = new HttpClient();

        public static string CAN_NOT_DECODE_JSON = "CAN_NOT_DECODE_JSON";

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
        }



        public void GetRequest<RESULT>(string url, Dictionary<string, string> headers, Action<RESULT> success, Action<string> error, string jsonPrefix = null)
        {
            client.GetAsync(url, jsonPrefix, headers, (result) =>
            {
                if (result.success)
                {
                    try
                    {
                        RESULT entity = JsonUtility.FromJson<RESULT>(result.resultText);
                        success.Invoke(entity);
                    }
                    catch (Exception e)
                    {
                        error.Invoke(e.Message);
                    }
                }
                else
                {
                    error.Invoke(result.error);
                }
            });
        }

        public void PostRequest<RESULT>(string url, Dictionary<string, string> headers, WWWForm form, string jsonParams, Action<RESULT> success, Action<string> error, string jsonPrefix = null)
        {
            client.PostAsync(url, jsonPrefix, jsonParams, headers, form, (result) =>

            {
                if (result.success)
                {
                    try
                    {
                        RESULT entity = JsonUtility.FromJson<RESULT>(result.resultText);
                        success.Invoke(entity);
                    }
                    catch (Exception e)
                    {
                        error.Invoke(e.Message);
                    }
                }
                else
                {
                    error.Invoke(result.error);
                }
            });
        }

        public void PutRequest<RESULT>(string url, Dictionary<string, string> headers, WWWForm form, string jsonParams, Action<RESULT> success, Action<string> error, string jsonPrefix = null)
        {

            client.PutAsync(url, jsonPrefix, jsonParams, headers, form, (result) =>
            {
                if (result.success)
                {
                    try
                    {
                        RESULT entity = JsonUtility.FromJson<RESULT>(result.resultText);
                        success.Invoke(entity);
                    }
                    catch (Exception e)
                    {
                        error.Invoke(e.Message);
                    }
                }
                else
                {
                    error.Invoke(result.error);
                }
            });
        }

        public void DeleteRequest<RESULT>(string url, Dictionary<string, string> headers, Action<RESULT> success, Action<string> error, string jsonPrefix = null)
        {

            client.DeleteAsync(url, jsonPrefix, headers, (result) =>
            {
                if (result.success)
                {
                    try
                    {
                        RESULT entity = JsonUtility.FromJson<RESULT>(result.resultText);
                        success.Invoke(entity);
                    }
                    catch (Exception e)
                    {
                        error.Invoke(CAN_NOT_DECODE_JSON);
                    }
                }
                else
                {
                    error.Invoke(result.error);
                }
            });
        }

        /// <summary>
        /// Posts a request using an IApiClient, automatically extracting headers and base URL.
        /// </summary>
        public void PostRequest<RESULT>(IApiClient apiClient, string endpoint, WWWForm form, string jsonParams, Action<RESULT> success, Action<string> error, string jsonPrefix = null)
        {
            string fullUrl = apiClient.BaseUrl.TrimEnd('/') + "/" + endpoint.TrimStart('/');
            var headers = apiClient.ToCustomHeader();
            PostRequest(fullUrl, headers, form, jsonParams, success, error, jsonPrefix);
        }

        /// <summary>
        /// Gets a request using an IApiClient, automatically extracting headers and base URL.
        /// </summary>
        public void GetRequest<RESULT>(IApiClient apiClient, string endpoint, Action<RESULT> success, Action<string> error, string jsonPrefix = null)
        {
            string fullUrl = apiClient.BaseUrl.TrimEnd('/') + "/" + endpoint.TrimStart('/');
            var headers = apiClient.ToCustomHeader();
            GetRequest(fullUrl, headers, success, error, jsonPrefix);
        }

        /// <summary>
        /// Puts a request using an IApiClient, automatically extracting headers and base URL.
        /// </summary>
        public void PutRequest<RESULT>(IApiClient apiClient, string endpoint, WWWForm form, string jsonParams, Action<RESULT> success, Action<string> error, string jsonPrefix = null)
        {
            string fullUrl = apiClient.BaseUrl.TrimEnd('/') + "/" + endpoint.TrimStart('/');
            var headers = apiClient.ToCustomHeader();
            PutRequest(fullUrl, headers, form, jsonParams, success, error, jsonPrefix);
        }

        /// <summary>
        /// Deletes a request using an IApiClient, automatically extracting headers and base URL.
        /// </summary>
        public void DeleteRequest<RESULT>(IApiClient apiClient, string endpoint, Action<RESULT> success, Action<string> error, string jsonPrefix = null)
        {
            string fullUrl = apiClient.BaseUrl.TrimEnd('/') + "/" + endpoint.TrimStart('/');
            var headers = apiClient.ToCustomHeader();
            DeleteRequest(fullUrl, headers, success, error, jsonPrefix);
        }
    }

}


