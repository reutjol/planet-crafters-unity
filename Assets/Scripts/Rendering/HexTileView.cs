using UnityEngine;

public class HexTileView : MonoBehaviour
{
    [Header("Renderers")]
    public Renderer[] faces = new Renderer[6]; // Face01..Face06
    public Renderer center;

    public string templateId;
    public string templateType;
    public string centerKey;
    public string[] edges = new string[6];

    public void ApplyTemplate(HexTileTemplateDto t, MaterialLibrary lib)
    {
        templateId = t._id;
        templateType = t.type;

        centerKey = t.center;
        edges = (t.edges != null && t.edges.Length == 6) ? (string[])t.edges.Clone() : new string[6];

        // center
        if (center != null)
        {
            var mat = lib.Get(centerKey);
            if (mat != null) center.sharedMaterial = mat;
        }

        // faces
        for (int i = 0; i < 6; i++)
        {
            if (faces[i] == null) continue;
            string key = (edges != null && edges.Length == 6) ? edges[i] : null;
            var mat = lib.Get(key);
            if (mat != null) faces[i].sharedMaterial = mat;
        }
    }

    public void RotateEdgesCW()
    {
        if (edges == null || edges.Length != 6) return;
        // CW: last -> first
        string last = edges[5];
        for (int i = 5; i > 0; i--) edges[i] = edges[i - 1];
        edges[0] = last;
    }
}
