using System;
using System.Collections;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class MatchManager : MonoBehaviour
{
    public static MatchManager Instance { get; private set; }

    // Persists across scene loads — shown in the lobby when it next opens
    public static string PendingLobbyError { get; private set; }
    public static void ClearPendingLobbyError() => PendingLobbyError = null;

    public event Action<MatchDto> OnMatchFinished;
    public event Action<int>      OnOpponentScoreUpdated;
    public event Action<string>   OnAiReaction;
    public event Action<System.Collections.Generic.List<LobbyPlayerDto>> OnLobbyUpdated;
    public event Action<string>   OnChallengeError;

    private Coroutine _loadingCoroutine;
    private Coroutine _readyTimeoutCoroutine;
    private bool _matchLoadingActive;
    private bool _finishSubmitted; // guard: prevent emitting submitScore twice
    private bool _resultShown;     // guard: prevent processing matchFinished twice
    private System.Action<string> _challengeErrorHandler;
    private System.Action<PlanetStageStateDto> _loadingStateCallback;
    private System.Action<string> _loadingErrorCallback;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ── Gameplay: enter match session ──

    public void EnterMatch(MatchDto match)
    {
        _finishSubmitted = false;
        _resultShown     = false;

        MatchSession.Instance.StartMatch(match, match.myUserId ?? "");

        if (!string.IsNullOrEmpty(match.matchStageId))
            AppSession.Instance.SetSelectedStage(match.matchStageId);

        // Subscribe early — covers the loading phase too (opponent may disconnect before scene activates)
        if (MatchSocketClient.Instance != null)
        {
            MatchSocketClient.Instance.OnMatchFinished      += HandleMatchFinished;
            MatchSocketClient.Instance.OnMatchError         += HandleMatchError;
            MatchSocketClient.Instance.OnAchievementRewards += HandleLateAchievements;
        }

        var config = Resources.Load<GameConfig>("GameConfig");
        if (config == null) return;

        _matchLoadingActive = true;
        _loadingCoroutine = StartCoroutine(EnterMatchWithLoadingRoutine(config.gameplaySceneIndex, config.planetSceneIndex));
    }

    private IEnumerator EnterMatchWithLoadingRoutine(int sceneIndex, int fallbackSceneIndex)
    {
        SceneLoader.HoldActivation = true;
        SceneLoader.Instance.LoadScene(sceneIndex);

        bool done = false;
        _loadingStateCallback = _ => done = true;
        _loadingErrorCallback = _ => { done = true; SceneLoader.HoldActivation = false; };

        GameManager.Instance.OnPlanetStageStateLoaded += _loadingStateCallback;
        GameManager.Instance.OnError                  += _loadingErrorCallback;
        GameManager.Instance.RequestPlanetStageState(forceRefresh: true);

        float elapsed = 0f;
        while (!done && elapsed < 30f) { elapsed += Time.unscaledDeltaTime; yield return null; }

        ClearLoadingCallbacks();

        _loadingCoroutine   = null;
        _matchLoadingActive = false;
        SceneLoader.HoldActivation = false;
    }

    private void ClearLoadingCallbacks()
    {
        if (GameManager.Instance == null) return;
        if (_loadingStateCallback != null) { GameManager.Instance.OnPlanetStageStateLoaded -= _loadingStateCallback; _loadingStateCallback = null; }
        if (_loadingErrorCallback != null) { GameManager.Instance.OnError                  -= _loadingErrorCallback; _loadingErrorCallback = null; }
    }

    private void AbortLoading(int sceneIndex)
    {
        if (!_matchLoadingActive) return;
        if (_loadingCoroutine != null) { StopCoroutine(_loadingCoroutine); _loadingCoroutine = null; }
        _matchLoadingActive = false;
        ClearLoadingCallbacks();
        GameManager.Instance?.ClearCache();
        SceneLoader.Instance?.AbortPendingAndLoad(sceneIndex);
    }

    // Called by GameBootstrap after stage is fully initialized — starts the timer sync
    public void NotifyStageReady()
    {
        var matchId = MatchSession.Instance?.MatchId;
        var userId  = MatchSession.Instance?.MyUserId;
        if (string.IsNullOrEmpty(matchId) || string.IsNullOrEmpty(userId)) return;
        MatchSocketClient.Instance?.EmitPlayerReady(matchId, userId);
        _readyTimeoutCoroutine = StartCoroutine(PlayerReadyTimeoutRoutine());
    }

    private IEnumerator PlayerReadyTimeoutRoutine()
    {
        yield return new WaitForSecondsRealtime(60f);
        // matchStarted never arrived — treat as a connection failure
        Debug.LogWarning("[MatchManager] playerReady timeout — no matchStarted received");
        HandleMatchError("Match start timed out. Please return to the planet map.");
    }

    // Called by MatchHUD on first OnProgressChanged — subscribes to score/reaction/timer events
    public void BeginGameplay(int initialScore)
    {
        MatchSession.Instance.SetInitialScore(initialScore);

        var config    = Resources.Load<GameConfig>("GameConfig");
        var serverUrl = config?.serverBaseUrl ?? "http://localhost:3000";
        var matchId   = MatchSession.Instance?.MatchId;
        var userId    = MatchSession.Instance?.MyUserId;

        if (MatchSocketClient.Instance != null && !string.IsNullOrEmpty(matchId))
        {
            MatchSocketClient.Instance.OnMatchStateReceived += HandleSocketMatchState;
            MatchSocketClient.Instance.OnReactionReceived   += HandleSocketReaction;
            MatchSocketClient.Instance.OnMatchStarted       += HandleMatchStarted;
            // OnMatchFinished already subscribed in EnterMatch — do not add again
            MatchSocketClient.Instance.Connect(serverUrl, matchId, userId);
        }
    }

    private void HandleSocketMatchState(JArray players)
    {
        if (players == null) return;
        var myId = MatchSession.Instance?.MyUserId;
        foreach (var p in players)
        {
            if (p["userId"]?.ToString() != myId)
            {
                OnOpponentScoreUpdated?.Invoke(p["score"]?.Value<int>() ?? 0);
                break;
            }
        }
    }

    private void HandleSocketReaction(string message)
    {
        if (!string.IsNullOrEmpty(message))
            OnAiReaction?.Invoke(message);
    }

    private void HandleMatchStarted(long startTimeMs)
    {
        if (_readyTimeoutCoroutine != null)
        {
            StopCoroutine(_readyTimeoutCoroutine);
            _readyTimeoutCoroutine = null;
        }
        if (MatchSession.Instance != null && MatchSession.Instance.StartTimeMs == 0)
            MatchSession.Instance.SetStartTime(startTimeMs);
    }

    private void HandleMatchError(string message)
    {
        if (_resultShown) return;
        _resultShown = true;

        Debug.LogError($"[MatchManager] Match error: {message}");
        if (_readyTimeoutCoroutine != null) { StopCoroutine(_readyTimeoutCoroutine); _readyTimeoutCoroutine = null; }

        PendingLobbyError = message; // shown next time the lobby opens
        DisconnectSocket();
        MatchSession.Instance?.Clear();
        GameManager.Instance?.ClearCache();

        var config = Resources.Load<GameConfig>("GameConfig");
        int planetScene = config?.planetSceneIndex ?? 5;

        if (_matchLoadingActive)
            AbortLoading(planetScene);
        else
            SceneLoader.Instance?.LoadScene(planetScene);
    }

    private void HandleMatchFinished(MatchDto match)
    {
        if (_resultShown) return;
        _resultShown = true;

        if (_readyTimeoutCoroutine != null) { StopCoroutine(_readyTimeoutCoroutine); _readyTimeoutCoroutine = null; }

        var config = Resources.Load<GameConfig>("GameConfig");

        if (_matchLoadingActive)
        {
            // Opponent disconnected while we were still loading — go back to planet map
            DisconnectSocket();
            MatchSession.Instance?.Clear();
            GameManager.Instance?.ClearCache();
            AbortLoading(config?.planetSceneIndex ?? 5);
            return;
        }

        // Unsubscribe gameplay events but keep socket open so the server's async
        // achievementRewards event (sent after matchFinished) can still arrive.
        // Full disconnect happens when the user presses Back (CancelMatch → DisconnectSocket).
        if (MatchSocketClient.Instance != null)
        {
            MatchSocketClient.Instance.OnMatchStateReceived -= HandleSocketMatchState;
            MatchSocketClient.Instance.OnReactionReceived   -= HandleSocketReaction;
            MatchSocketClient.Instance.OnMatchStarted       -= HandleMatchStarted;
            MatchSocketClient.Instance.OnMatchFinished      -= HandleMatchFinished;
            MatchSocketClient.Instance.OnMatchError         -= HandleMatchError;
        }

        var midGameAchievements = MatchSession.Instance?.TakeAchievements() ?? new System.Collections.Generic.List<UnlockedAchievementDto>();
        var endAchievements     = match?.achievementRewards ?? new System.Collections.Generic.List<UnlockedAchievementDto>();
        midGameAchievements.AddRange(endAchievements);
        AchievementNotifier.Notify(midGameAchievements);

        OnMatchFinished?.Invoke(match);
        MatchSession.Instance?.Clear();
    }

    public void PushScore()
    {
        if (MatchSession.Instance == null || !MatchSession.Instance.IsActive) return;
        var matchId = MatchSession.Instance?.MatchId;
        var userId  = MatchSession.Instance?.MyUserId;
        if (string.IsNullOrEmpty(matchId)) return;
        MatchSocketClient.Instance?.EmitScore(matchId, userId, GetCurrentMatchScore());
    }

    // Deck empty or time up — emit submitScore; server replies with matchFinished to both sides
    public void SubmitFinalScore()
    {
        if (_finishSubmitted) return;
        _finishSubmitted = true;

        var matchId = MatchSession.Instance?.MatchId;
        var userId  = MatchSession.Instance?.MyUserId;
        MatchSocketClient.Instance?.EmitSubmitScore(matchId, userId, GetCurrentMatchScore());
    }

    // ── Lobby ────────────────────────────────────────────────────

    public void OpenLobby()
    {
        var (planetId, stageId) = GetMatchStage();

        _challengeErrorHandler = err => OnChallengeError?.Invoke(err);
        MatchSocketClient.Instance.OnLobbyUpdated   += OnLobbyPlayersUpdated;
        MatchSocketClient.Instance.OnMatchReady     += OnMatchReadyReceived;
        MatchSocketClient.Instance.OnChallengeError += _challengeErrorHandler;

        EnsureSocketConnected(() =>
        {
            MatchSocketClient.Instance.JoinLobby(
                AppSession.Instance?.UserId,
                AppSession.Instance?.Username,
                AppSession.Instance?.SelectedAvatar ?? "avatar",
                planetId, stageId);
        });
    }

    public void CloseLobby()
    {
        if (MatchSocketClient.Instance == null) return;
        MatchSocketClient.Instance.OnLobbyUpdated   -= OnLobbyPlayersUpdated;
        MatchSocketClient.Instance.OnMatchReady     -= OnMatchReadyReceived;
        if (_challengeErrorHandler != null)
        {
            MatchSocketClient.Instance.OnChallengeError -= _challengeErrorHandler;
            _challengeErrorHandler = null;
        }
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
                AppSession.Instance?.SelectedAvatar ?? "avatar",
                planetId, stageId);
        });
    }

    private void OnLobbyPlayersUpdated(System.Collections.Generic.List<LobbyPlayerDto> players) =>
        OnLobbyUpdated?.Invoke(players);

    private void OnMatchReadyReceived(MatchDto match) => EnterMatch(match);

    private void EnsureSocketConnected(System.Action onConnected = null)
    {
        if (MatchSocketClient.Instance == null) return;
        var config    = Resources.Load<GameConfig>("GameConfig");
        var serverUrl = config?.serverBaseUrl ?? "http://localhost:3000";
        var userId    = AppSession.Instance?.UserId ?? "";
        MatchSocketClient.Instance.Connect(serverUrl, "", userId, onConnected);
    }

    // ── Helpers ──────────────────────────────────────────────────

    public void CancelMatch()
    {
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

    private void HandleLateAchievements(System.Collections.Generic.List<UnlockedAchievementDto> rewards)
    {
        if (rewards != null && rewards.Count > 0)
            AchievementNotifier.Notify(rewards);
    }

    private void DisconnectSocket()
    {
        if (MatchSocketClient.Instance == null) return;
        MatchSocketClient.Instance.OnMatchStateReceived -= HandleSocketMatchState;
        MatchSocketClient.Instance.OnReactionReceived   -= HandleSocketReaction;
        MatchSocketClient.Instance.OnMatchStarted       -= HandleMatchStarted;
        MatchSocketClient.Instance.OnMatchFinished      -= HandleMatchFinished;
        MatchSocketClient.Instance.OnMatchError         -= HandleMatchError;
        MatchSocketClient.Instance.OnAchievementRewards -= HandleLateAchievements;
        MatchSocketClient.Instance.Disconnect();
    }

    private int GetCurrentMatchScore() =>
        MatchSession.Instance?.CurrentMatchScore ?? 0;

    private (string planetId, string stageId) GetMatchStage()
    {
        var planet = AppSession.Instance?.ActivePlanet;
        if (planet == null) return (null, null);
        var stage   = planet.stages?.Find(s => s.meta?.isUnlocked == true);
        var stageId = stage?.stageId ?? AppSession.Instance?.SelectedStageId;
        if (string.IsNullOrEmpty(stageId)) return (null, null);
        return (planet.planetId, stageId);
    }
}
