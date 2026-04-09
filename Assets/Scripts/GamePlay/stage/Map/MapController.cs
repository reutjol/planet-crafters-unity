using System;
<<<<<<< HEAD
using System.Collections;
=======
>>>>>>> origin/main
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Main controller for the hex tile map gameplay.
/// Manages tile placement, loading from server state, and placeable cell generation.
/// </summary>
public class MapController : MonoBehaviour
{
    [Header("Refs")]
    public HexMapManager mapManager;
    public Transform MapRoot;

    [Header("Settings")]
    public float tileHeightY = 0.5f;

<<<<<<< HEAD
    [Header("Star Effect")]
    public GameObject starPrefab;
    public float starRiseHeight = 1.5f;
    public float starDuration = 0.8f;

    [Header("Bonus Text")]
    public TMPro.TextMeshProUGUI bonusText;
    public float bonusTextDuration = 5f;

    // Set by GameBootstrap - needed for ApplyServerState
    [HideInInspector] public TileFactory tileFactory;

    // Occupied axial coords (q,r)
    private readonly HashSet<Vector2Int> occupied = new();

    // Live tile GameObjects on the map, keyed by axial coord
    private readonly Dictionary<Vector2Int, GameObject> liveTiles = new();

    // ✅ single source of truth for server state
    private readonly List<PlacedTileDto> placedTiles = new();

    private int previousScore = 0;
    private Vector2Int? lastPlacedCoord = null;

    public event Action OnMapStateChanged;
    public event Action<ProgressDto> OnProgressChanged;
    public event Action OnStageCompleted;
    public event Action<int> OnStageCompletedWithCoins;
=======
    // Occupied axial coords (q,r)
    private readonly HashSet<Vector2Int> occupied = new();

    // ✅ single source of truth for server state
    private readonly List<PlacedTileDto> placedTiles = new();

    public event Action OnMapStateChanged;

    public IReadOnlyList<PlacedTileDto> GetPlacedTiles() => placedTiles;
>>>>>>> origin/main

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
<<<<<<< HEAD
    // Load tiles from server into the map (Diff — only spawns new tiles)
    // ===============================
    public void LoadPlacedTilesFromServer(IEnumerable<PlacedTileDto> tiles, TileFactory factory)
    {
        var list = tiles != null
            ? ((tiles as IList<PlacedTileDto>) ?? new List<PlacedTileDto>(tiles))
            : new List<PlacedTileDto>();
=======
    // Load tiles from server into the map
    // ===============================
    public void LoadPlacedTilesFromServer(IEnumerable<PlacedTileDto> tiles, TileFactory factory)
    {
        // If no tiles exist, create the first plus cell and exit
        if (tiles == null)
        {
            ClearMap();
            InitEmptyMap();
            return;
        }

        // Convert to List to check Count once
        var list = (tiles as IList<PlacedTileDto>) ?? new List<PlacedTileDto>(tiles);
>>>>>>> origin/main

        if (list.Count == 0)
        {
            ClearMap();
            InitEmptyMap();
            return;
        }

<<<<<<< HEAD
        if (mapManager == null) { Debug.LogError("[MapController] mapManager is null"); return; }
        if (MapRoot == null)    { Debug.LogError("[MapController] MapRoot is null"); return; }
        if (factory == null)    { Debug.LogError("[MapController] TileFactory is null"); return; }

        bool isInitialLoad = liveTiles.Count == 0;

        var newKeys = new HashSet<Vector2Int>();
        occupied.Clear();
        placedTiles.Clear();

        foreach (var t in list)
        {
            if (t == null || t.coord == null || string.IsNullOrEmpty(t.tileId)) continue;

            int q = t.coord.q;
            int r = t.coord.r;
            var key = new Vector2Int(q, r);

            newKeys.Add(key);
            occupied.Add(key);
            placedTiles.Add(t);

            // Already on the map — skip
            if (liveTiles.ContainsKey(key)) continue;

            // New tile — spawn it
            mapManager.RemoveCell(q, r);

            var go = factory.CreateTileByTemplateId(t.tileId, MapRoot);
            if (go == null) continue;

            // Apply server-supplied face levels if available
            if (t.faces != null && t.faces.Count > 0)
            {
                var view = go.GetComponent<HexTileView>();
                view?.ApplyFaceLevels(t.faces, t.center);
            }

            Vector3 pos = mapManager.AxialToWorld(q, r) + Vector3.up * tileHeightY;
            go.transform.position = pos;
            go.transform.rotation = Quaternion.Euler(0f, t.rotation * 60f, 0f);

            var draggable = go.GetComponent<DraggableTile>();
            if (draggable != null) Destroy(draggable);

            liveTiles[key] = go;

            if (!isInitialLoad)
            {
                lastPlacedCoord = key;
                StartCoroutine(RaiseTile(go, pos));
            }
        }

        // Remove tiles that no longer exist in the server state
        var toRemove = new List<Vector2Int>();
        foreach (var key in liveTiles.Keys)
            if (!newKeys.Contains(key))
                toRemove.Add(key);

        foreach (var key in toRemove)
        {
            Destroy(liveTiles[key]);
            liveTiles.Remove(key);
        }

        // Rebuild plus cells
        mapManager.ResetGrid();
        RefreshPlaceableCells();
=======
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

        ClearMapVisualOnly();   
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

                // Remove DraggableTile component - tiles on map should not be draggable
                var draggable = go.GetComponent<DraggableTile>();
                if (draggable != null) Destroy(draggable);

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
>>>>>>> origin/main

        OnMapStateChanged?.Invoke();
    }

    // ===============================
<<<<<<< HEAD
    // Raise animation — tile rises from below ground
    // ===============================
    private IEnumerator RaiseTile(GameObject tile, Vector3 finalPos)
    {
        Vector3 startPos = finalPos - Vector3.up * 2f;
        tile.transform.position = startPos;

        float duration = 0.35f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float eased = 1f - Mathf.Pow(1f - t, 3f); // EaseOutCubic
            tile.transform.position = Vector3.Lerp(startPos, finalPos, eased);
            yield return null;
        }

        tile.transform.position = finalPos;
    }

