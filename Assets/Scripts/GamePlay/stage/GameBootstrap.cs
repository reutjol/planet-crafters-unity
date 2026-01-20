using System.Collections;
using UnityEngine;

/// <summary>
/// Initializes the gameplay scene by loading templates and stage state from GameManager
/// This component acts as a bridge between GameManager and the gameplay systems
/// </summary>
public class GameBootstrap : MonoBehaviour
{
    [SerializeField] private HexTileTemplateService templateService;
    [SerializeField] private TileFactory tileFactory;
    [SerializeField] private HandController handController;
    [SerializeField] private MapController mapController;

    private bool isInitialized = false;
    private System.Action<PlanetStageStateDto> currentStateLoadHandler;

    private IEnumerator Start()
    {
        // 0) Validate dependencies
        if (!ValidateDependencies()) yield break;

        // 1) Load templates
        yield return LoadTemplates();
        if (!templateService.IsReady)
        {
            Debug.LogError("[GameBootstrap] Templates not ready after loading");
            yield break;
        }

        // Connect TileFactory to TemplateService
        tileFactory.templateService = templateService;

        // 2) Check GameManager
        if (GameManager.Instance == null)
        {
            Debug.LogError("[GameBootstrap] GameManager.Instance is null!");
            yield break;
        }

        // 3) Try to get cached state first
        var cachedState = GameManager.Instance.GetCachedPlanetStageState();

        if (cachedState != null)
        {
            Debug.Log("[GameBootstrap] Using cached state from GameManager");
            InitializeGameplay(cachedState);
            yield break;
        }

        // 4) If no cache, request state from GameManager
        Debug.Log("[GameBootstrap] No cached state, requesting from GameManager");

        bool stateLoaded = false;
        PlanetStageStateDto loadedState = null;

        // Subscribe to GameManager event
        currentStateLoadHandler = (state) =>
        {
            loadedState = state;
            stateLoaded = true;
        };

        GameManager.Instance.OnPlanetStageStateLoaded += currentStateLoadHandler;

        // Request the state
        GameManager.Instance.RequestPlanetStageState(forceRefresh: false);

        // Wait for state to load (with timeout)
        float timeout = 10f;
        float elapsed = 0f;

        while (!stateLoaded && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Unsubscribe
        GameManager.Instance.OnPlanetStageStateLoaded -= currentStateLoadHandler;
        currentStateLoadHandler = null;

        if (!stateLoaded || loadedState == null)
        {
            Debug.LogError("[GameBootstrap] Failed to load stage state from GameManager");
            yield break;
        }

        // 5) Initialize gameplay with loaded state
        InitializeGameplay(loadedState);
    }

    private bool ValidateDependencies()
    {
        if (templateService == null)
        {
            Debug.LogError("[GameBootstrap] templateService is null");
            return false;
        }
        if (tileFactory == null)
        {
            Debug.LogError("[GameBootstrap] tileFactory is null");
            return false;
        }
        if (handController == null)
        {
            Debug.LogError("[GameBootstrap] handController is null");
            return false;
        }
        if (mapController == null)
        {
            Debug.LogError("[GameBootstrap] mapController is null");
            return false;
        }

        return true;
    }

    private IEnumerator LoadTemplates()
    {
        Debug.Log("[GameBootstrap] Loading tile templates...");
        yield return templateService.LoadTemplates();
    }

    private void InitializeGameplay(PlanetStageStateDto state)
    {
        if (isInitialized)
        {
            Debug.LogWarning("[GameBootstrap] Already initialized, skipping");
            return;
        }

        Debug.Log("[GameBootstrap] Initializing gameplay with state");

        // Validate state parts
        if (state.map?.placedTiles == null)
            Debug.LogWarning("[GameBootstrap] state.map.placedTiles is null");
        if (state.hand == null)
            Debug.LogWarning("[GameBootstrap] state.hand is null");
        if (state.deck == null)
            Debug.LogWarning("[GameBootstrap] state.deck is null");

        // Connect controllers
        handController.factory = tileFactory;
        handController.mapController = mapController;

        // Load state into gameplay systems
        mapController.LoadPlacedTilesFromServer(state.map?.placedTiles, tileFactory);
        handController.LoadFromServer(state.hand, state.deck);

        // Enable auto-save
        var autoSave = FindObjectOfType<StageStateAutoSave>();
        if (autoSave != null)
        {
            autoSave.SetReady(true);
            Debug.Log("[GameBootstrap] Auto-save enabled");
        }

        isInitialized = true;
        Debug.Log("[GameBootstrap] Gameplay initialized successfully");
    }

    private void OnDestroy()
    {
        // Cleanup: Unsubscribe from events to prevent memory leaks
        if (currentStateLoadHandler != null && GameManager.Instance != null)
        {
            GameManager.Instance.OnPlanetStageStateLoaded -= currentStateLoadHandler;
            currentStateLoadHandler = null;
        }
    }
}
