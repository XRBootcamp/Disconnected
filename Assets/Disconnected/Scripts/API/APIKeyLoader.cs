using UnityEngine;
using System.Net.Http;
using GroqApiLibrary;
using Runware;

public class APIKeyLoader : MonoBehaviour
{
    public static APIKeyLoader Instance { get; private set; }
    public static APIKeyConfig Config { get; private set; }

    public HttpClient GroqHttpClient { get; private set; }
    public GroqApiClient GroqApi { get; private set; }
    public RunwareApiClient RunwareApi { get; private set; }

    public SF3DAPIClient SF3DApi {get; private set; }

    private static bool _initialized = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (_initialized)
            return;

        Config = Resources.Load<APIKeyConfig>("APIKeys");
        if (Config == null)
        {
            Debug.LogError("Missing APIKeys.asset in Resources folder.");
            return;
        }

        // Initialize RunwareApiClient
        if (!string.IsNullOrEmpty(Config.runwareKey))
        {
            RunwareApi = new RunwareApiClient(Config.runwareKey);
        }
        else
        {
            Debug.LogError("Missing or empty Runware API key in APIKeys.asset.");
        }

        GroqHttpClient = new HttpClient();
        GroqHttpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Config.groqKey);
        GroqApi = new GroqApiClient(Config.groqKey, GroqHttpClient);
        SF3DApi = new SF3DAPIClient(Config.stableFastKey);

        _initialized = true;
    }

    /// <summary>
    /// Clears all headers from the GroqApiClient's HttpClient while preserving the authorization header.
    /// </summary>
    public void ClearGroqHeadersPreserveAuth()
    {
        if (GroqHttpClient == null) return;

        var authHeader = GroqHttpClient.DefaultRequestHeaders.Authorization;
        GroqHttpClient.DefaultRequestHeaders.Clear();
        if (authHeader != null)
        {
            GroqHttpClient.DefaultRequestHeaders.Authorization = authHeader;
        }
    }

    /// <summary>
    /// Clears all headers from the RunwareApiClient's HttpClient while preserving the authorization header.
    /// </summary>
    public void ClearRunwareHeadersPreserveAuth()
    {
        if (RunwareApi == null) return;

        var authHeader = RunwareApi.httpClient.DefaultRequestHeaders.Authorization;
        RunwareApi.httpClient.DefaultRequestHeaders.Clear();
        if (authHeader != null)
        {
            RunwareApi.httpClient.DefaultRequestHeaders.Authorization = authHeader;
        }
    }

    protected virtual void OnDestroy()
    {
        // Dispose RunwareApiClient if needed
        if (RunwareApi != null)
        {
            RunwareApi.Dispose();
        }
    }
}
