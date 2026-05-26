using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controller for the stage map scene.
/// Loads planet data from GameManager and spawns stage nodes in a hex layout.
/// Each resource type has its own prefab; locked stages are darkened, completed stages are brightened.
/// </summary>
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

    private readonly List<GameObject> spawned = new List<GameObject>();

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

        Clear();
        DrawStages(planet);
    }

    private void Clear()
    {
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
            Debug.LogWarning("[MapStageController] planet.stages is NULL.");
            return;
        }

        foreach (var stage in planet.stages)
        {
            if (stage == null) continue;

            if (stage.meta == null)
            {
                Debug.LogWarning($"[MapStageController] stage {stage.stageId}: meta is NULL -> skip");
                continue;
            }

            if (stage.meta.coord == null)
            {
                Debug.LogWarning($"[MapStageController] stage {stage.stageId}: meta.coord is NULL -> skip");
                continue;
            }

            int q = stage.meta.coord.q;
            int r = stage.meta.coord.r;
            bool unlocked = stage.meta.isUnlocked;
            bool completed = stage.meta.isCompleted;

            var prefab = ChoosePrefab(stage);
            if (!prefab) continue;

            var pos = AxialToWorld(q, r);
            var go = Instantiate(prefab, pos, prefab.transform.rotation, stagesParent);
            go.name = $"{stage.stageId} ({q},{r})";
            spawned.Add(go);

            ApplyStateTint(go, unlocked, completed);

            var view = go.GetComponent<StageNodeView>();
            if (view != null)
            {
                int stageScore = stage.state?.progress?.score ?? 0;
                float developedPercent = stage.state?.progress?.developedPercent ?? 0f;
                int coinsAwarded = stage.meta?.coinsAwarded ?? 0;
                view.Init(stage.stageId, unlocked, completed, stage.meta.resourceType, stageScore, developedPercent, coinsAwarded);
            }
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

    private void ApplyStateTint(GameObject go, bool unlocked, bool completed)
    {
        if (unlocked && !completed) return;

        var renderers = go.GetComponentsInChildren<Renderer>();
        foreach (var r in renderers)
        {
            foreach (var mat in r.materials)
            {
                if (mat.shader.name.Contains("TextMeshPro")) continue;

                if (!unlocked)
                {
                    mat.color *= new Color(lockedDarkness, lockedDarkness, lockedDarkness, 1f);
                }
                else if (completed)
                {
                    mat.EnableKeyword("_EMISSION");
                    if (mat.HasProperty("_EmissionColor"))
                        mat.SetColor("_EmissionColor", new Color(0.4f, 0.4f, 0.15f));
                }
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
