using UnityEngine;

/// <summary>
/// Visual component for a hex tile GameObject.
/// Manages center and edge renderers, applies materials from templates.
/// </summary>
public class HexTileView : MonoBehaviour
{
    [Header("Renderers")]
    public Renderer[] faces = new Renderer[6]; // Face01..Face06
    public Renderer center;

    public string templateId;
    public string templateType;
    public string centerKey;
    public string[] edges = new string[6];

<<<<<<< HEAD
    private const string PrefabBasePath = "Prefabs/hex2/level";

=======
>>>>>>> origin/main
    public void ApplyTemplate(HexTileTemplateDto t, MaterialLibrary lib)
    {
        templateId = t._id;
        templateType = t.type;

        centerKey = t.center;
        edges = (t.edges != null && t.edges.Length == 6) ? (string[])t.edges.Clone() : new string[6];

<<<<<<< HEAD
        // center — level 1 default
=======
        // center
>>>>>>> origin/main
        if (center != null)
        {
            var mat = lib.Get(centerKey);
            if (mat != null) center.sharedMaterial = mat;
<<<<<<< HEAD
            SpawnLeveledPrefab(centerKey, 0, 1, center.transform, new Vector3(90f, 0f, 0f));
        }

        // faces — level 1 default
=======
        }

        // faces
>>>>>>> origin/main
        for (int i = 0; i < 6; i++)
        {
            if (faces[i] == null) continue;
            string key = (edges != null && edges.Length == 6) ? edges[i] : null;
            var mat = lib.Get(key);
            if (mat != null) faces[i].sharedMaterial = mat;
<<<<<<< HEAD
            int variant = Random.value < 0.5f ? 1 : 2;
            SpawnLeveledPrefab(key, variant, 1, faces[i].transform, FaceRotations[i]);
        }
    }

    /// <summary>
    /// Overrides face/center prefabs with the correct level versions from server data.
    /// Call this after ApplyTemplate when placed tile face data is available.
    /// </summary>
    public void ApplyFaceLevels(System.Collections.Generic.List<FaceDataDto> faceData, CenterDataDto centerData)
    {
        if (center != null && centerData != null && !string.IsNullOrEmpty(centerData.resource))
        {
            ClearResourcePrefabs(center.transform);
            SpawnLeveledPrefab(centerData.resource, 0, centerData.level, center.transform, new Vector3(90f, 0f, 0f));
        }

        for (int i = 0; i < 6; i++)
        {
            if (faces[i] == null) continue;
            var fd = (faceData != null && i < faceData.Count) ? faceData[i] : null;
            if (fd == null || string.IsNullOrEmpty(fd.resource)) continue;
            ClearResourcePrefabs(faces[i].transform);
            SpawnLeveledPrefab(fd.resource, fd.variant, fd.level, faces[i].transform, FaceRotations[i]);
        }
    }

    private static readonly Vector3[] FaceRotations =
    {
        new Vector3( 30f,  90f,  90f),
        new Vector3(-30f,  90f,  90f),
        new Vector3(-90f,  90f,  90f),
        new Vector3(-30f, -90f, -90f),
        new Vector3( 30f, -90f, -90f),
        new Vector3( 90f,   0f,   0f),
    };

    [SerializeField] private float resourceScale = 0.01f;

    /// <summary>
    /// Spawns a resource prefab with the correct level subfolder and name.
    /// variant=0 means center (e.g. "rockC", "rockC2"), variant>0 means face (e.g. "rock1", "rock21").
    /// </summary>
    private void SpawnLeveledPrefab(string resource, int variant, int level, Transform parent, Vector3 rotation)
    {
        if (string.IsNullOrEmpty(resource)) return;
        int lvl = Mathf.Clamp(level, 1, 3);

        string prefabName;
        if (variant == 0) // center
            prefabName = lvl == 1 ? $"{resource}C" : $"{resource}C{lvl}";
        else // face
            prefabName = lvl == 1 ? $"{resource}{variant}" : $"{resource}{lvl}{variant}";

        string path = PrefabBasePath + lvl + "/" + prefabName;
        var prefab = Resources.Load<GameObject>(path);
        if (prefab == null) return;
        var go = Instantiate(prefab, parent.position, parent.rotation, parent);
        go.transform.localScale = Vector3.one * resourceScale;
        go.transform.localRotation = Quaternion.Euler(rotation);
    }

    private void ClearResourcePrefabs(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
            Destroy(parent.GetChild(i).gameObject);
    }

=======
        }
    }

>>>>>>> origin/main
    public void RotateEdgesCW()
    {
        if (edges == null || edges.Length != 6) return;
        // CW: last -> first
        string last = edges[5];
        for (int i = 5; i > 0; i--) edges[i] = edges[i - 1];
        edges[0] = last;
    }
}
