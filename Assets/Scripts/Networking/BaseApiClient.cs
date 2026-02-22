using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

/// <summary>
/// Base class for all API clients - provides common HTTP request functionality with error handling
/// </summary>
public abstract class BaseApiClient : MonoBehaviour
{
    [SerializeField] protected GameConfig gameConfig;

    protected string BaseUrl => gameConfig != null ? gameConfig.serverBaseUrl : "https://planet-crafters-server.onrender.com";

    protected virtual void Awake()
    {
        // Find GameConfig if not assigned
        if (gameConfig == null)
        {
            gameConfig = Resources.Load<GameConfig>("GameConfig");
            if (gameConfig == null)
            {
                Debug.LogWarning($"[{GetType().Name}] GameConfig not found in Resources. Using hardcoded URL.");
            }
        }
    }

    /// <summary>
    /// Sends a GET request
    /// </summary>
    protected IEnumerator GetRequest<T>(
        string endpoint,
        string accessToken,
        Action<T> onSuccess,
        Action<string> onError)
    {
        var url = $"{BaseUrl}{endpoint}";

        using var req = UnityWebRequest.Get(url);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Accept", "application/json");

        if (!string.IsNullOrEmpty(accessToken))
            req.SetRequestHeader("Authorization", $"Bearer {accessToken}");

        yield return req.SendWebRequest();

        var raw = req.downloadHandler?.text ?? "";

        if (req.result != UnityWebRequest.Result.Success)
        {
            var errorMsg = $"GET {endpoint} failed: {req.error} ({req.responseCode}) {raw}";
            Debug.LogError($"[{GetType().Name}] {errorMsg}");
            onError?.Invoke(errorMsg);
            yield break;
        }

        T response;
        try
        {
            response = JsonConvert.DeserializeObject<T>(raw);
        }
        catch (Exception ex)
        {
            var errorMsg = $"JSON parse error: {ex.Message}\nRaw response: {raw}";
            Debug.LogError($"[{GetType().Name}] {errorMsg}");
            onError?.Invoke(errorMsg);
            yield break;
        }

        if (response == null)
        {
            var errorMsg = $"Response deserialized to null.\nRaw response: {raw}";
            Debug.LogError($"[{GetType().Name}] {errorMsg}");
            onError?.Invoke(errorMsg);
            yield break;
        }

        Debug.Log($"[{GetType().Name}] GET {endpoint} succeeded ({req.responseCode})");
        onSuccess?.Invoke(response);
    }

    /// <summary>
    /// Sends a POST request with JSON body
    /// </summary>
    protected IEnumerator PostRequest<TRequest, TResponse>(
        string endpoint,
        TRequest body,
        string accessToken,
        Action<TResponse> onSuccess,
        Action<string> onError)
    {
        var url = $"{BaseUrl}{endpoint}";

        using var req = new UnityWebRequest(url, "POST");
        var json = JsonConvert.SerializeObject(body);
        req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");

        if (!string.IsNullOrEmpty(accessToken))
            req.SetRequestHeader("Authorization", $"Bearer {accessToken}");

        yield return req.SendWebRequest();

        var raw = req.downloadHandler?.text ?? "";

        if (req.result != UnityWebRequest.Result.Success)
        {
            var errorMsg = $"POST {endpoint} failed: {req.error} ({req.responseCode}) {raw}";
            Debug.LogError($"[{GetType().Name}] {errorMsg}");
            onError?.Invoke(errorMsg);
            yield break;
        }

        TResponse response;
        try
        {
            response = JsonConvert.DeserializeObject<TResponse>(raw);
        }
        catch (Exception ex)
        {
            var errorMsg = $"JSON parse error: {ex.Message}\nRaw response: {raw}";
            Debug.LogError($"[{GetType().Name}] {errorMsg}");
            onError?.Invoke(errorMsg);
            yield break;
        }

        Debug.Log($"[{GetType().Name}] POST {endpoint} succeeded ({req.responseCode})");
        onSuccess?.Invoke(response);
    }

    /// <summary>
    /// Sends a POST request without expecting a response body
    /// </summary>
    protected IEnumerator PostRequestNoResponse(
        string endpoint,
        string accessToken,
        Action onSuccess,
        Action<string> onError)
    {
        var url = $"{BaseUrl}{endpoint}";

        using var req = new UnityWebRequest(url, "POST");
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Accept", "application/json");

        if (!string.IsNullOrEmpty(accessToken))
            req.SetRequestHeader("Authorization", $"Bearer {accessToken}");

        yield return req.SendWebRequest();

        var raw = req.downloadHandler?.text ?? "";

        if (req.result != UnityWebRequest.Result.Success)
        {
            var errorMsg = $"POST {endpoint} failed: {req.error} ({req.responseCode}) {raw}";
            Debug.LogError($"[{GetType().Name}] {errorMsg}");
            onError?.Invoke(errorMsg);
            yield break;
        }

        Debug.Log($"[{GetType().Name}] POST {endpoint} succeeded ({req.responseCode})");
        onSuccess?.Invoke();
    }

    /// <summary>
    /// Sends a PUT request with JSON body
    /// </summary>
    protected IEnumerator PutRequest<TRequest>(
        string endpoint,
        TRequest body,
        string accessToken,
        Action onSuccess,
        Action<string> onError)
    {
        var url = $"{BaseUrl}{endpoint}";

        using var req = new UnityWebRequest(url, "PUT");
        var json = JsonConvert.SerializeObject(body);
        req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");

        if (!string.IsNullOrEmpty(accessToken))
            req.SetRequestHeader("Authorization", $"Bearer {accessToken}");

        yield return req.SendWebRequest();

        var raw = req.downloadHandler?.text ?? "";

        if (req.result != UnityWebRequest.Result.Success)
        {
            var errorMsg = $"PUT {endpoint} failed: {req.error} ({req.responseCode}) {raw}";
            Debug.LogError($"[{GetType().Name}] {errorMsg}");
            onError?.Invoke(errorMsg);
            yield break;
        }

        Debug.Log($"[{GetType().Name}] PUT {endpoint} succeeded ({req.responseCode})");
        onSuccess?.Invoke();
    }
}