    // ===============================
=======
>>>>>>> origin/main
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
<<<<<<< HEAD
    // Debug: trigger bonus visual + progress update without touching the map
    // ===============================
    public void DebugTriggerBonus(int bonusPoints)
    {
        ShowBonusText(bonusPoints);
        previousScore += bonusPoints;
        var fakeProgress = new ProgressDto { score = previousScore, isCompleted = false };
        OnProgressChanged?.Invoke(fakeProgress);
    }

    // ===============================
    // Apply full state received from server
    // ===============================
    public void ApplyServerState(PlanetStageStateDto state)
    {
        int oldScore = previousScore;
        lastPlacedCoord = null;

        LoadPlacedTilesFromServer(state.map?.placedTiles, tileFactory);

        if (state.progress != null)
        {
            previousScore = state.progress.score;

            if (lastPlacedCoord.HasValue && starPrefab != null && state.scoredConnections != null && state.scoredConnections.Count > 0)
                SpawnConnectionStars(lastPlacedCoord.Value, state.scoredConnections);

            if (state.bonusPoints > 0)
                ShowBonusText(state.bonusPoints);

            OnProgressChanged?.Invoke(state.progress);
            if (state.progress.isCompleted)
            {
                OnStageCompleted?.Invoke();
                OnStageCompletedWithCoins?.Invoke(state.coinsAwarded);
            }
        }
    }

    // ===============================
    // Spawn stars at midpoints between new tile and its occupied neighbors
    // ===============================
    private void SpawnConnectionStars(Vector2Int newCoord, System.Collections.Generic.List<CoordDto> scoredConnections)
    {
        Vector3 newWorldPos = mapManager.AxialToWorld(newCoord.x, newCoord.y) + Vector3.up * tileHeightY;

        foreach (var conn in scoredConnections)
        {
            Vector3 neighborWorldPos = mapManager.AxialToWorld(conn.q, conn.r) + Vector3.up * tileHeightY;
            Vector3 midpoint = (newWorldPos + neighborWorldPos) * 0.5f;

            var star = Instantiate(starPrefab, midpoint, Quaternion.identity);
            StartCoroutine(StarRiseAnimation(star));
        }
    }

    // ===============================
    // Star rise + fade animation
    // ===============================
    private IEnumerator StarRiseAnimation(GameObject star)
    {
        Vector3 startPos = star.transform.position;
        Vector3 endPos = startPos + Vector3.up * starRiseHeight;

        float elapsed = 0f;
        while (elapsed < starDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / starDuration);

            // Rise
            star.transform.position = Vector3.Lerp(startPos, endPos, t);

            // Scale: grow quickly then shrink toward the end
            float scaleCurve = t < 0.3f
                ? t / 0.3f
                : 1f - ((t - 0.3f) / 0.7f);
            star.transform.localScale = Vector3.one * scaleCurve;

            yield return null;
        }

        Destroy(star);
    }

    // ===============================
    // Show +N bonus text on Canvas
    // ===============================
    private void ShowBonusText(int bonus)
    {
        if (bonusText == null) return;
        bonusText.text = $"+{bonus}";
        StopCoroutine("BonusTextAnimation");
        StartCoroutine(BonusTextAnimation());
    }

    private IEnumerator BonusTextAnimation()
    {
        Color startColor = bonusText.color;
        bonusText.color = new Color(startColor.r, startColor.g, startColor.b, 1f);
        bonusText.gameObject.SetActive(true);

        float elapsed = 0f;
        while (elapsed < bonusTextDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / bonusTextDuration);
            bonusText.color = new Color(startColor.r, startColor.g, startColor.b, 1f - t);
            yield return null;
        }

        bonusText.gameObject.SetActive(false);
=======
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

        // Remove DraggableTile component - tiles on map should not be draggable
        var draggable = draggedTile.GetComponent<DraggableTile>();
        if (draggable != null) Destroy(draggable);

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
>>>>>>> origin/main
    }

    // ===============================
    // Clear everything (state + visuals)
    // ===============================
    public void ClearMap()
    {
        occupied.Clear();
        placedTiles.Clear();
<<<<<<< HEAD
        liveTiles.Clear();
=======
>>>>>>> origin/main
        ClearMapVisualOnly();
        OnMapStateChanged?.Invoke();
    }

    // Destroys only visuals under MapRoot.
    private void ClearMapVisualOnly()
    {
        // 1) Clear cells/pluses created by HexMapManager
        if (mapManager != null)
            mapManager.ResetGrid();

        // 2) Clear placed tiles (children of MapRoot)
        if (MapRoot == null) return;

        for (int i = MapRoot.childCount - 1; i >= 0; i--)
            Destroy(MapRoot.GetChild(i).gameObject);
<<<<<<< HEAD

        liveTiles.Clear();
=======
>>>>>>> origin/main
    }


}
