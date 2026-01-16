using System.Collections.Generic;
using UnityEngine;

public class HexMapManager : MonoBehaviour
{
    [Header("Prefabs")]
    public HexCell plusCellPrefab;

    [Header("Map Root")]
    public Transform mapRoot;

    [Header("Hex Settings")]
    public float hexSize = 1f;

    private Dictionary<Vector2Int, HexCell> cells =
        new Dictionary<Vector2Int, HexCell>();

    private static readonly Vector2Int[] axialDirs =
    {
        new Vector2Int(1, 0),
        new Vector2Int(1, -1),
        new Vector2Int(0, -1),
        new Vector2Int(-1, 0),
        new Vector2Int(-1, 1),
        new Vector2Int(0, 1),
    };

    private void Start()
    {
    
    }

    public HexCell GetCell(int q, int r)
    {
        cells.TryGetValue(new Vector2Int(q, r), out HexCell cell);
        return cell;
    }

    public HexCell SpawnPlusCell(int q, int r)
    {
        var key = new Vector2Int(q, r);
        if (cells.ContainsKey(key))
            return cells[key];

        Vector3 worldPos = AxialToWorld(q, r);
        HexCell cell = Instantiate(
            plusCellPrefab,
            worldPos,
            Quaternion.identity,
            mapRoot
        );

        cell.Init(q, r, true);
        cells[key] = cell;
        return cell;
    }

    public IEnumerable<Vector2Int> GetNeighbors(int q, int r)
    {
        foreach (var dir in axialDirs)
            yield return new Vector2Int(q + dir.x, r + dir.y);
    }

    public Vector3 AxialToWorld(int q, int r)
    {
        // Axial -> XZ plane
        float x = hexSize * (Mathf.Sqrt(3f) * q + Mathf.Sqrt(3f) / 2f * r);
        float z = hexSize * (3f / 2f * r);
        return new Vector3(x, 0f, z);
    }

    public void RemoveCell(int q, int r)
    {
        var key = new Vector2Int(q, r);
        if (cells.TryGetValue(key, out HexCell cell))
        {
            Destroy(cell.gameObject);
            cells.Remove(key);
        }
    }

    public void ResetGrid()
    {
        foreach (var kv in cells)
        {
            if (kv.Value != null)
                Destroy(kv.Value.gameObject);
        }
        cells.Clear();
    }

}
