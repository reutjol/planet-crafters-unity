using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;


public class AuthApiClient : MonoBehaviour
{
    public string baseUrl = "http://localhost:3000";

    [System.Serializable]
    private class LoginBody { public string email; public string password; }

    [System.Serializable]
    private class RegisterBody { public string name; public string email; public string userName; public string password; }

    [System.Serializable]
    private class RefreshBody { public string refreshToken; }

    public IEnumerator Login(string email, string password,
        System.Action<AuthResponseDto> onOk,
        System.Action<string> onErr)
    {
        var url = $"{baseUrl}/api/auth";
        var body = new LoginBody { email = email, password = password };
        yield return PostJson(url, body, onOk, onErr);
    }

    public IEnumerator Register(string name, string email, string userName, string password,
        System.Action<AuthResponseDto> onOk,
        System.Action<string> onErr)
    {
        var url = $"{baseUrl}/api/users";
        var body = new RegisterBody { name = name, email = email, userName = userName, password = password };
        yield return PostJson(url, body, onOk, onErr);
    }

    public IEnumerator Refresh(string refreshToken,
        System.Action<string> onOkAccessToken,
        System.Action<string> onErr)
    {
        var url = $"{baseUrl}/api/auth/refresh";
        var body = new RefreshBody { refreshToken = refreshToken };

        using var req = new UnityWebRequest(url, "POST");
        var json = JsonConvert.SerializeObject(body);
        req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");

        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            onErr?.Invoke($"Refresh failed: {req.error} ({req.responseCode}) {req.downloadHandler.text}");
            yield break;
        }

        // response: { accessToken: "..." }
        var wrapper = JsonConvert.DeserializeObject<AccessTokenWrapper>(req.downloadHandler.text);
        if (wrapper == null || string.IsNullOrEmpty(wrapper.accessToken))
        {
            onErr?.Invoke("Refresh failed: missing accessToken");
            yield break;
        }

        onOkAccessToken?.Invoke(wrapper.accessToken);
    }

    [System.Serializable]
    private class AccessTokenWrapper { public string accessToken; }

    private IEnumerator PostJson(string url, object body,
     System.Action<AuthResponseDto> onOk,
     System.Action<string> onErr)
    {
        using var req = new UnityWebRequest(url, "POST");
        var json = JsonConvert.SerializeObject(body);   // ✅ Newtonsoft
        req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");

        yield return req.SendWebRequest();

        var raw = req.downloadHandler?.text ?? "";
        Debug.Log($"[HTTP] {req.responseCode}: {raw}");

        if (req.result != UnityWebRequest.Result.Success)
        {
            onErr?.Invoke($"POST failed: {req.error} ({req.responseCode}) {raw}");
            yield break;
        }

        var resp = JsonConvert.DeserializeObject<AuthResponseDto>(raw); // ✅ Newtonsoft
        if (resp == null || string.IsNullOrEmpty(resp.accessToken))
        {
            onErr?.Invoke("Auth failed: missing accessToken");
            yield break;
        }

        onOk?.Invoke(resp);
    }

}
