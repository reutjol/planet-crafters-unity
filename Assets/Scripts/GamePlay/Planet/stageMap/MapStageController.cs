using System.Collections.Generic;
using UnityEngine;

public class MapStageController : MonoBehaviour
{
    [Header("Prefabs by Resource Type")]
    [SerializeField] private GameObject rockPrefab;
    [SerializeField] private GameObject terraPrefab;
    [SerializeField] private GameObject bioPrefab;
    [SerializeField] private GameObject crystalPrefab;

    [Header("Tint")]
    [Range(0f, 1f)] [SerializeField] private float lockedDarkness = 0.35f;

    [Header("Config")]
    [SerializeField] private GameConfig gameConfig;

    [Header("Scene")]
    [SerializeField] private Transform stagesParent;

    [Header("Layout")]
    [SerializeField] private float hexSize = 1.0f;
    [SerializeField] private bool pointyTop = true;

    // Survive scene reloads
    private static Transform _persistentParent;
    private static readonly Dictionary<string, GameObject> _stageObjects = new();
    private static readonly Dictionary<string, bool> _lockedState = new();
    // Original shared materials saved before tinting, used to restore on unlock
    private static readonly Dictionary<string, List<(Renderer r, Material[] mats)>> _originalMaterials = new();

    private void OnEnable()
    {
        if (GameManager.Instance == null) return;
        GameManager.Instance.OnPlanetLoaded += HandlePlanetLoaded;
        GameManager.Instance.OnUnauthorized += HandleUnauthorized;
        GameManager.Instance.OnError += HandleError;
    }

    private void OnDisable()
    {
        if (GameManager.Instance == null) return;
        GameManager.Instance.OnPlanetLoaded -= HandlePlanetLoaded;
        GameManager.Instance.OnUnauthorized -= HandleUnauthorized;
        GameManager.Instance.OnError -= HandleError;

        if (_persistentParent != null)
            _persistentParent.gameObject.SetActive(false);
    }

