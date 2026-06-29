using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class GameBootstrap : MonoBehaviour
{
    [FormerlySerializedAs("templateService")] [SerializeField] private HexTileTemplateService _templateService;
    [FormerlySerializedAs("tileFactory")]     [SerializeField] private TileFactory _tileFactory;
    [FormerlySerializedAs("handController")]  [SerializeField] private HandController _handController;
    [FormerlySerializedAs("mapController")]   [SerializeField] private MapController _mapController;
    [FormerlySerializedAs("gameConfig")]      [SerializeField] private GameConfig _gameConfig;
    [FormerlySerializedAs("gameOverPanel")]   [SerializeField] private GameObject _gameOverPanel;
    [FormerlySerializedAs("stageCompletePanel")] [SerializeField] private StageCompletePanel _stageCompletePanel;
    [FormerlySerializedAs("scoreDisplay")]    [SerializeField] private ScoreDisplay _scoreDisplay;
    [SerializeField] private BoosterController _boosterController;

    private bool _isInitialized = false;
    private bool _stageCompleted = false;
    private System.Action<PlanetStageStateDto> _currentStateLoadHandler;

    private void Awake()
    {
        if (_gameOverPanel != null)
            _gameOverPanel.SetActive(false);
    }

    private IEnumerator Start()
    {
        if (!ValidateDependencies()) yield break;

        yield return LoadTemplates();
        if (!_templateService.IsReady)
        {
            //Debug.LogError("[GameBootstrap] Templates not ready after loading");
            SceneLoader.Instance?.GoBack();
            yield break;
        }

        _tileFactory.TemplateService = _templateService;

        if (GameManager.Instance == null)
        {
            //Debug.LogError("[GameBootstrap] GameManager.Instance is null!");
            SceneLoader.Instance?.GoBack();
            yield break;
        }

        var cachedState = GameManager.Instance.GetCachedPlanetStageState();

        if (cachedState != null && cachedState.targetScore > 0)
        {
            InitializeGameplay(cachedState);
            NotifyReadyIfMatch();
            yield break;
        }

        bool stateLoaded = false;
        PlanetStageStateDto loadedState = null;

        _currentStateLoadHandler = (state) =>
        {
            loadedState = state;
            stateLoaded = true;
        };

        GameManager.Instance.OnPlanetStageStateLoaded += _currentStateLoadHandler;

        string loadError = null;
        System.Action<string> errorHandler = (err) => { loadError = err; stateLoaded = true; };
        System.Action unauthorizedHandler = () => { loadError = "Unauthorized"; stateLoaded = true; };
        GameManager.Instance.OnError += errorHandler;
        GameManager.Instance.OnUnauthorized += unauthorizedHandler;

        bool needsFresh = cachedState != null && cachedState.targetScore == 0;
        //Debug.Log($"[GameBootstrap] Requesting stage state (forceRefresh={needsFresh})...");
        GameManager.Instance.RequestPlanetStageState(forceRefresh: needsFresh);

        float timeout = 180f;
        float elapsed = 0f;

        while (!stateLoaded && elapsed < timeout)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        //Debug.Log($"[GameBootstrap] Wait done — loaded={stateLoaded}, elapsed={elapsed:F1}s, error={loadError ?? "none"}");

        GameManager.Instance.OnPlanetStageStateLoaded -= _currentStateLoadHandler;
        GameManager.Instance.OnError -= errorHandler;
        GameManager.Instance.OnUnauthorized -= unauthorizedHandler;
        _currentStateLoadHandler = null;

        if (!string.IsNullOrEmpty(loadError))
        {
            //Debug.LogError($"[GameBootstrap] GameManager error: {loadError}");
            SceneLoader.Instance?.GoBack();
            yield break;
        }

        if (!stateLoaded || loadedState == null)
        {
            //Debug.LogError("[GameBootstrap] Failed to load stage state — timed out or null");
            GameManager.Instance?.ClearCache();
            SceneLoader.Instance?.GoBack();
            yield break;
        }

        InitializeGameplay(loadedState);
        NotifyReadyIfMatch();
    }

    private bool ValidateDependencies()
    {
        if (_templateService == null) { /*Debug.LogError("[GameBootstrap] templateService is null");*/ return false; }
        if (_tileFactory == null)     { /*Debug.LogError("[GameBootstrap] tileFactory is null");*/ return false; }
        if (_handController == null)  { /*Debug.LogError("[GameBootstrap] handController is null");*/ return false; }
        if (_mapController == null)   { /*Debug.LogError("[GameBootstrap] mapController is null");*/ return false; }

        if (_scoreDisplay == null)
            _scoreDisplay = FindObjectOfType<ScoreDisplay>(true);

        return true;
    }

    private IEnumerator LoadTemplates()
    {
        yield return _templateService.LoadTemplates();
    }

    private void InitializeGameplay(PlanetStageStateDto state)
    {
        if (_isInitialized)
        {
            //Debug.LogWarning("[GameBootstrap] Already initialized, skipping");
            return;
        }
        //Debug.Log($"[GameBootstrap] InitializeGameplay — hand={state?.hand?.tilesInHand?.Count ?? 0}, deck={state?.deck?.remainingTiles?.Count ?? 0}, map={state?.map?.placedTiles?.Count ?? 0}");
        if (_gameConfig == null)
            _gameConfig = Resources.Load<GameConfig>("GameConfig");

        _handController.Initialize(_tileFactory, _mapController);
        _mapController.SetTileFactory(_tileFactory);

        if (_scoreDisplay != null)
            _scoreDisplay.SetTargetScore(state.targetScore);

        _mapController.OnStageCompleted += HandleStageCompleted;
        _mapController.OnStageCompletedWithCoins += HandleStageCompletedWithCoins;
        _mapController.OnAvatarsUnlocked += HandleAvatarsUnlocked;
        _handController.OnHandAndDeckEmpty += HandleGameOver;

        _mapController.ApplyServerState(state);
        _handController.LoadFromServer(state.hand, state.deck);

        if (_boosterController == null)
            _boosterController = FindObjectOfType<BoosterController>(true);
        _boosterController?.Initialize(_mapController, _handController);

        _isInitialized = true;
    }

    private void NotifyReadyIfMatch()
    {
        if (MatchSession.Instance != null && MatchSession.Instance.IsActive)
            MatchManager.Instance?.NotifyStageReady();
    }

    private void HandleStageCompletedWithCoins(int coins)
    {
        if (MatchSession.Instance != null && MatchSession.Instance.IsActive)
            return;

        if (_stageCompletePanel != null)
            _stageCompletePanel.Show(coins);
    }

    private void HandleGameOver()
    {
        if (_stageCompleted) return;

        if (MatchSession.Instance != null && MatchSession.Instance.IsActive)
        {
            // In multiplayer, deck empty = submit score and end match
            MatchManager.Instance?.SubmitFinalScore();
            return;
        }

        if (_gameOverPanel != null)
        {
            _gameOverPanel.SetActive(true);
            PopupManager.OnPopupOpened();
        }

        // Reset server state so re-entering the stage without explicit restart still gets a fresh map.
        GameManager.Instance?.ResetCurrentStage();
    }

    private void HandleStageCompleted()
    {
        _stageCompleted = true;
        if (GameManager.Instance != null)
            GameManager.Instance.ClearCache();
    }

    private void HandleAvatarsUnlocked(List<string> avatarIds)
    {
        if (avatarIds == null || avatarIds.Count == 0) return;
        var notifications = new List<UnlockedAchievementDto>();
        foreach (var id in avatarIds)
            notifications.Add(new UnlockedAchievementDto { id = id, title = "New Avatar Unlocked!", rewardType = "avatar", rewardAmount = 0 });
        AchievementNotifier.Notify(notifications);
    }

    private void OnDestroy()
    {
        if (_currentStateLoadHandler != null && GameManager.Instance != null)
        {
            GameManager.Instance.OnPlanetStageStateLoaded -= _currentStateLoadHandler;
            _currentStateLoadHandler = null;
        }

        if (_mapController != null)
        {
            _mapController.OnStageCompleted -= HandleStageCompleted;
            _mapController.OnStageCompletedWithCoins -= HandleStageCompletedWithCoins;
            _mapController.OnAvatarsUnlocked -= HandleAvatarsUnlocked;
        }

        if (_handController != null)
            _handController.OnHandAndDeckEmpty -= HandleGameOver;
    }
}
