using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class MapController : MonoBehaviour
{
    [FormerlySerializedAs("mapManager")] [SerializeField] private HexMapManager _mapManager;
    [FormerlySerializedAs("MapRoot")]    [SerializeField] private Transform _mapRoot;

    [Header("Settings")]
    [FormerlySerializedAs("tileHeightY")] [SerializeField] private float _tileHeightY = 0.5f;

    [Header("Star Effect")]
    [FormerlySerializedAs("starPrefab")]      [SerializeField] private GameObject _starPrefab;
    [FormerlySerializedAs("starRiseHeight")]  [SerializeField] private float _starRiseHeight = 1.5f;
    [FormerlySerializedAs("starDuration")]    [SerializeField] private float _starDuration = 0.8f;

    [Header("Cluster Roamers")]
    [SerializeField] private List<ResourceRoamerConfig> _roamerConfigs = new();
    [SerializeField] private GameObject _defaultRoamerPrefab;
    [SerializeField] private float _defaultRoamerHeightY = 3f;
    [SerializeField] private float _defaultRoamerScale = 0.2f;
    [SerializeField] private float _defaultRoamerSpeed = 2.5f;

    [System.Serializable]
    public class ResourceRoamerConfig
    {
        public string resourceType;
        public GameObject prefab;
        public float heightY = 3f;
        public float scale = 1f;
        public float moveSpeed = 2.5f;
        public bool snapToTileSurface = false;
        public float surfaceOffset = 0.05f;
        public bool useWaypoints = false;
    }

    private TileFactory _tileFactory;

    public float TileHeightY => _tileHeightY;

    private readonly HashSet<Vector2Int> _occupied = new();
    private readonly Dictionary<Vector2Int, GameObject> _liveTiles = new();
    private readonly List<GameObject> _starPool = new();
    private readonly Dictionary<string, GameObject> _activeRoamers = new();

    private int _previousScore = 0;
    private Vector2Int? _lastPlacedCoord = null;
    private bool _hasLoadedServerMap = false;

    public event Action OnMapStateChanged;
    public event Action<ProgressDto> OnProgressChanged;
    public event Action OnStageCompleted;
    public event Action<int> OnStageCompletedWithCoins;
    public event Action<int> OnBonusScored;

    public void SetTileFactory(TileFactory factory) => _tileFactory = factory;

    public void InitEmptyMap()
    {
        if (_mapManager == null) { Debug.LogError("[MapController] mapManager is null"); return; }
        _mapManager.SpawnPlusCell(0, 0);
    }

    public void LoadPlacedTilesFromServer(IEnumerable<PlacedTileDto> tiles, TileFactory factory, bool animateNewTiles = false)
    {
        var list = tiles != null
            ? ((tiles as IList<PlacedTileDto>) ?? new List<PlacedTileDto>(tiles))
            : new List<PlacedTileDto>();

        if (list.Count == 0)
        {
            ClearMap();
            InitEmptyMap();
            return;
        }

        if (_mapManager == null) { Debug.LogError("[MapController] mapManager is null"); return; }
        if (_mapRoot == null)    { Debug.LogError("[MapController] MapRoot is null"); return; }
        if (factory == null)     { Debug.LogError("[MapController] TileFactory is null"); return; }

        var newKeys = new HashSet<Vector2Int>();
        _occupied.Clear();

        foreach (var t in list)
        {
            if (t == null || t.coord == null || string.IsNullOrEmpty(t.tileId)) continue;

            int q = t.coord.q;
            int r = t.coord.r;
            var key = new Vector2Int(q, r);

            newKeys.Add(key);
            _occupied.Add(key);

            if (_liveTiles.ContainsKey(key)) continue;

            _mapManager.RemoveCell(q, r);

            var go = factory.CreateTileByTemplateId(t.tileId, _mapRoot);
            if (go == null) continue;

            Vector3 pos = _mapManager.AxialToWorld(q, r) + Vector3.up * _tileHeightY;
            go.transform.position = pos;
            go.transform.rotation = Quaternion.Euler(0f, t.rotation * 60f, 0f);

            var draggable = go.GetComponent<DraggableTile>();
            if (draggable != null) { draggable.enabled = false; Destroy(draggable); }

            _liveTiles[key] = go;

            if (animateNewTiles)
            {
                _lastPlacedCoord = key;
                PlayAttachedSfx(go);
                StartCoroutine(RaiseTile(go, pos));
            }
        }

        var toRemove = new List<Vector2Int>();
        foreach (var key in _liveTiles.Keys)
            if (!newKeys.Contains(key))
                toRemove.Add(key);

        foreach (var key in toRemove)
        {
            Destroy(_liveTiles[key]);
            _liveTiles.Remove(key);
        }

        _mapManager.ResetGrid();
        RefreshPlaceableCells();

        OnMapStateChanged?.Invoke();
    }

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
            float eased = 1f - Mathf.Pow(1f - t, 3f);
            tile.transform.position = Vector3.Lerp(startPos, finalPos, eased);
            yield return null;
        }

        tile.transform.position = finalPos;
    }

    private static void PlayAttachedSfx(GameObject target)
    {
        if (target == null) return;

        var source = target.GetComponentInChildren<AudioSource>(true);
        if (source == null || source.clip == null) return;

        source.Stop();
        source.Play();
    }

    public void RefreshPlaceableCells()
    {
        if (_mapManager == null) return;
        if (_occupied.Count == 0) return;

        foreach (var pos in _occupied)
        {
            foreach (var n in _mapManager.GetNeighbors(pos.x, pos.y))
            {
                if (_occupied.Contains(n)) continue;

                var cell = _mapManager.GetCell(n.x, n.y);
                if (cell == null)
                    _mapManager.SpawnPlusCell(n.x, n.y);
            }
        }
    }

    public void DebugTriggerBonus(int bonusPoints)
    {
        OnBonusScored?.Invoke(bonusPoints);
        _previousScore += bonusPoints;
        var fakeProgress = new ProgressDto { score = _previousScore, isCompleted = false };
        OnProgressChanged?.Invoke(fakeProgress);
    }

    public void ApplyServerState(PlanetStageStateDto state)
    {
        bool animateNewTiles = _hasLoadedServerMap;
        _lastPlacedCoord = null;

        LoadPlacedTilesFromServer(state.map?.placedTiles, _tileFactory, animateNewTiles);
        _hasLoadedServerMap = true;

        if (state.progress != null)
        {
            _previousScore = state.progress.score;

            if (_lastPlacedCoord.HasValue && _starPrefab != null && state.scoredConnections != null && state.scoredConnections.Count > 0)
                SpawnConnectionStars(_lastPlacedCoord.Value, state.scoredConnections);

            if (state.bonusPoints > 0)
                OnBonusScored?.Invoke(state.bonusPoints);

            if (state.roamingClusters != null)
                UpdateRoamingClusters(state.roamingClusters);

            OnProgressChanged?.Invoke(state.progress);
            if (state.progress.isCompleted)
            {
                OnStageCompleted?.Invoke();
                OnStageCompletedWithCoins?.Invoke(state.coinsAwarded);
            }
        }
    }

    private static readonly Vector2Int[] HexDirs = {
        new(1,0), new(1,-1), new(0,-1), new(-1,0), new(-1,1), new(0,1)
    };

    private List<List<int>> BuildAdjacency(List<CoordDto> coords)
    {
        var indexMap = new Dictionary<Vector2Int, int>();
        for (int i = 0; i < coords.Count; i++)
            indexMap[new Vector2Int(coords[i].q, coords[i].r)] = i;

        var adj = new List<List<int>>();
        for (int i = 0; i < coords.Count; i++)
        {
            adj.Add(new List<int>());
            var c = new Vector2Int(coords[i].q, coords[i].r);
            foreach (var d in HexDirs)
                if (indexMap.TryGetValue(c + d, out int j))
                    adj[i].Add(j);
        }
        return adj;
    }

    private void UpdateRoamingClusters(List<RoamingClusterDto> clusters)
    {
        var newKeys = new HashSet<string>();
        var countPerType = new Dictionary<string, int>();

        foreach (var cluster in clusters)
        {
            if (cluster.coords == null || cluster.coords.Count == 0) continue;

            var resource = cluster.resourceType ?? "default";
            if (!countPerType.ContainsKey(resource)) countPerType[resource] = 0;
            var key = $"{resource}_{countPerType[resource]++}";
            newKeys.Add(key);

            var cfg = _roamerConfigs.Find(c => c.resourceType == resource);
            var height = cfg != null ? cfg.heightY : _defaultRoamerHeightY;

            var positions = new List<Vector3>();
            foreach (var c in cluster.coords)
            {
                Vector3 base3 = _mapManager.AxialToWorld(c.q, c.r);
                float y;
                if (cfg != null && cfg.snapToTileSurface)
                {
                    Vector3 rayOrigin = base3 + Vector3.up * 20f;
                    y = Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 40f)
                        ? hit.point.y + cfg.surfaceOffset
                        : base3.y + height;
                }
                else
                {
                    y = base3.y + height;
                }
                positions.Add(new Vector3(base3.x, y, base3.z));
            }

            var adj = BuildAdjacency(cluster.coords);

            bool useWaypoints = cfg != null && cfg.useWaypoints;

            if (useWaypoints)
            {
                var tiles = new List<GameObject>();
                foreach (var c in cluster.coords)
                {
                    _liveTiles.TryGetValue(new Vector2Int(c.q, c.r), out var tileGo);
                    tiles.Add(tileGo);
                }

                if (_activeRoamers.TryGetValue(key, out var existing) && existing != null)
                {
                    existing.GetComponent<WaypointRoamer>()?.UpdateTiles(tiles, adj);
                }
                else
                {
                    var prefab = cfg.prefab ?? _defaultRoamerPrefab;
                    if (prefab == null) continue;

                    var go = Instantiate(prefab);
                    var roamer = go.GetComponent<WaypointRoamer>() ?? go.AddComponent<WaypointRoamer>();
                    go.transform.localScale = Vector3.one * cfg.scale;
                    roamer.Initialize(tiles, adj, resource, cfg.moveSpeed);
                    _activeRoamers[key] = go;
                }
            }
            else if (_activeRoamers.TryGetValue(key, out var existing) && existing != null)
            {
                existing.GetComponent<ClusterRoamer>()?.UpdatePositions(positions, adj);
            }
            else
            {
                var prefab = cfg?.prefab ?? _defaultRoamerPrefab;
                if (prefab == null) continue;

                var go = Instantiate(prefab);
                var roamer = go.GetComponent<ClusterRoamer>() ?? go.AddComponent<ClusterRoamer>();
                go.transform.localScale = Vector3.one * (cfg != null ? cfg.scale : _defaultRoamerScale);
                roamer.Initialize(positions, adj, cfg != null ? cfg.moveSpeed : _defaultRoamerSpeed);
                _activeRoamers[key] = go;
            }
        }

        var toRemove = new List<string>();
        foreach (var kv in _activeRoamers)
            if (!newKeys.Contains(kv.Key)) toRemove.Add(kv.Key);
        foreach (var key in toRemove)
        {
            if (_activeRoamers[key] != null) Destroy(_activeRoamers[key]);
            _activeRoamers.Remove(key);
        }
    }

    private void SpawnConnectionStars(Vector2Int newCoord, List<CoordDto> scoredConnections)
    {
        Vector3 newWorldPos = _mapManager.AxialToWorld(newCoord.x, newCoord.y) + Vector3.up * _tileHeightY;

        foreach (var conn in scoredConnections)
        {
            Vector3 neighborWorldPos = _mapManager.AxialToWorld(conn.q, conn.r) + Vector3.up * _tileHeightY;
            Vector3 midpoint = (newWorldPos + neighborWorldPos) * 0.5f + Vector3.up * 0.5f;

            var star = GetPooledStar();
            star.transform.position = midpoint;
            star.transform.localScale = Vector3.one * 0.1f;
            star.SetActive(true);
            PlayAttachedSfx(star);
            StartCoroutine(StarRiseAnimation(star));
        }
    }

    private GameObject GetPooledStar()
    {
        foreach (var s in _starPool)
            if (!s.activeInHierarchy) return s;

        var newStar = Instantiate(_starPrefab);
        newStar.SetActive(false);
        _starPool.Add(newStar);
        return newStar;
    }

    private IEnumerator StarRiseAnimation(GameObject star)
    {
        Vector3 startPos = star.transform.position;
        Vector3 endPos = startPos + Vector3.up * _starRiseHeight;
        Vector3 baseScale = star.transform.localScale;

        float elapsed = 0f;
        while (elapsed < _starDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / _starDuration);

            star.transform.position = Vector3.Lerp(startPos, endPos, t);

            float scaleCurve = t < 0.3f
                ? t / 0.3f
                : 1f - ((t - 0.3f) / 0.7f);
            star.transform.localScale = baseScale * scaleCurve;

            yield return null;
        }

        star.SetActive(false);
    }

    public void ClearMap()
    {
        _occupied.Clear();
        _liveTiles.Clear();
        foreach (var kv in _activeRoamers)
            if (kv.Value != null) Destroy(kv.Value);
        _activeRoamers.Clear();
        ClearMapVisualOnly();
        OnMapStateChanged?.Invoke();
    }

    private void ClearMapVisualOnly()
    {
        if (_mapManager != null)
            _mapManager.ResetGrid();

        if (_mapRoot == null) return;

        for (int i = _mapRoot.childCount - 1; i >= 0; i--)
            Destroy(_mapRoot.GetChild(i).gameObject);

        _liveTiles.Clear();
    }
}
