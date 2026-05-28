using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class HexMapManager : MonoBehaviour
{
    [FormerlySerializedAs("plusCellPrefab")] [SerializeField] private HexCell _plusCellPrefab;
    [FormerlySerializedAs("mapRoot")]        [SerializeField] private Transform _mapRoot;
    [FormerlySerializedAs("hexSize")]        [SerializeField] private float _hexSize = 1f;

    private readonly Dictionary<Vector2Int, HexCell> _cells = new();

    private static readonly Vector2Int[] AxialDirs =
    {
        new Vector2Int(1,  0),
        new Vector2Int(1, -1),
        new Vector2Int(0, -1),
        new Vector2Int(-1, 0),
        new Vector2Int(-1, 1),
        new Vector2Int(0,  1),
    };

    public HexCell GetCell(int q, int r)
    {
        _cells.TryGetValue(new Vector2Int(q, r), out HexCell cell);
        return cell;
    }

    public HexCell SpawnPlusCell(int q, int r)
    {
        var key = new Vector2Int(q, r);
        if (_cells.TryGetValue(key, out HexCell existing))
            return existing;

        Vector3 worldPos = AxialToWorld(q, r);
        HexCell cell = Instantiate(_plusCellPrefab, worldPos, Quaternion.identity, _mapRoot);
        cell.Init(q, r, true);
        _cells[key] = cell;
        return cell;
    }

    public IEnumerable<Vector2Int> GetNeighbors(int q, int r)
    {
        foreach (var dir in AxialDirs)
            yield return new Vector2Int(q + dir.x, r + dir.y);
    }

    public Vector3 AxialToWorld(int q, int r)
    {
        float x = _hexSize * (Mathf.Sqrt(3f) * q + Mathf.Sqrt(3f) / 2f * r);
        float z = _hexSize * (3f / 2f * r);
        return new Vector3(x, 0f, z);
    }

    public void RemoveCell(int q, int r)
    {
        var key = new Vector2Int(q, r);
        if (_cells.TryGetValue(key, out HexCell cell))
        {
            Destroy(cell.gameObject);
            _cells.Remove(key);
        }
    }

    public void ResetGrid()
    {
        foreach (var kv in _cells)
        {
            if (kv.Value != null)
                Destroy(kv.Value.gameObject);
        }
        _cells.Clear();
    }
}
