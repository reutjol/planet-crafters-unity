using UnityEngine;

/// <summary>
/// Places Face00-Face05 and Center children at the correct hex positions.
/// Right-click the component in Inspector -> "Apply Hex Layout".
/// </summary>
public class HexTileLayout : MonoBehaviour
{
    private static readonly Vector3[] FacePositions =
    {
        new Vector3(-2.61f, 0f, 0f),
        new Vector3(-1.33f, 0f, 2.24f),
        new Vector3( 1.33f, 0f, 2.24f),
        new Vector3( 2.61f, 0f, 0f),
        new Vector3( 1.33f, 0f, -2.24f),
        new Vector3(-1.33f, 0f, -2.24f),
    };

    private static readonly float[] FaceRotationsY =
        { 0f, 60f, 120f, 180f, 240f, 300f };

    private void Awake() => ApplyLayout();

    [ContextMenu("Apply Hex Layout")]
    public void ApplyLayout()
    {
        for (int i = 0; i < 6; i++)
        {
            string name = $"Face0{i}";
            Transform face = transform.Find(name);
            if (face == null) { Debug.LogWarning($"[HexTileLayout] '{name}' not found"); continue; }

            face.localPosition = FacePositions[i];
            face.localRotation = Quaternion.Euler(0f, FaceRotationsY[i], 0f);
        }

        Transform center = transform.Find("Center");
        if (center != null)
        {
            center.localPosition = Vector3.zero;
            center.localRotation = Quaternion.Euler(90f, 0f, 0f);
        }
    }
}
