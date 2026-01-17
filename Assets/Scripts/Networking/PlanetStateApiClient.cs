using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.Text;

public class PlanetStateApiClient : MonoBehaviour
{
    [SerializeField] private string baseUrl = "https://planet-crafters-server.onrender.com";

    // =========================
    // GET stage state
    // =========================
    public IEnumerator GetPlanetStageState(
        string planetId,
        string stageId,
        int deckSize,
        string accessToken,
        Action<PlanetStageStateDto> onOk,
        Action<string> onErr)
    {
        var url = $"{baseUrl}/api/planet-state/{planetId}/{stageId}?deckSize={deckSize}";
        using var req = UnityWebRequest.Get(url);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Accept", "application/json");

        if (!string.IsNullOrEmpty(accessToken))
            req.SetRequestHeader("Authorization", $"Bearer {accessToken}");

        yield return req.SendWebRequest();

        var raw = req.downloadHandler?.text ?? "";
        Debug.Log($"[PlanetStateApi] GET {req.responseCode}: {raw}");

        if (req.result != UnityWebRequest.Result.Success)
        {
            onErr?.Invoke($"GET planet-state failed: {req.error} ({req.responseCode}) {raw}");
            yield break;
        }

        PlanetStageStateDto dto;
        try
        {
            dto = JsonConvert.DeserializeObject<PlanetStageStateDto>(raw);
        }
        catch (Exception e)
        {
            onErr?.Invoke("JSON parse error: " + e.Message);
            yield break;
        }

        onOk?.Invoke(dto);
    }

    // =========================
    // SAVE stage state
    // =========================
    public IEnumerator SavePlanetStageState(
        string planetId,
        string stageId,
        string accessToken,
        SaveStageStateRequestDto dto,
        Action onOk,
        Action<string> onErr)
    {
        string url = $"{baseUrl}/api/planet-state/{planetId}/{stageId}";

        Debug.Log("[SAVE] URL: " + url);

        string json = JsonConvert.SerializeObject(dto, Formatting.Indented);
        Debug.Log("[SAVE] JSON payload:\n" + json);

        var req = new UnityWebRequest(url, "PUT"); // או POST אם השרת מצפה
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        req.uploadHandler = new UploadHandlerRaw(bodyRaw);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");

        if (!string.IsNullOrEmpty(accessToken))
            req.SetRequestHeader("Authorization", "Bearer " + accessToken);

        yield return req.SendWebRequest();

        Debug.Log("[SAVE] Response code: " + req.responseCode);
        Debug.Log("[SAVE] Response text: " + req.downloadHandler.text);

        if (req.result == UnityWebRequest.Result.Success)
            onOk?.Invoke();
        else
            onErr?.Invoke($"Save failed ({req.responseCode}) {req.error}\n{req.downloadHandler.text}");
    }

    public IEnumerator ResetStage(
    string planetId,
    string stageId,
    string accessToken,
    Action onOk,
    Action<string> onErr)
    {
        string url = $"{baseUrl}/api/planet-state/{planetId}/{stageId}/reset";
        Debug.Log("[RESET] URL: " + url);

        var req = new UnityWebRequest(url, "POST");
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Accept", "application/json");

        if (!string.IsNullOrEmpty(accessToken))
            req.SetRequestHeader("Authorization", "Bearer " + accessToken);

        Debug.Log("[RESET] Sending request...");
        yield return req.SendWebRequest();

        var raw = req.downloadHandler?.text ?? "";
        Debug.Log($"[RESET] Response {req.responseCode}: {raw}");

        if (req.result == UnityWebRequest.Result.Success)
            onOk?.Invoke();
        else
            onErr?.Invoke($"Reset failed ({req.responseCode}) {req.error} {raw}");
    }
}


