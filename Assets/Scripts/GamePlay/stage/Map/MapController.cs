using System;
using System.Collections.Generic;
using UnityEngine;

public class MapController : MonoBehaviour
{
    [Header("Refs")]
    public HexMapManager mapManager;
    public Transform MapRoot;

    [Header("Settings")]
    public float tileHeightY = 0.5f;

    // Occupied axial coords (q,r)
    private readonly HashSet<Vector2Int> occupied = new();

    // ✅ single source of truth for server state
    private readonly List<PlacedTileDto> placedTiles = new();

    public event Action OnMapStateChanged;

    public IReadOnlyList<PlacedTileDto> GetPlacedTiles() => placedTiles;

    // ===============================
    // New stage: start with first plus
    // ===============================
    public void InitEmptyMap()
    {
        if (mapManager == null)
        {
            Debug.LogError("[MapController] mapManager is null");
            return;
        }

        mapManager.SpawnPlusCell(0, 0);
    }

    // ===============================
    // Load tiles from server into the map
    // ===============================
    public void LoadPlacedTilesFromServer(IEnumerable<PlacedTileDto> tiles, TileFactory factory)
    {
        // אם אין אריחים בכלל -> יצירת פלוס ראשון ולצאת
        if (tiles == null)
        {
            ClearMap();
            InitEmptyMap();
            return;
        }

        // להפוך ל-List כדי לבדוק Count פעם אחת
        var list = (tiles as IList<PlacedTileDto>) ?? new List<PlacedTileDto>(tiles);

        if (list.Count == 0)
        {
            ClearMap();
            InitEmptyMap();
            return;
        }

        if (mapManager == null)
        {
            Debug.LogError("[MapController] mapManager is null");
            return;
        }
        if (MapRoot == null)
        {
            Debug.LogError("[MapController] MapRoot is null");
            return;
        }
        if (factory == null)
        {
            Debug.LogError("[MapController] TileFactory is null");
            return;
        }

        ClearMapVisualOnly();     // destroy visuals + plus cells (via RemoveCell usage)
        occupied.Clear();
        placedTiles.Clear();

        bool hasAnyTiles = false;

        if (tiles != null)
        {
            foreach (var t in tiles)
            {
                if (t == null || t.coord == null || string.IsNullOrEmpty(t.tileId))
                    continue;

                hasAnyTiles = true;

                int q = t.coord.q;
                int r = t.coord.r;

                var key = new Vector2Int(q, r);
                occupied.Add(key);

                // remove a plus cell if exists in that coord
                mapManager.RemoveCell(q, r);

                var go = factory.CreateTileByTemplateId(t.tileId, MapRoot);
                if (go == null) continue;

                Vector3 pos = mapManager.AxialToWorld(q, r) + Vector3.up * tileHeightY;
                go.transform.position = pos;
                go.transform.rotation = Quaternion.Euler(0f, t.rotation * 60f, 0f);

                placedTiles.Add(t);
            }
        }

        if (!hasAnyTiles)
        {
            // stage is empty -> first plus
            InitEmptyMap();
        }
        else
        {
            // tiles exist -> create surrounding plus cells
            RefreshPlaceableCells();
        }

        OnMapStateChanged?.Invoke();
    }

    // ===============================
    // Create plus cells around occupied tiles
    // ===============================
    public void RefreshPlaceableCells()
    {
        if (mapManager == null) return;
        if (occupied.Count == 0) return;

        foreach (var pos in occupied)
        {
            foreach (var n in mapManager.GetNeighbors(pos.x, pos.y))
            {
                if (occupied.Contains(n)) continue;

                var cell = mapManager.GetCell(n.x, n.y);
                if (cell == null)
                    mapManager.SpawnPlusCell(n.x, n.y);
            }
        }
    }

    // ===============================
    // Try place a dragged tile onto a plus cell
    // ===============================
    public bool TryPlaceTile(int q, int r, int rotation, GameObject draggedTile, string templateId)
    {
        if (mapManager == null || MapRoot == null) return false;

        var key = new Vector2Int(q, r);
        if (occupied.Contains(key)) return false;

        HexCell cell = mapManager.GetCell(q, r);
        if (cell == null || cell.occupied || !cell.isPlusCell) return false;

        // mark occupied + remove plus cell
        occupied.Add(key);
        mapManager.RemoveCell(q, r);

        // move dragged tile to map
        Vector3 pos = mapManager.AxialToWorld(q, r) + Vector3.up * tileHeightY;

        draggedTile.transform.SetParent(MapRoot);
        draggedTile.transform.position = pos;
        draggedTile.transform.rotation = Quaternion.Euler(0f, rotation * 60f, 0f);

        // update server state list
        placedTiles.Add(new PlacedTileDto
        {
            tileId = templateId,
            rotation = rotation,
            coord = new CoordDto { q = q, r = r }
        });

        // spawn new placeable cells around
        RefreshPlaceableCells();

        // notify once
        OnMapStateChanged?.Invoke();
        return true;
    }

    // ===============================
    // Clear everything (state + visuals)
    // ===============================
    public void ClearMap()
    {
        occupied.Clear();
        placedTiles.Clear();
        ClearMapVisualOnly();
        OnMapStateChanged?.Invoke();
    }

    // Destroys only visuals under MapRoot.
    private void ClearMapVisualOnly()
    {
        // 1) לנקות תאים/פלוסים שה-HexMapManager יצר
        if (mapManager != null)
            mapManager.ResetGrid();

        // 2) לנקות אריחים שהונחו (שהם ילדים של MapRoot)
        if (MapRoot == null) return;

        for (int i = MapRoot.childCount - 1; i >= 0; i--)
            Destroy(MapRoot.GetChild(i).gameObject);
    }


}
