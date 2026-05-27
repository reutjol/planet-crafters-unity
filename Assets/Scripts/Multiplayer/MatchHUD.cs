using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// In-game HUD for multiplayer matches. Shows timer, match scores, and result.
/// Place this in the gameplay scene. Assign MapController and UI references in Inspector.
/// The HUD activates automatically when MatchSession.IsActive.
/// </summary>
public class MatchHUD : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MapController mapController;

    [Header("HUD Panel")]
    [SerializeField] private GameObject hudPanel;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI myScoreText;
    [SerializeField] private TextMeshProUGUI opponentScoreText;

    [Header("Result Panel")]
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TextMeshProUGUI resultTitleText;
    [SerializeField] private TextMeshProUGUI resultScoreText;
    [SerializeField] private Button resultBackButton;

    private bool _matchActive;
    private bool _initialScoreSet;
    private int _myMatchScore;
    private int _opponentMatchScore;
    private bool _resultShown;

    private void Start()
    {
        if (resultBackButton != null)
            resultBackButton.onClick.AddListener(OnResultBack);

        _matchActive = MatchSession.Instance != null && MatchSession.Instance.IsActive;

        if (hudPanel != null) hudPanel.SetActive(_matchActive);
        if (resultPanel != null) resultPanel.SetActive(false);

        if (!_matchActive) return;

        if (mapController == null)
            mapController = FindObjectOfType<MapController>();

        if (mapController != null)
            mapController.OnProgressChanged += OnProgressChanged;

        if (MatchManager.Instance != null)
        {
            MatchManager.Instance.OnMatchFinished += OnMatchFinished;
            MatchManager.Instance.OnMatchUpdated += OnMatchUpdated;
        }

        // ApplyServerState may have fired before we subscribed — read from cache
        var cached = GameManager.Instance?.GetCachedPlanetStageState();
        if (cached?.progress != null)
            OnProgressChanged(cached.progress);
    }

    private void OnDestroy()
    {
        if (mapController != null)
            mapController.OnProgressChanged -= OnProgressChanged;

        if (MatchManager.Instance != null)
        {
            MatchManager.Instance.OnMatchFinished -= OnMatchFinished;
            MatchManager.Instance.OnMatchUpdated -= OnMatchUpdated;
        }
    }

    private void OnProgressChanged(ProgressDto progress)
    {
        if (!_initialScoreSet)
        {
            MatchSession.Instance?.SetInitialScore(progress.score);
            MatchManager.Instance?.BeginGameplayPolling(progress.score);
            _initialScoreSet = true;
            MatchSession.Instance?.UpdateCurrentScore(progress.score);
            _myMatchScore = 0;
            RefreshScoreDisplay();
            return;
        }

        MatchSession.Instance?.UpdateCurrentScore(progress.score);
        _myMatchScore = MatchSession.Instance?.CurrentMatchScore ?? 0;
        RefreshScoreDisplay();
        MatchManager.Instance?.PushScore();
    }

    private void Update()
    {
        if (!_matchActive || _resultShown) return;
        if (MatchSession.Instance == null || MatchSession.Instance.StartTimeMs == 0) return;

        UpdateTimer();

        var remaining = MatchSession.Instance.GetSecondsRemaining();
        if (remaining <= 0f)
        {
            _matchActive = false;
            MatchManager.Instance?.SubmitFinalScore();
        }
    }

    private void OnMatchUpdated(MatchDto match)
    {
        var myId = MatchSession.Instance?.MyUserId;
        foreach (var p in match.players)
        {
            if (p.userId != myId)
            {
                _opponentMatchScore = p.score;
                break;
            }
        }
        RefreshScoreDisplay();
    }

    private void OnMatchFinished(MatchDto match)
    {
        if (_resultShown) return;
        _resultShown = true;
        _matchActive = false;

        if (hudPanel != null) hudPanel.SetActive(false);
        if (resultPanel != null) resultPanel.SetActive(true);

        // Refresh planet so top bar reflects any coin award
        GameManager.Instance?.RequestActivePlanet(forceRefresh: true);

        var myId = MatchSession.Instance?.MyUserId;
        var winnerId = match.winnerId;

        if (resultTitleText != null)
        {
            if (winnerId == null) resultTitleText.text = "Draw!";
            else if (winnerId == myId) resultTitleText.text = "You Win!";
            else resultTitleText.text = "You Lose!";
        }

        if (resultScoreText != null)
        {
            int myScore = 0, oppScore = 0;
            foreach (var p in match.players)
            {
                if (p.userId == myId) myScore = p.score;
                else oppScore = p.score;
            }
            resultScoreText.text = $"You: {myScore}  |  Opponent: {oppScore}";
        }
    }

    private void OnResultBack()
    {
        var config = Resources.Load<GameConfig>("GameConfig");
        if (config != null)
            UnityEngine.SceneManagement.SceneManager.LoadScene(config.planetSceneIndex);
    }

    private void UpdateTimer()
    {
        if (timerText == null || MatchSession.Instance == null) return;
        var remaining = Mathf.CeilToInt(MatchSession.Instance.GetSecondsRemaining());
        var minutes = remaining / 60;
        var seconds = remaining % 60;
        timerText.text = $"{minutes}:{seconds:D2}";
    }

    private void RefreshScoreDisplay()
    {
        if (myScoreText != null) myScoreText.text = $"You: {_myMatchScore}";
        if (opponentScoreText != null) opponentScoreText.text = $"Opp: {_opponentMatchScore}";
    }
}
