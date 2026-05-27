using System;
using System.Collections;
using UnityEngine;

public class MatchManager : MonoBehaviour
{
    public static MatchManager Instance { get; private set; }

    public event Action<MatchDto> OnMatchUpdated;
    public event Action<MatchDto> OnMatchFinished;

    private const float LobbyPollInterval = 3f;
    private const float GameplayPollInterval = 5f;
    private const int DefaultDuration = 180;

    private Coroutine _pollCoroutine;
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
                m => match = m, _ => { });

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

        var config = Resources.Load<GameConfig>("GameConfig");
        if (config == null) return;

        StartCoroutine(EnterMatchWithLoadingRoutine(config.gameplaySceneIndex));
    }

    private IEnumerator EnterMatchWithLoadingRoutine(int sceneIndex)
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

        // Release loading screen — gameplay scene will activate
        SceneLoader.HoldActivation = false;
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
            m => result = m, _ => { });

        if (result?.startTime != null && MatchSession.Instance != null && MatchSession.Instance.StartTimeMs == 0)
            MatchSession.Instance.SetStartTime(result.startTime.Value);
    }

    public void BeginGameplayPolling(int initialScore)
    {
        MatchSession.Instance.SetInitialScore(initialScore);
        StopPolling();
        _pollCoroutine = StartCoroutine(GameplayPollRoutine());
    }

    public void PushScore()
    {
        if (MatchSession.Instance == null || !MatchSession.Instance.IsActive) return;
        var matchId = MatchSession.Instance?.MatchId;
        var token = AppSession.Instance?.AccessToken;
        if (string.IsNullOrEmpty(matchId)) return;
        StartCoroutine(PushScoreRoutine(matchId, GetCurrentMatchScore(), token));
    }

    private IEnumerator PushScoreRoutine(string matchId, int score, string token)
    {
        MatchDto result = null;
        yield return MatchApiClient.Instance.UpdateScore(matchId, score, token,
            m => result = m, _ => { });

        if (result == null) yield break;

        OnMatchUpdated?.Invoke(result);

        if (result.status == "finished")
        {
            OnMatchFinished?.Invoke(result);
            MatchSession.Instance?.Clear();
        }
    }

    private IEnumerator GameplayPollRoutine()
    {
        var token = AppSession.Instance?.AccessToken;
        var matchId = MatchSession.Instance?.MatchId;

        while (MatchSession.Instance != null && MatchSession.Instance.IsActive)
        {
            yield return new WaitForSeconds(GameplayPollInterval);

            if (string.IsNullOrEmpty(matchId)) yield break;

            // Poll only to sync startTime until it arrives; safety net for match finish
            MatchDto match = null;
            yield return MatchApiClient.Instance.GetMatch(matchId, token,
                m => match = m, _ => { });

            if (match != null)
            {
                if (match.startTime != null && MatchSession.Instance != null && MatchSession.Instance.StartTimeMs == 0)
                    MatchSession.Instance.SetStartTime(match.startTime.Value);

                OnMatchUpdated?.Invoke(match);

                if (match.status == "finished")
                {
                    OnMatchFinished?.Invoke(match);
                    MatchSession.Instance.Clear();
                    yield break;
                }
            }
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
        yield return MatchApiClient.Instance.FinishMatch(matchId, finalScore, token,
            m => result = m, _ => { });

        if (result != null)
            OnMatchFinished?.Invoke(result);

        MatchSession.Instance?.Clear();
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
        MatchSession.Instance?.Clear();
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
