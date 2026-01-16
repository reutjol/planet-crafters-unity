using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class HexTileTemplateService : MonoBehaviour
{
    [Header("Server")]
    public string url = "http://localhost:3000/api/hex-tiles";

    public bool IsReady { get; private set; }

    private Dictionary<string, HexTileTemplateDto> byId = new();
    public IReadOnlyDictionary<string, HexTileTemplateDto> TemplatesById => byId;

    public IEnumerator LoadTemplates()
    {
        IsReady = false;

        using var req = UnityWebRequest.Get(url);
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"LoadTemplates failed: {req.error} url={url}");
            yield break;
        }

        string json = req.downloadHandler.text;
        string wrapped = "{\"items\":" + json + "}";
        var list = JsonUtility.FromJson<HexTileTemplateListWrapper>(wrapped);

        byId.Clear();
        foreach (var t in list.items)
        {
            if (t == null || string.IsNullOrEmpty(t._id)) continue;
            byId[t._id] = t;
        }

        IsReady = true;
        Debug.Log($"Loaded templates: {byId.Count}");
    }
}
