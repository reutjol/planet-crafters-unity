using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

public class PlanetApiClient : MonoBehaviour
{
    [SerializeField] private string baseUrl = "http://localhost:3000";

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    public static PlanetApiClient Instance { get; private set; }

    public IEnumerator GetActivePlanet(
        string accessToken,
        System.Action<PlanetDto> onOk,
        System.Action<string> onErr)
    {
        var url = $"{baseUrl}/api/planets/active";

        using var req = UnityWebRequest.Get(url);
        req.downloadHandler = new DownloadHandlerBuffer();

        req.SetRequestHeader("Accept", "application/json");

        if (!string.IsNullOrEmpty(accessToken))
            req.SetRequestHeader("Authorization", $"Bearer {accessToken}");
        else
            Debug.LogWarning("[PlanetApiClient] WARNING: Authorization header NOT set (empty token).");

        yield return req.SendWebRequest();

        var raw = req.downloadHandler?.text ?? "";
    
        var headers = req.GetResponseHeaders();
        if (headers != null)
        {
            foreach (var kv in headers)
                Debug.Log($"[PlanetApiClient] Header: {kv.Key} = {kv.Value}");
        }
        else
        {
            Debug.Log("[PlanetApiClient] No response headers.");
        }

        // --- handle connection/protocol errors ---
        if (req.result != UnityWebRequest.Result.Success)
        {
            var msg = $"GET /api/planets/active failed: result={req.result} code={req.responseCode} err={req.error} body={raw}";
            onErr?.Invoke(msg);
            yield break;
        }

        PlanetDto planet = null;
        try
        {
            planet = JsonConvert.DeserializeObject<PlanetDto>(raw);
        }
        catch (System.Exception ex)
        {
            onErr?.Invoke("JSON parse error: " + ex.Message + "\nRaw:\n" + raw);
            yield break;
        }

        if (planet == null)
        {
            onErr?.Invoke("GetActivePlanet: planet deserialized to NULL.\nRaw:\n" + raw);
            yield break;
        }

        Debug.Log($"[PlanetApiClient] Parsed planet: planetId={(planet.planetId ?? "(null)")}");

        if (string.IsNullOrEmpty(planet.planetId))
        {
            onErr?.Invoke("GetActivePlanet: invalid response (missing planetId).\nRaw:\n" + raw);
            yield break;
        }

        onOk?.Invoke(planet);
    }
}
