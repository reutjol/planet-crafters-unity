using UnityEngine;

/// <summary>
/// Factory for creating tile GameObjects from templates.
/// Instantiates tile prefabs and applies visual configuration via HexTileView.
/// </summary>
public class TileFactory : MonoBehaviour
{
    public GameObject tilePrefab;
    public HexTileTemplateService templateService;
    public MaterialLibrary materialLibrary;

    public GameObject CreateTileByTemplateId(string templateId, Transform parent)
    {
        if (!templateService.IsReady)
        {
            Debug.LogError("Templates not ready yet.");
            return null;
        }

        if (!templateService.TemplatesById.TryGetValue(templateId, out var t))
        {
            Debug.LogError($"Template id not found: {templateId}");
            return null;
        }

        var go = Instantiate(tilePrefab, parent, false);
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;

        var view = go.GetComponent<HexTileView>();
        if (view == null)
        {
            Debug.LogError("Tile prefab missing HexTileView.");
            return go;
        }

        view.ApplyTemplate(t, materialLibrary);
        return go;
    }
}
