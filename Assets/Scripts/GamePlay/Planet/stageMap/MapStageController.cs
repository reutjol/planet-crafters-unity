using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controller for the stage map scene.
/// Loads planet data from GameManager and spawns stage nodes in a hex layout.
/// Different prefabs are used for locked, unlocked, and completed stages.
/// </summary>
public class MapStageController : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject stageUnlockedPrefab;
    [SerializeField] private GameObject stageLockedPrefab;
    [SerializeField] private GameObject stageCompletedPrefab;

    [Header("Scene")]
    [SerializeField] private Transform stagesParent;

    [Header("Layout")]
    [SerializeField] private float hexSize = 1.0f;
    [SerializeField] private bool pointyTop = true;

    private readonly List<GameObject> spawned = new List<GameObject>();

    private void Awake()
    {
        Debug.Log("[MapStageController] Awake");
    }

    private void OnEnable()
    {
        // Check if GameManager exists before subscribing to events
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("[MapStageController] GameManager.Instance is null in OnEnable, skipping event subscription");
            return;
        }

        GameManager.Instance.OnPlanetLoaded += HandlePlanetLoaded;
        GameManager.Instance.OnUnauthorized += HandleUnauthorized;
        GameManager.Instance.OnError += HandleError;
    }

    private void OnDisable() {
        if (GameManager.Instance == null) return;
        GameManager.Instance.OnPlanetLoaded -= HandlePlanetLoaded;
        GameManager.Instance.OnUnauthorized -= HandleUnauthorized;
        GameManager.Instance.OnError -= HandleError;
    }

    private void Start()
    {
        Debug.Log("[MapStageController] Start");

        // --- Inspector wiring checks ---
        Debug.Log($"[MapStageController] stagesParent assigned? {(stagesParent != null)}");
        Debug.Log($"[MapStageController] prefabs assigned? unlocked={(stageUnlockedPrefab != null)}, locked={(stageLockedPrefab != null)}, completed={(stageCompletedPrefab != null)}");
        Debug.Log($"[MapStageController] layout: hexSize={hexSize}, pointyTop={pointyTop}");

        if (stagesParent == null)
        {
            Debug.LogError("[MapStageController] ERROR: stagesParent is NULL (assign in Inspector).");
            return;
        }

        // --- GameManager existence ---
        if (GameManager.Instance == null)
        {
            Debug.LogError("[MapStageController] ERROR: GameManager.Instance is NULL. (Is GameManager in Boot scene with DontDestroyOnLoad?)");
            return;
        }

        Debug.Log("[MapStageController] GameManager found. Subscribing to events...");

        // --- Session / token checks ---
        if (AppSession.Instance == null)
        {
            Debug.LogError("[MapStageController] ERROR: AppSession.Instance is NULL.");
            return;
        }

        Debug.Log($"[MapStageController] HasAccess? {AppSession.Instance.HasAccess()}");
        Debug.Log($"[MapStageController] AccessToken length: {(AppSession.Instance.AccessToken != null ? AppSession.Instance.AccessToken.Length : -1)}");

        // request planet (cache or fetch)
        Debug.Log("[MapStageController] RequestActivePlanet()...");
        GameManager.Instance.RequestActivePlanet();
    }

    private void HandleUnauthorized()
    {
        Debug.LogWarning("[MapStageController] OnUnauthorized (likely 401). Session may be missing/expired.");
    }

    private void HandleError(string err)
    {
        Debug.LogError("[MapStageController] OnError: " + err);
    }

    private void HandlePlanetLoaded(PlanetDto planet)
    {
        Debug.Log("[MapStageController] ✅ OnPlanetLoaded fired!");

        if (planet == null)
        {
            Debug.LogError("[MapStageController] planet is NULL");
            return;
        }

        Debug.Log($"[MapStageController] planetId={planet.planetId}");
        Debug.Log($"[MapStageController] stages array null? {(planet.stages == null)}");

        Clear();
        DrawStages(planet);

        Debug.Log($"[MapStageController] Draw complete. Spawned count={spawned.Count}");
    }

    private void Clear()
    {
        Debug.Log($"[MapStageController] Clear() destroying {spawned.Count} spawned objects...");
        foreach (var go in spawned)
        {
            if (go != null) Destroy(go);
        }
        spawned.Clear();
    }

    private void DrawStages(PlanetDto planet)
    {
        if (planet?.stages == null)
        {
            Debug.LogWarning("[MapStageController] DrawStages: planet.stages is NULL -> nothing to draw.");
            return;
        }

        Debug.Log("[MapStageController] DrawStages: begin loop...");

        int skippedNull = 0;
        int skippedNoCoord = 0;
        int skippedNoPrefab = 0;
        int drawn = 0;

        foreach (var stage in planet.stages)
        {
            if (stage == null)
            {
                skippedNull++;
                continue;
            }

            if (stage.meta == null)
            {
                Debug.LogWarning($"[MapStageController] stage {stage.stageId}: meta is NULL -> skip");
                skippedNoCoord++;
                continue;
            }

            if (stage.meta.coord == null)
            {
                Debug.LogWarning($"[MapStageController] stage {stage.stageId}: meta.coord is NULL -> skip");
                skippedNoCoord++;
                continue;
            }

            int q = stage.meta.coord.q;
            int r = stage.meta.coord.r;

            bool unlocked = stage.meta.isUnlocked;
            bool completed = stage.meta.isCompleted;

            var pos = AxialToWorld(q, r);
            var prefab = ChoosePrefab(stage);

            Debug.Log($"[MapStageController] stage={stage.stageId} q={q} r={r} unlocked={unlocked} completed={completed} -> worldPos={pos} prefab={(prefab ? prefab.name : "NULL")}");

            if (!prefab)
            {
                skippedNoPrefab++;
                continue;
            }

            var go = Instantiate(prefab, pos, Quaternion.identity, stagesParent);
            go.name = $"{stage.stageId} ({q},{r})";
            spawned.Add(go);
            drawn++;

            var view = go.GetComponent<StageNodeView>();
            if (view != null)
            {
                Debug.Log($"[MapStageController] StageNodeView found on {go.name} -> Init()");
                view.Init(stage.stageId, unlocked, completed);
            }
            else
            {
                Debug.Log($"[MapStageController] StageNodeView NOT found on {go.name} (optional)");
            }
        }

        Debug.Log($"[MapStageController] DrawStages summary: drawn={drawn}, skippedNull={skippedNull}, skippedNoCoord={skippedNoCoord}, skippedNoPrefab={skippedNoPrefab}");
    }

    private GameObject ChoosePrefab(StageDto stage)
    {
        if (stage.meta.isCompleted && stageCompletedPrefab) return stageCompletedPrefab;
        if (stage.meta.isUnlocked && stageUnlockedPrefab) return stageUnlockedPrefab;

        if (!stageLockedPrefab)
        {
            Debug.LogError("[MapStageController] stageLockedPrefab is NULL - cannot draw locked stages.");
            return null;
        }

        return stageLockedPrefab;
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
