using Newtonsoft.Json;
using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class StageStateApiClient : MonoBehaviour
{
    [SerializeField] private string baseUrl = "http://localhost:3000";
    
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    public IEnumerator SaveState(string stageId, string accessToken, StageStateDto state, System.Action<string> onError = null)
    {
        string url = $"{baseUrl}/api/stages/{stageId}/state";

        string json = JsonConvert.SerializeObject(state);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        using var req = new UnityWebRequest(url, "PUT");
        req.uploadHandler = new UploadHandlerRaw(bodyRaw);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("Authorization", $"Bearer {accessToken}");

        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            string msg = $"PUT SaveState failed: {req.responseCode} {req.downloadHandler.text}";
            Debug.LogError(msg);
            onError?.Invoke(msg);
            yield break;
        }

        Debug.Log("SaveState OK: " + req.downloadHandler.text);
    }

    public IEnumerator LoadState(string stageId, string accessToken, Action<StageStateDto> onOk, Action<string> onErr)
    {
        if (string.IsNullOrEmpty(accessToken))
        {
            onErr?.Invoke("LoadState: missing access token");
            yield break;
        }

        string url = $"{baseUrl}/api/stages/{stageId}/state";

        using var req = UnityWebRequest.Get(url);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Authorization", $"Bearer {accessToken}");
        req.SetRequestHeader("Accept", "application/json");

        yield return req.SendWebRequest();

        string raw = req.downloadHandler?.text ?? "";
        Debug.Log($"[HTTP] {req.responseCode}: {raw}");

        if (req.result != UnityWebRequest.Result.Success)
        {
            onErr?.Invoke($"GET LoadState failed: {req.error} ({req.responseCode}) {raw}");
            yield break;
        }

        var state = JsonConvert.DeserializeObject<StageStateDto>(raw);
        if (state == null)
        {
            onErr?.Invoke("LoadState: invalid response");
            yield break;
        }

        onOk?.Invoke(state);
    }
}
