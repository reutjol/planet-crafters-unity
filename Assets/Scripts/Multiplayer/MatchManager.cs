using System;
using System.Collections;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class MatchManager : MonoBehaviour
{
    public static MatchManager Instance { get; private set; }

    public event Action<MatchDto> OnMatchUpdated;
    public event Action<MatchDto> OnMatchFinished;
    public event Action<int> OnOpponentScoreUpdated;
    public event Action<string> OnAiReaction;
    public event Action<System.Collections.Generic.List<LobbyPlayerDto>> OnLobbyUpdated;
    public event Action<string> OnChallengeError;

    private const float LobbyPollInterval = 3f;
    private const float GameplayPollInterval = 5f;
    private const int DefaultDuration = 180;

    private Coroutine _pollCoroutine;
    private Coroutine _loadingCoroutine;
    private bool _matchLoadingActive;
    private bool _finishSubmitted;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ── Lobby: Create a room ──

    public void CreateRoom(Action<MatchDto> onSuccess, Action<string> onError)
    {
        var (planetId, stageId) = GetMatchStage();
        if (planetId == null || stageId == null)
        {
            onError?.Invoke("No active planet or stage available");
            return;
        }

        StartCoroutine(CreateRoomRoutine(planetId, stageId, onSuccess, onError));
    }

    private IEnumerator CreateRoomRoutine(string planetId, string stageId,
        Action<MatchDto> onSuccess, Action<string> onError)
    {
        var token = AppSession.Instance?.AccessToken;
        MatchDto result = null;
        string err = null;

        yield return MatchApiClient.Instance.CreateMatch(
            planetId, stageId, DefaultDuration, token,
            m => result = m,
            e => err = e);

        if (!string.IsNullOrEmpty(err)) { onError?.Invoke(err); yield break; }

        onSuccess?.Invoke(result);
        StartLobbyPolling(result.matchId);
    }

    // ── Lobby: Join a room by code ──

    public void JoinRoom(string code, Action<MatchDto> onSuccess, Action<string> onError)
    {
        var (planetId, stageId) = GetMatchStage();
        if (planetId == null || stageId == null)
        {
            onError?.Invoke("No active planet or stage available");
            return;
        }

        StartCoroutine(JoinRoomRoutine(code, planetId, stageId, onSuccess, onError));
    }

    private IEnumerator JoinRoomRoutine(string code, string planetId, string stageId,
        Action<MatchDto> onSuccess, Action<string> onError)
    {
        var token = AppSession.Instance?.AccessToken;
        MatchDto result = null;
        string err = null;

        yield return MatchApiClient.Instance.JoinMatch(
            code, planetId, stageId, token,
            m => result = m,
            e => err = e);

        if (!string.IsNullOrEmpty(err)) { onError?.Invoke(err); yield break; }

        onSuccess?.Invoke(result);
        // Status is already 'active' after joining — start gameplay
        EnterMatch(result);
    }

    // ── Polling: wait for opponent in lobby ──

    private void StartLobbyPolling(string matchId)
    {
        StopPolling();
        _pollCoroutine = StartCoroutine(LobbyPollRoutine(matchId));
    }

    private IEnumerator LobbyPollRoutine(string matchId)
    {
        var token = AppSession.Instance?.AccessToken;

        while (true)
        {
            yield return new WaitForSeconds(LobbyPollInterval);

            MatchDto match = null;
            yield return MatchApiClient.Instance.GetMatch(matchId, token,
                m => match = m,
                err => Debug.LogWarning($"[MatchManager] LobbyPoll GetMatch failed: {err}"));

            if (match == null) continue;

            OnMatchUpdated?.Invoke(match);

            if (match.status == "active")
            {
                EnterMatch(match);
                yield break;
            }

            if (match.status == "finished")
                yield break;
        }
    }

    // ── Gameplay: start match session and score polling ──

    public void EnterMatch(MatchDto match)
    {
        StopPolling();
        _finishSubmitted = false;

        MatchSession.Instance.StartMatch(match, match.myUserId ?? "");

        if (!string.IsNullOrEmpty(match.matchStageId))
            AppSession.Instance.SetSelectedStage(match.matchStageId);

        // Subscribe here so opponentLeft is handled even during the loading phase
        if (MatchSocketClient.Instance != null)
            MatchSocketClient.Instance.OnOpponentLeft += HandleOpponentLeft;

        var config = Resources.Load<GameConfig>("GameConfig");
        if (config == null) return;

        _matchLoadingActive = true;
        _loadingCoroutine = StartCoroutine(EnterMatchWithLoadingRoutine(config.gameplaySceneIndex, config.planetSceneIndex));
    }

    private IEnumerator EnterMatchWithLoadingRoutine(int sceneIndex, int fallbackSceneIndex)
    {
        // Hold the loading screen open while we prefetch stage state
        SceneLoader.HoldActivation = true;
        SceneLoader.Instance.LoadScene(sceneIndex);

        bool done = false;
        System.Action<PlanetStageStateDto> onLoaded = _ => done = true;
        System.Action<string> onError = _ => { done = true; SceneLoader.HoldActivation = false; };

        GameManager.Instance.OnPlanetStageStateLoaded += onLoaded;
        GameManager.Instance.OnError += onError;
        GameManager.Instance.RequestPlanetStageState(forceRefresh: true);

        float elapsed = 0f;
        while (!done && elapsed < 30f)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        GameManager.Instance.OnPlanetStageStateLoaded -= onLoaded;
        GameManager.Instance.OnError -= onError;

        // Verify match is still active — opponent may have disconnected during loading
        if (MatchSession.Instance != null && MatchSession.Instance.IsActive)
        {
            var verifyMatchId = MatchSession.Instance.MatchId;
            var verifyToken = AppSession.Instance?.AccessToken;
            MatchDto matchCheck = null;
            yield return MatchApiClient.Instance.GetMatch(verifyMatchId, verifyToken,
                m => matchCheck = m,
                _ => { });

            if (matchCheck != null && matchCheck.status != "active")
            {
                Debug.Log("[MatchManager] Match already finished during loading — aborting");
                DisconnectSocket();
                MatchSession.Instance?.Clear();
                GameManager.Instance?.ClearCache();
                _loadingCoroutine = null;
                _matchLoadingActive = false;
                SceneLoader.Instance?.AbortPendingAndLoad(fallbackSceneIndex);
                yield break;
            }
        }

        _loadingCoroutine = null;
        _matchLoadingActive = false;

        // Release loading screen — gameplay scene will activate
        SceneLoader.HoldActivation = false;
    }

    private void AbortLoading(int sceneIndex)
    {
        if (!_matchLoadingActive) return;
        if (_loadingCoroutine != null)
        {
            StopCoroutine(_loadingCoroutine);
            _loadingCoroutine = null;
        }
        _matchLoadingActive = false;
        GameManager.Instance?.ClearCache();
        SceneLoader.Instance?.AbortPendingAndLoad(sceneIndex);
    }

    public void NotifyStageReady()
    {
        StartCoroutine(NotifyStageReadyRoutine());
    }

    private IEnumerator NotifyStageReadyRoutine()
    {
        var matchId = MatchSession.Instance?.MatchId;
        var token = AppSession.Instance?.AccessToken;
        if (string.IsNullOrEmpty(matchId)) yield break;

        MatchDto result = null;
        yield return MatchApiClient.Instance.SendReady(matchId, token,
            m => result = m,
            err => Debug.LogWarning($"[MatchManager] SendReady failed: {err}"));

        if (result?.startTime != null && MatchSession.Instance != null && MatchSession.Instance.StartTimeMs == 0)
            MatchSession.Instance.SetStartTime(result.startTime.Value);
    }

    public void BeginGameplayPolling(int initialScore)
    {
        MatchSession.Instance.SetInitialScore(initialScore);

        // Connect socket for real-time score sync and AI reactions
        var config = Resources.Load<GameConfig>("GameConfig");
        var serverUrl = config?.serverBaseUrl ?? "http://localhost:3000";
        var matchId = MatchSession.Instance?.MatchId;
        var userId = MatchSession.Instance?.MyUserId;

        if (MatchSocketClient.Instance != null && !string.IsNullOrEmpty(matchId))
        {
            MatchSocketClient.Instance.OnMatchStateReceived += HandleSocketMatchState;
            MatchSocketClient.Instance.OnReactionReceived += HandleSocketReaction;
            MatchSocketClient.Instance.OnOpponentFinished += HandleOpponentFinished;
            // OnOpponentLeft already subscribed in EnterMatch — do not add twice
            MatchSocketClient.Instance.Connect(serverUrl, matchId, userId);
        }

        StopPolling();
        _pollCoroutine = StartCoroutine(GameplayPollRoutine());
    }

    private void HandleSocketMatchState(JArray players)
    {
        if (players == null) return;
        var myId = MatchSession.Instance?.MyUserId;
        foreach (var p in players)
        {
            var uid = p["userId"]?.ToString();
            var score = p["score"]?.Value<int>() ?? 0;
            if (uid != myId)
            {
                OnOpponentScoreUpdated?.Invoke(score);
                break;
            }
        }
    }

    private void HandleSocketReaction(string message)
    {
        if (!string.IsNullOrEmpty(message))
            OnAiReaction?.Invoke(message);
    }

    public void PushScore()
    {
        if (MatchSession.Instance == null || !MatchSession.Instance.IsActive) return;
        var matchId = MatchSession.Instance?.MatchId;
        var userId = MatchSession.Instance?.MyUserId;
        if (string.IsNullOrEmpty(matchId)) return;

        MatchSocketClient.Instance?.EmitScore(matchId, userId, GetCurrentMatchScore());
    }

    private IEnumerator GameplayPollRoutine()
    {
        var token = AppSession.Instance?.AccessToken;
        var matchId = MatchSession.Instance?.MatchId;

        while (MatchSession.Instance != null && MatchSession.Instance.IsActive)
        {
            if (string.IsNullOrEmpty(matchId)) yield break;

            // Poll to sync startTime and detect match end; first iteration runs immediately
            MatchDto match = null;
            yield return MatchApiClient.Instance.GetMatch(matchId, token,
                m => match = m,
                err => Debug.LogWarning($"[MatchManager] GameplayPoll GetMatch failed: {err}"));

            if (match != null)
            {
                if (match.startTime != null && MatchSession.Instance != null && MatchSession.Instance.StartTimeMs == 0)
                    MatchSession.Instance.SetStartTime(match.startTime.Value);

                OnMatchUpdated?.Invoke(match);

                if (match.status == "finished")
                {
                    if (_finishSubmitted) yield break;
                    _finishSubmitted = true;
                    DisconnectSocket();
                    OnMatchFinished?.Invoke(match);
                    MatchSession.Instance.Clear();
                    yield break;
                }
            }

            yield return new WaitForSeconds(GameplayPollInterval);
        }
    }

    public void SubmitFinalScore()
    {
        if (_finishSubmitted) return;
        StartCoroutine(SubmitFinalScoreRoutine());
    }

    private IEnumerator SubmitFinalScoreRoutine()
    {
        if (_finishSubmitted) yield break;
        _finishSubmitted = true;

        StopPolling();

        var matchId = MatchSession.Instance?.MatchId;
        var token = AppSession.Instance?.AccessToken;
        var finalScore = GetCurrentMatchScore();

        MatchDto result = null;
        string finishErr = null;
        yield return MatchApiClient.Instance.FinishMatch(matchId, finalScore, token,
            m => result = m,
            err => { finishErr = err; Debug.LogWarning($"[MatchManager] FinishMatch failed: {err}"); });

        // Disconnect AFTER the API call so the server doesn't treat this as a forfeit
        DisconnectSocket();

        if (result == null)
        {
            Debug.LogWarning("[MatchManager] FinishMatch returned no result — match may not be recorded on server");
            MatchSession.Instance?.Clear();
            yield break;
        }

        // Wait briefly so last-second tile placements from the opponent
        // have time to reach the server before we fetch the final state.
        yield return new WaitForSeconds(1.5f);

        MatchDto finalResult = null;
        yield return MatchApiClient.Instance.GetMatch(matchId, token,
            m => finalResult = m,
            _ => { });

        var finishedMatch = finalResult ?? result;
        AchievementNotifier.Notify(finishedMatch?.achievementRewards);

        OnMatchFinished?.Invoke(finishedMatch);
        MatchSession.Instance?.Clear();
    }

    // ── Lobby ────────────────────────────────────────────────────

    public void OpenLobby()
    {
        var (planetId, stageId) = GetMatchStage();
        Debug.Log($"[MatchManager] OpenLobby: userId={AppSession.Instance?.UserId} username={AppSession.Instance?.Username} planetId={planetId}");

        MatchSocketClient.Instance.OnLobbyUpdated   += OnLobbyPlayersUpdated;
        MatchSocketClient.Instance.OnMatchReady     += OnMatchReadyReceived;
        MatchSocketClient.Instance.OnChallengeError += err => OnChallengeError?.Invoke(err);

        EnsureSocketConnected(() =>
        {
            Debug.Log("[MatchManager] Socket ready — joining lobby");
            MatchSocketClient.Instance.JoinLobby(
                AppSession.Instance?.UserId,
                AppSession.Instance?.Username,
                planetId, stageId);
        });
    }

    public void CloseLobby()
    {
        if (MatchSocketClient.Instance == null) return;
        MatchSocketClient.Instance.OnLobbyUpdated   -= OnLobbyPlayersUpdated;
        MatchSocketClient.Instance.OnMatchReady     -= OnMatchReadyReceived;
        MatchSocketClient.Instance.OnChallengeError -= err => OnChallengeError?.Invoke(err);
        MatchSocketClient.Instance.LeaveLobby();
    }

    public void ChallengePlayer(string targetUserId) =>
        MatchSocketClient.Instance?.ChallengePlayer(targetUserId);

    public void ChallengeRandom() =>
        MatchSocketClient.Instance?.ChallengeRandom();

    public void RejoinLobby()
    {
        var (planetId, stageId) = GetMatchStage();
        EnsureSocketConnected(() =>
        {
            MatchSocketClient.Instance.JoinLobby(
                AppSession.Instance?.UserId,
                AppSession.Instance?.Username,
                planetId, stageId);
        });
    }

    private void OnLobbyPlayersUpdated(System.Collections.Generic.List<LobbyPlayerDto> players) =>
        OnLobbyUpdated?.Invoke(players);

    private void OnMatchReadyReceived(MatchDto match) => EnterMatch(match);

    private void EnsureSocketConnected(System.Action onConnected = null)
    {
        if (MatchSocketClient.Instance == null) return;
        var config = Resources.Load<GameConfig>("GameConfig");
        var serverUrl = config?.serverBaseUrl ?? "http://localhost:3000";
        var userId = AppSession.Instance?.UserId ?? "";
        MatchSocketClient.Instance.Connect(serverUrl, "", userId, onConnected);
    }

    // ── Helpers ──

    public void StopPolling()
    {
        if (_pollCoroutine != null)
        {
            StopCoroutine(_pollCoroutine);
            _pollCoroutine = null;
        }
    }

    public void CancelMatch()
    {
        StopPolling();
        DisconnectSocket();
        MatchSession.Instance?.Clear();
    }

    public void AbandonMatch()
    {
        DisconnectSocket();
        MatchSession.Instance?.Clear();
        var config = Resources.Load<GameConfig>("GameConfig");
        AbortLoading(config?.planetSceneIndex ?? 5);
    }

    private void HandleOpponentFinished()
    {
        Debug.Log("[MatchManager] Opponent finished their deck — submitting final score");
        SubmitFinalScore();
    }

    private void HandleOpponentLeft()
    {
        Debug.Log("[MatchManager] Opponent left");
        if (_matchLoadingActive)
        {
            // Opponent left while we were still loading — abort and go back to planet
            Debug.Log("[MatchManager] Opponent left during loading — aborting load");
            DisconnectSocket();
            MatchSession.Instance?.Clear();
            var config = Resources.Load<GameConfig>("GameConfig");
            AbortLoading(config?.planetSceneIndex ?? 5);
        }
        else
        {
            Debug.Log("[MatchManager] Opponent left during gameplay — submitting final score");
            SubmitFinalScore();
        }
    }

    private void DisconnectSocket()
    {
        if (MatchSocketClient.Instance == null) return;
        MatchSocketClient.Instance.OnMatchStateReceived -= HandleSocketMatchState;
        MatchSocketClient.Instance.OnReactionReceived -= HandleSocketReaction;
        MatchSocketClient.Instance.OnOpponentLeft -= HandleOpponentLeft;
        MatchSocketClient.Instance.OnOpponentFinished -= HandleOpponentFinished;
        MatchSocketClient.Instance.Disconnect();
    }

    private int GetCurrentMatchScore()
    {
        return MatchSession.Instance?.CurrentMatchScore ?? 0;
    }

    private (string planetId, string stageId) GetMatchStage()
    {
        var planet = AppSession.Instance?.ActivePlanet;
        if (planet == null) return (null, null);

        // Use first unlocked stage
        var stage = planet.stages?.Find(s => s.meta?.isUnlocked == true);
        var stageId = stage?.stageId ?? AppSession.Instance?.SelectedStageId;

        if (string.IsNullOrEmpty(stageId)) return (null, null);
        return (planet.planetId, stageId);
    }

}
