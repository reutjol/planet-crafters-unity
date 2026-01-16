using System;
using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("API refs (assign or auto-find)")]
    [SerializeField] private PlanetApiClient planetApi;
    [SerializeField] private StageStateApiClient stageStateApi;

    public event Action<PlanetDto> OnPlanetLoaded;
    public event Action<StageStateDto> OnStageStateLoaded;
    public event Action<string> OnError;           
    public event Action OnUnauthorized;            

    private bool isLoadingPlanet;
    private bool isLoadingStageState;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void EnsureApiRefs()
    {
        if (planetApi == null) planetApi = FindObjectOfType<PlanetApiClient>(true);
        if (stageStateApi == null) stageStateApi = FindObjectOfType<StageStateApiClient>(true);

        if (planetApi == null) Debug.LogError("PlanetApiClient not found/assigned.");
        if (stageStateApi == null) Debug.LogError("StageStateApiClient not found/assigned.");
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
            onOk: (planet) =>
            {
                AppSession.Instance.ActivePlanet = planet;
                OnPlanetLoaded?.Invoke(planet);
            },
            onErr: (err) => errMsg = err
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
        AppSession.Instance.SelectedStageState = null;
    }

    // ---------- Stage state ----------
    public void RequestSelectedStageState(bool forceRefresh = false)
    {
        if (!HasAccess())
        {
            OnUnauthorized?.Invoke();
            return;
        }

        var stageId = AppSession.Instance.SelectedStageId;
        if (string.IsNullOrEmpty(stageId))
        {
            OnError?.Invoke("No SelectedStageId. Call SelectStage(stageId) first.");
            return;
        }

        if (!forceRefresh && AppSession.Instance.SelectedStageState != null)
        {
            OnStageStateLoaded?.Invoke(AppSession.Instance.SelectedStageState);
            return;
        }

        if (isLoadingStageState) return;

        EnsureApiRefs();
        StartCoroutine(LoadStageStateRoutine(stageId));
    }

    private IEnumerator LoadStageStateRoutine(string stageId)
    {
        isLoadingStageState = true;
        string errMsg = null;

        yield return StartCoroutine(stageStateApi.LoadState(
            stageId,
            AppSession.Instance.AccessToken,
            onOk: (state) =>
            {
                AppSession.Instance.SelectedStageState = state;
                OnStageStateLoaded?.Invoke(state);
            },
            onErr: (err) => errMsg = err
        ));

        isLoadingStageState = false;

        if (!string.IsNullOrEmpty(errMsg))
            HandleError(errMsg);
    }

    private void HandleError(string err)
    {
        Debug.LogError(err);

        if (err.Contains("401"))
        {
            AppSession.Instance.Logout();
            OnUnauthorized?.Invoke(); 
            return;
        }

        OnError?.Invoke(err);
    }
}
