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

        if (planetApi == null)
            Debug.LogWarning("[GameManager] PlanetApiClient not found at startup - will retry later");
        if (planetStateApi == null)
            Debug.LogWarning("[GameManager] PlanetStateApiClient not found at startup - will retry later");
    }

    private void EnsureApiRefs()
    {
        // Only retry if not cached yet
        if (planetApi == null || planetStateApi == null)
        {
            CacheApiReferences();
        }

        if (planetApi == null)
            Debug.LogError("[GameManager] PlanetApiClient not found/assigned.");
        if (planetStateApi == null)
            Debug.LogError("[GameManager] PlanetStateApiClient not found/assigned.");
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
            OnUnauthorized?.Invoke();
            return;
        }

        var planetId = AppSession.Instance?.ActivePlanet?.planetId;
        var stageId = AppSession.Instance?.SelectedStageId;

        if (string.IsNullOrEmpty(planetId))
        {
            OnError?.Invoke("No active planet. Call RequestActivePlanet() first.");
            return;
        }

        if (string.IsNullOrEmpty(stageId))
        {
            OnError?.Invoke("No selected stage. Call SelectStage(stageId) first.");
            return;
        }

        // Use cache if available and not forcing refresh
        if (!forceRefresh && currentPlanetStageState != null && currentStageId == stageId)
        {
            Debug.Log("[GameManager] Using cached PlanetStageState");
            OnPlanetStageStateLoaded?.Invoke(currentPlanetStageState);
            return;
        }

        if (isLoadingPlanetStageState)
        {
            Debug.LogWarning("[GameManager] Already loading PlanetStageState");
            return;
        }

        EnsureApiRefs();
        StartCoroutine(LoadPlanetStageStateRoutine(planetId, stageId));
    }

    private IEnumerator LoadPlanetStageStateRoutine(string planetId, string stageId)
    {
        isLoadingPlanetStageState = true;
        string errMsg = null;

        Debug.Log($"[GameManager] Loading PlanetStageState for planet={planetId}, stage={stageId}");

        yield return StartCoroutine(planetStateApi.GetPlanetStageState(
            planetId,
            stageId,
            defaultDeckSize,
            AppSession.Instance.AccessToken,
            onSuccess: (state) =>
            {
                currentPlanetStageState = state;
                currentStageId = stageId;
                Debug.Log("[GameManager] PlanetStageState loaded successfully");
                OnPlanetStageStateLoaded?.Invoke(state);
            },
            onError: (err) => errMsg = err
        ));

        isLoadingPlanetStageState = false;

        if (!string.IsNullOrEmpty(errMsg))
            HandleError(errMsg);
    }

    // ---------- Save Stage State ----------
    /// <summary>
    /// Saves the current stage state to the server
    /// </summary>
    public void SavePlanetStageState(SaveStageStateRequestDto stateDto, Action onSuccess = null, Action<string> onError = null)
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
            var error = "Cannot save: missing planetId or stageId";
            OnError?.Invoke(error);
            onError?.Invoke(error);
            return;
        }

        EnsureApiRefs();
        StartCoroutine(SavePlanetStageStateRoutine(planetId, stageId, stateDto, onSuccess, onError));
    }

    private IEnumerator SavePlanetStageStateRoutine(
        string planetId,
        string stageId,
        SaveStageStateRequestDto stateDto,
        Action onSuccess,
        Action<string> onError)
    {
        string errMsg = null;

        yield return StartCoroutine(planetStateApi.SavePlanetStageState(
            planetId,
            stageId,
            AppSession.Instance.AccessToken,
            stateDto,
            onSuccess: () =>
            {
                Debug.Log("[GameManager] Stage state saved successfully");
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
                Debug.Log("[GameManager] Stage reset successfully");
                // Cache already cleared before the async call started
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
        currentStageId = null;
        currentPlanetStageState = null;
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

        bool refreshDone = false;
        bool refreshSuccess = false;
        string newAccessToken = null;

        yield return authApi.Refresh(
            AppSession.Instance.RefreshToken,
            onSuccess: (token) =>
            {
                refreshDone = true;
                refreshSuccess = true;
                newAccessToken = token;
            },
            onError: (err) =>
            {
                refreshDone = true;
                refreshSuccess = false;
                Debug.LogError($"[GameManager] Token refresh failed: {err}");
            }
        );

        // Cleanup temp GameObject
        Destroy(authApiGO);

        // Wait for refresh to complete
        float timeout = 5f;
        float elapsed = 0f;
        while (!refreshDone && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        isRefreshingToken = false;

        if (refreshSuccess && !string.IsNullOrEmpty(newAccessToken))
        {
            Debug.Log("[GameManager] Token refreshed successfully");
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
