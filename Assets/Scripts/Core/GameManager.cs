using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Central game state manager - handles all API interactions and state management
/// Singleton that persists across scenes
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("API Clients (assign or auto-find)")]
    [SerializeField] private PlanetApiClient planetApi;
    [SerializeField] private PlanetStateApiClient planetStateApi;
    [SerializeField] private BoosterApiClient boosterApi;

    [Header("Settings")]
    [SerializeField] private int defaultDeckSize = 30;

    // Token refresh state
    private bool isRefreshingToken = false;
    private System.Collections.Generic.Queue<System.Action> pendingRequestsAfterRefresh = new System.Collections.Generic.Queue<System.Action>();

    // Events
    public event Action<PlanetDto> OnPlanetLoaded;
    public event Action<PlanetStageStateDto> OnPlanetStageStateLoaded;
    public event Action<string> OnError;
    public event Action OnUnauthorized;

    // Loading flags
    private bool isLoadingPlanet;
    private bool isLoadingPlanetStageState;
    private Coroutine _loadStageStateCoroutine;

    // Cache for current stage state
    private PlanetStageStateDto currentPlanetStageState;
    private string currentStageId;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Cache API client references on startup
        CacheApiReferences();
    }

    private void CacheApiReferences()
    {
        if (planetApi == null)
            planetApi = FindObjectOfType<PlanetApiClient>(true);
        if (planetStateApi == null)
            planetStateApi = FindObjectOfType<PlanetStateApiClient>(true);
        if (boosterApi == null)
            boosterApi = FindObjectOfType<BoosterApiClient>(true);

        if (planetApi == null)
            Debug.LogWarning("[GameManager] PlanetApiClient not found at startup - will retry later");
        if (planetStateApi == null)
            Debug.LogWarning("[GameManager] PlanetStateApiClient not found at startup - will retry later");
        if (boosterApi == null)
            Debug.LogWarning("[GameManager] BoosterApiClient not found at startup - will retry later");
    }

    private void EnsureApiRefs()
    {
        if (planetApi == null || planetStateApi == null || boosterApi == null)
            CacheApiReferences();

        if (planetApi == null)
            Debug.LogError("[GameManager] PlanetApiClient not found/assigned.");
        if (planetStateApi == null)
            Debug.LogError("[GameManager] PlanetStateApiClient not found/assigned.");
        if (boosterApi == null)
            Debug.LogError("[GameManager] BoosterApiClient not found/assigned.");
    }

    public BoosterApiClient BoosterApi
    {
        get
        {
            if (boosterApi == null) CacheApiReferences();
            return boosterApi;
        }
    }

    public bool HasAccess() => AppSession.Instance != null && AppSession.Instance.HasAccess();

    // ---------- Planet ----------
    public void RequestActivePlanet(bool forceRefresh = false)
    {
        if (!HasAccess())
        {
            OnUnauthorized?.Invoke();
            return;
        }

        if (!forceRefresh && AppSession.Instance.ActivePlanet != null)
        {
            OnPlanetLoaded?.Invoke(AppSession.Instance.ActivePlanet);
            return;
        }

        if (isLoadingPlanet) return;

        EnsureApiRefs();
        StartCoroutine(LoadActivePlanetRoutine());
    }

    private IEnumerator LoadActivePlanetRoutine()
    {
        isLoadingPlanet = true;
        string errMsg = null;

        yield return StartCoroutine(planetApi.GetActivePlanet(
            AppSession.Instance.AccessToken,
            onSuccess: (planet) =>
            {
                AppSession.Instance.ActivePlanet = planet;
                UserCoinsDisplay.UpdateCoins(planet.totalCoins);
                OnPlanetLoaded?.Invoke(planet);
            },
            onError: (err) => errMsg = err
        ));

        isLoadingPlanet = false;

        if (!string.IsNullOrEmpty(errMsg))
            HandleError(errMsg);
    }

    // ---------- Stage selection ----------
    public void SelectStage(string stageId)
    {
        if (AppSession.Instance == null) return;
        AppSession.Instance.SetSelectedStage(stageId);

        // Clear cache when selecting a new stage
        currentStageId = null;
        currentPlanetStageState = null;
    }

    // ---------- Planet Stage State (main gameplay state) ----------
    /// <summary>
    /// Requests the full stage state including map, hand, deck for the selected stage
    /// This is the primary method for loading gameplay state
    /// </summary>
    public void RequestPlanetStageState(bool forceRefresh = false)
    {
        if (!HasAccess())
        {
            Debug.LogError("[GameManager] RequestPlanetStageState: No access token!");
            OnUnauthorized?.Invoke();
            return;
        }

        var planetId = AppSession.Instance?.ActivePlanet?.planetId;
        var stageId = AppSession.Instance?.SelectedStageId;

        if (string.IsNullOrEmpty(planetId))
        {
            Debug.LogError("[GameManager] RequestPlanetStageState: No active planet!");
            OnError?.Invoke("No active planet. Call RequestActivePlanet() first.");
            return;
        }

        if (string.IsNullOrEmpty(stageId))
        {
            Debug.LogError("[GameManager] RequestPlanetStageState: No selected stage!");
            OnError?.Invoke("No selected stage. Call SelectStage(stageId) first.");
            return;
        }

        // Use cache if available and not forcing refresh
        if (!forceRefresh && currentPlanetStageState != null && currentStageId == stageId)
        {
            OnPlanetStageStateLoaded?.Invoke(currentPlanetStageState);
            return;
        }

        if (isLoadingPlanetStageState)
        {
            Debug.Log("[GameManager] Already loading PlanetStageState - SKIPPING");
            return;
        }

        EnsureApiRefs();
        _loadStageStateCoroutine = StartCoroutine(LoadPlanetStageStateRoutine(planetId, stageId));
    }

    private IEnumerator LoadPlanetStageStateRoutine(string planetId, string stageId)
    {
        isLoadingPlanetStageState = true;
        string errMsg = null;

        var api = planetStateApi ?? PlanetStateApiClient.Instance;
        if (api == null)
        {
            Debug.LogError("[GameManager] planetStateApi is null, cannot load stage state");
            isLoadingPlanetStageState = false;
            OnError?.Invoke("PlanetStateApiClient not available");
            yield break;
        }

        yield return StartCoroutine(api.GetPlanetStageState(
            planetId,
            stageId,
            defaultDeckSize,
            AppSession.Instance.AccessToken,
            onSuccess: (state) =>
            {
                currentPlanetStageState = state;
                currentStageId = stageId;
                OnPlanetStageStateLoaded?.Invoke(state);
            },
            onError: (err) => errMsg = err
        ));

        isLoadingPlanetStageState = false;
        _loadStageStateCoroutine = null;

        if (!string.IsNullOrEmpty(errMsg))
        {
            if ((errMsg.Contains("401") || errMsg.Contains("Unauthorized")) &&
                AppSession.Instance != null && AppSession.Instance.HasRefresh() && !isRefreshingToken)
            {
                pendingRequestsAfterRefresh.Enqueue(() => RequestPlanetStageState(forceRefresh: true));
            }
            HandleError(errMsg);
        }
    }

    // ---------- Single-player scene entry with loading hold ----------
    public void LoadStageSceneWithHold(int sceneIndex)
    {
        StartCoroutine(LoadStageSceneWithHoldRoutine(sceneIndex));
    }

    private IEnumerator LoadStageSceneWithHoldRoutine(int sceneIndex)
    {
        SceneLoader.HoldActivation = true;
        SceneLoader.Instance.LoadScene(sceneIndex);

        bool done = false;
        Action<PlanetStageStateDto> onLoaded = _ => done = true;
        Action<string> onError = _ => { done = true; SceneLoader.HoldActivation = false; };
        Action onUnauth = () => { done = true; SceneLoader.HoldActivation = false; };

        OnPlanetStageStateLoaded += onLoaded;
        OnError += onError;
        OnUnauthorized += onUnauth;

        RequestPlanetStageState(forceRefresh: true);

        float elapsed = 0f;
        while (!done && elapsed < 30f) { elapsed += Time.unscaledDeltaTime; yield return null; }

        OnPlanetStageStateLoaded -= onLoaded;
        OnError -= onError;
        OnUnauthorized -= onUnauth;

        SceneLoader.HoldActivation = false;
    }

    // ---------- Reset Stage ----------
    /// <summary>
    /// Resets the current stage to initial state
    /// Clears cache immediately to prevent race conditions
    /// </summary>
    public void ResetCurrentStage(Action onSuccess = null, Action<string> onError = null)
    {
        if (!HasAccess())
        {
            OnUnauthorized?.Invoke();
            onError?.Invoke("Unauthorized");
            return;
        }

        var planetId = AppSession.Instance?.ActivePlanet?.planetId;
        var stageId = AppSession.Instance?.SelectedStageId;

        if (string.IsNullOrEmpty(planetId) || string.IsNullOrEmpty(stageId))
        {
            var error = "Cannot reset: missing planetId or stageId";
            OnError?.Invoke(error);
            onError?.Invoke(error);
            return;
        }

        // Clear cache BEFORE starting the reset to prevent race conditions
        ClearCache();

        EnsureApiRefs();
        StartCoroutine(ResetStageRoutine(planetId, stageId, onSuccess, onError));
    }

    private IEnumerator ResetStageRoutine(
        string planetId,
        string stageId,
        Action onSuccess,
        Action<string> onError)
    {
        string errMsg = null;

        yield return StartCoroutine(planetStateApi.ResetStage(
            planetId,
            stageId,
            AppSession.Instance.AccessToken,
            onSuccess: () =>
            {
                onSuccess?.Invoke();
            },
            onError: (err) => errMsg = err
        ));

        if (!string.IsNullOrEmpty(errMsg))
        {
            HandleError(errMsg);
            onError?.Invoke(errMsg);
        }
    }

    // ---------- Utility Methods ----------
    /// <summary>
    /// Gets the cached stage state if available
    /// </summary>
    public PlanetStageStateDto GetCachedPlanetStageState()
    {
        return currentPlanetStageState;
    }

    /// <summary>
    /// Clears all cached data
    /// </summary>
    public void ClearCache()
    {
        if (_loadStageStateCoroutine != null)
        {
            StopCoroutine(_loadStageStateCoroutine);
            _loadStageStateCoroutine = null;
        }
        currentStageId = null;
        currentPlanetStageState = null;
        isLoadingPlanetStageState = false;
    }

    public void HandleUnauthorizedAndRetry(System.Action onRefreshed)
    {
        if (AppSession.Instance != null && AppSession.Instance.HasRefresh() && !isRefreshingToken)
        {
            pendingRequestsAfterRefresh.Enqueue(onRefreshed);
            StartCoroutine(RefreshTokenAndRetry());
        }
        else
        {
            AppSession.Instance?.Logout();
            OnUnauthorized?.Invoke();
        }
    }

    private void HandleError(string err)
    {
        Debug.LogError($"[GameManager] Error: {err}");

        // Check if this is a 401 Unauthorized error
        if (err.Contains("401") || err.Contains("Unauthorized"))
        {
            // Try to refresh the token before logging out
            if (AppSession.Instance != null && AppSession.Instance.HasRefresh() && !isRefreshingToken)
            {
                Debug.Log("[GameManager] 401 detected, attempting token refresh...");
                StartCoroutine(RefreshTokenAndRetry());
                return;
            }

            // If no refresh token or already tried, logout
            Debug.LogWarning("[GameManager] No refresh token available or refresh failed, logging out");
            AppSession.Instance?.Logout();
            OnUnauthorized?.Invoke();
            return;
        }

        OnError?.Invoke(err);
    }

    private IEnumerator RefreshTokenAndRetry()
    {
        if (isRefreshingToken)
        {
            Debug.LogWarning("[GameManager] Token refresh already in progress");
            yield break;
        }

        isRefreshingToken = true;

        // Find AuthApiClient (it's local to auth scenes, so we need to create one temporarily)
        var authApiGO = new GameObject("TempAuthApiClient");
        var authApi = authApiGO.AddComponent<AuthApiClient>();

        // Load GameConfig for AuthApiClient
        var gameConfig = Resources.Load<GameConfig>("GameConfig");
        if (gameConfig != null)
        {
            // Assign via reflection since it's a SerializeField
            var field = typeof(AuthApiClient).BaseType.GetField("gameConfig",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(authApi, gameConfig);
        }

        bool refreshSuccess = false;
        string newAccessToken = null;

        yield return authApi.Refresh(
            AppSession.Instance.RefreshToken,
            onSuccess: (token) =>
            {
                refreshSuccess = true;
                newAccessToken = token;
            },
            onError: (err) =>
            {
                refreshSuccess = false;
                Debug.LogError($"[GameManager] Token refresh failed: {err}");
            }
        );

        Destroy(authApiGO);

        isRefreshingToken = false;

        if (refreshSuccess && !string.IsNullOrEmpty(newAccessToken))
        {
            AppSession.Instance.SetAccess(newAccessToken);

            // Process any pending requests
            while (pendingRequestsAfterRefresh.Count > 0)
            {
                var pendingRequest = pendingRequestsAfterRefresh.Dequeue();
                pendingRequest?.Invoke();
            }
        }
        else
        {
            Debug.LogError("[GameManager] Token refresh failed, logging out");
            AppSession.Instance?.Logout();
            OnUnauthorized?.Invoke();
        }
    }
}
