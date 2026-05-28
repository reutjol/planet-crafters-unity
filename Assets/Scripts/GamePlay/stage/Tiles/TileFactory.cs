using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class TileFactory : MonoBehaviour
{
    [FormerlySerializedAs("templateService")] [SerializeField] private HexTileTemplateService _templateService;

    private const string HexPrefabPath = "Prefabs/hex/";
    private readonly Dictionary<string, GameObject> _prefabCache = new();

    public HexTileTemplateService TemplateService
    {
        set => _templateService = value;
    }

    public GameObject CreateTileByTemplateId(string templateId, Transform parent)
    {
        if (!_templateService.IsReady)
        {
            Debug.LogError("[TileFactory] Templates not ready yet.");
            return null;
        }

        if (!_templateService.TemplatesById.TryGetValue(templateId, out var t))
        {
            Debug.LogError($"[TileFactory] Template id not found: {templateId}");
            return null;
        }

        if (!_prefabCache.TryGetValue(t.type, out var prefab))
        {
            prefab = Resources.Load<GameObject>(HexPrefabPath + t.type);
            if (prefab == null)
            {
                Debug.LogError($"[TileFactory] Hex prefab not found: {HexPrefabPath + t.type}");
                return null;
            }
            _prefabCache[t.type] = prefab;
        }

        var go = Instantiate(prefab, parent, false);
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;

        var view = go.GetComponent<HexTileView>();
        if (view == null) view = go.AddComponent<HexTileView>();

        string[] edges = (t.edges != null && t.edges.Length == 6)
            ? (string[])t.edges.Clone()
            : new string[6];
        view.SetData(t._id, t.type, edges);

        return go;
    }
}