    [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticState()
    {
        _persistentParent = null;
        _stageObjects.Clear();
        _lockedState.Clear();
        _originalMaterials.Clear();
    }

    public static void ResetPersistentState()
    {
        if (_persistentParent != null)
        {
            UnityEngine.Object.Destroy(_persistentParent.gameObject);
            _persistentParent = null;
        }
        _stageObjects.Clear();
        _lockedState.Clear();
        _originalMaterials.Clear();
    }

    private void Start()
    {
        if (gameConfig == null)
            gameConfig = Resources.Load<GameConfig>("GameConfig");

        if (stagesParent == null)
        {
            Debug.LogError("[MapStageController] stagesParent is NULL (assign in Inspector).");
            return;
        }

        if (GameManager.Instance == null)
        {
            Debug.LogError("[MapStageController] GameManager.Instance is NULL.");
            return;
        }

        if (AppSession.Instance == null)
        {
            Debug.LogError("[MapStageController] AppSession.Instance is NULL.");
            return;
        }

        if (_persistentParent != null)
            _persistentParent.gameObject.SetActive(true);

        GameManager.Instance.RequestActivePlanet(forceRefresh: true);
    }

    private void HandleUnauthorized()
    {
        Debug.LogWarning("[MapStageController] Unauthorized (401). Session may be missing/expired.");
    }

    private void HandleError(string err)
    {
        Debug.LogError("[MapStageController] Error: " + err);
    }

    private void HandlePlanetLoaded(PlanetDto planet)
    {
        if (planet == null)
        {
            Debug.LogError("[MapStageController] planet is NULL");
            return;
        }

        if (_persistentParent != null)
        {
            UpdateStages(planet);
            return;
        }

        stagesParent.SetParent(null);
        DontDestroyOnLoad(stagesParent.gameObject);
        _persistentParent = stagesParent;

        DrawStages(planet);
    }

    // First load — instantiate all stage objects
    private void DrawStages(PlanetDto planet)
    {
        if (planet?.stages == null) return;

        foreach (var stage in planet.stages)
        {
            if (stage?.meta == null || stage.meta.isMatchStage || stage.meta.coord == null) continue;

            var prefab = ChoosePrefab(stage);
            if (!prefab) continue;

            bool unlocked = stage.meta.isUnlocked;
            bool completed = stage.meta.isCompleted;

            var pos = AxialToWorld(stage.meta.coord.q, stage.meta.coord.r);
            pos.y += prefab.transform.localPosition.y;
            var go = Instantiate(prefab, pos, prefab.transform.rotation, _persistentParent);
            go.name = $"{stage.stageId} ({stage.meta.coord.q},{stage.meta.coord.r})";

            _stageObjects[stage.stageId] = go;
            _lockedState[stage.stageId] = !unlocked;

            if (!unlocked)
                SaveOriginalMaterials(stage.stageId, go);

            ApplyStateTint(go, unlocked);

            var view = go.GetComponent<StageNodeView>();
            if (view != null)
                view.Init(stage.stageId, unlocked, completed, stage.meta.resourceType,
                    stage.state?.progress?.score ?? 0,
                    stage.state?.progress?.developedPercent ?? 0f,
                    stage.meta?.coinsAwarded ?? 0);
        }
    }

    // Subsequent loads — only update what changed
    private void UpdateStages(PlanetDto planet)
    {
        if (planet?.stages == null) return;

        foreach (var stage in planet.stages)
        {
            if (stage?.meta == null || !_stageObjects.TryGetValue(stage.stageId, out var go)) continue;

            bool unlocked = stage.meta.isUnlocked;
            bool completed = stage.meta.isCompleted;
            bool wasLocked = _lockedState.GetValueOrDefault(stage.stageId, true);

            // If stage just became unlocked — restore original materials to remove dark tint
            if (unlocked && wasLocked)
            {
                RestoreOriginalMaterials(stage.stageId);
                _lockedState[stage.stageId] = false;
            }

            var view = go.GetComponent<StageNodeView>();
            if (view != null)
                view.Init(stage.stageId, unlocked, completed, stage.meta.resourceType,
                    stage.state?.progress?.score ?? 0,
                    stage.state?.progress?.developedPercent ?? 0f,
                    stage.meta?.coinsAwarded ?? 0);
        }
    }

    private GameObject ChoosePrefab(StageDto stage)
    {
        GameObject prefab = stage.meta.resourceType switch
        {
            "terra"   => terraPrefab,
            "bio"     => bioPrefab,
            "crystal" => crystalPrefab,
            _         => rockPrefab,
        };

        if (!prefab)
            Debug.LogError($"[MapStageController] Prefab for resourceType '{stage.meta.resourceType}' is NULL.");

        return prefab;
    }

    private void SaveOriginalMaterials(string stageId, GameObject go)
    {
        var list = new List<(Renderer r, Material[] mats)>();
        foreach (var r in go.GetComponentsInChildren<Renderer>())
            list.Add((r, r.sharedMaterials));
        _originalMaterials[stageId] = list;
    }

    private static void RestoreOriginalMaterials(string stageId)
    {
        if (!_originalMaterials.TryGetValue(stageId, out var list)) return;
        foreach (var (r, mats) in list)
            if (r != null) r.sharedMaterials = mats;
        _originalMaterials.Remove(stageId);
    }

    private void ApplyStateTint(GameObject go, bool unlocked)
    {
        if (unlocked) return;

        var tint = new Color(lockedDarkness, lockedDarkness, lockedDarkness, 1f);
        foreach (var r in go.GetComponentsInChildren<Renderer>())
        {
            foreach (var mat in r.materials)
            {
                if (mat.shader.name.Contains("TextMeshPro")) continue;
                mat.color *= tint;
            }
        }
    }

    private Vector3 AxialToWorld(int q, int r)
    {
        float x, z;

        if (pointyTop)
        {
            x = hexSize * (Mathf.Sqrt(3f) * q + Mathf.Sqrt(3f) / 2f * r);
            z = hexSize * (3f / 2f * r);
        }
        else
        {
            x = hexSize * (3f / 2f * q);
            z = hexSize * (Mathf.Sqrt(3f) / 2f * q + Mathf.Sqrt(3f) * r);
        }

        return new Vector3(x, 0f, z);
    }
}
