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
    [SerializeField] private GameConfig gameConfig;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private StageCompletePanel stageCompletePanel;
    [SerializeField] private ScoreDisplay scoreDisplay;

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

        if (cachedState != null && cachedState.targetScore > 0)
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

        string loadError = null;
        System.Action<string> errorHandler = (err) => { loadError = err; stateLoaded = true; };
        GameManager.Instance.OnError += errorHandler;

        // Request the state — force refresh if cache had no targetScore
        bool needsFresh = cachedState != null && cachedState.targetScore == 0;
        Debug.Log($"[GameBootstrap] Requesting state. planetId={AppSession.Instance?.ActivePlanet?.planetId}, stageId={AppSession.Instance?.SelectedStageId}");
        GameManager.Instance.RequestPlanetStageState(forceRefresh: needsFresh);

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
        GameManager.Instance.OnError -= errorHandler;
        currentStateLoadHandler = null;

        if (!string.IsNullOrEmpty(loadError))
        {
            Debug.LogError($"[GameBootstrap] GameManager error: {loadError}");
            yield break;
        }

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

        if (scoreDisplay == null)
            scoreDisplay = FindObjectOfType<ScoreDisplay>(true);

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

        // Ensure gameConfig is available
        if (gameConfig == null)
            gameConfig = Resources.Load<GameConfig>("GameConfig");

        // Connect controllers
        handController.factory = tileFactory;
        handController.mapController = mapController;
        mapController.tileFactory = tileFactory;

        // Load initial state — subscribe to OnStageCompleted AFTER this call so we
        // don't navigate away if the stage was already completed before this session.
        if (scoreDisplay != null)
            scoreDisplay.SetTargetScore(state.targetScore);

        mapController.ApplyServerState(state);
        handController.LoadFromServer(state.hand, state.deck);

        // Subscribe to future completions (not the initial load)
        mapController.OnStageCompleted += HandleStageCompleted;
        mapController.OnStageCompletedWithCoins += HandleStageCompletedWithCoins;
        handController.OnHandAndDeckEmpty += HandleGameOver;

        isInitialized = true;
        Debug.Log("[GameBootstrap] Gameplay initialized successfully");
    }

    private void HandleStageCompletedWithCoins(int coins)
    {
        if (stageCompletePanel != null)
            stageCompletePanel.Show(coins);
    }

    private void HandleGameOver()
    {
        Debug.Log("[GameBootstrap] Game Over — hand and deck empty.");
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            PopupManager.OnPopupOpened();
        }
    }

    private void HandleStageCompleted()
    {
        Debug.Log("[GameBootstrap] Stage completed! Showing complete panel.");

        if (GameManager.Instance != null)
            GameManager.Instance.ClearCache();
    }

    private void OnDestroy()
    {
        // Cleanup: Unsubscribe from events to prevent memory leaks
        if (currentStateLoadHandler != null && GameManager.Instance != null)
        {
            GameManager.Instance.OnPlanetStageStateLoaded -= currentStateLoadHandler;
            currentStateLoadHandler = null;
        }

        if (mapController != null)
        {
            mapController.OnStageCompleted -= HandleStageCompleted;
            mapController.OnStageCompletedWithCoins -= HandleStageCompletedWithCoins;
        }

        if (handController != null)
            handController.OnHandAndDeckEmpty -= HandleGameOver;
    }
}
