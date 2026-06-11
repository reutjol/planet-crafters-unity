using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MatchLobbyController : MonoBehaviour
{
    [Header("Panel Root")]
    [SerializeField] private GameObject lobbyPanel;
    [SerializeField] private Button closeButton;

    [Header("Lobby Screen")]
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private Button randomButton;
    [SerializeField] private Transform playerListContent;
    [SerializeField] private GameObject playerRowPrefab;

    private readonly List<GameObject> _playerRows = new List<GameObject>();

    private void Start()
    {
        closeButton?.onClick.AddListener(OnCloseClicked);
        randomButton?.onClick.AddListener(OnRandomClicked);

        if (MatchManager.Instance != null)
        {
            MatchManager.Instance.OnLobbyUpdated   += OnLobbyUpdated;
            MatchManager.Instance.OnChallengeError += OnChallengeError;
        }

        if (MatchSocketClient.Instance != null)
            MatchSocketClient.Instance.OnChallenged += OnChallenged;
    }

    private void OnDestroy()
    {
        if (MatchManager.Instance != null)
        {
            MatchManager.Instance.OnLobbyUpdated   -= OnLobbyUpdated;
            MatchManager.Instance.OnChallengeError -= OnChallengeError;
        }

        if (MatchSocketClient.Instance != null)
            MatchSocketClient.Instance.OnChallenged -= OnChallenged;
    }

    // ── Public: open from PlanetScreenController ──

    public void Open()
    {
        lobbyPanel?.SetActive(true);
        ShowStatus(L("text.looking_for_players", "Looking for players..."));
        PopupManager.OnPopupOpened();
        Debug.Log($"[Lobby] Open called. MatchManager={MatchManager.Instance} AppSession.UserId={AppSession.Instance?.UserId}");
        MatchManager.Instance?.OpenLobby();
    }

    private void OnCloseClicked()
    {
        MatchManager.Instance?.CloseLobby();
        lobbyPanel?.SetActive(false);
        PopupManager.OnPopupClosed();
    }

    // ── Lobby events ──

    private void OnLobbyUpdated(List<LobbyPlayerDto> players)
    {
        Debug.Log($"[Lobby] OnLobbyUpdated: {players?.Count ?? 0} players total");
        var myId = AppSession.Instance?.UserId;
        var others = players?.FindAll(p => p.userId != myId) ?? new List<LobbyPlayerDto>();

        // Update status text
        ShowStatus(others.Count == 0
            ? L("text.waiting_for_players", "Waiting for players...")
            : $"{others.Count} {L("text.players_online", "player(s) online")}");

        // Enable/disable random button
        if (randomButton != null) randomButton.interactable = others.Count > 0;

        // Rebuild player rows
        foreach (var row in _playerRows) Destroy(row);
        _playerRows.Clear();

        if (playerListContent == null || playerRowPrefab == null) return;

        foreach (var player in others)
        {
            var row = Instantiate(playerRowPrefab, playerListContent);
            _playerRows.Add(row);

            var nameText = row.GetComponentInChildren<TextMeshProUGUI>();
            if (nameText != null) nameText.text = player.username;

            var btn = row.GetComponentInChildren<Button>();
            if (btn != null)
            {
                var id = player.userId;
                btn.onClick.AddListener(() => OnChallengeClicked(id));
            }
        }
    }

    private void OnChallenged()
    {
        DisableAllButtons();
        ShowStatus(L("text.someone_selected_you", "Someone selected you! Starting match..."));
    }

    private void OnChallengeError(string err)
    {
        ShowStatus($"Error: {err}");
    }

    // ── Buttons ──

    private void OnRandomClicked()
    {
        DisableAllButtons();
        ShowStatus(L("text.challenging_random", "Challenging random player..."));
        MatchManager.Instance?.ChallengeRandom();
    }

    private void OnChallengeClicked(string targetUserId)
    {
        DisableAllButtons();
        ShowStatus(L("text.challenging", "Challenging..."));
        MatchManager.Instance?.ChallengePlayer(targetUserId);
    }

    private void DisableAllButtons()
    {
        if (randomButton != null) randomButton.interactable = false;
        foreach (var row in _playerRows)
        {
            if (row == null) continue;
            var btn = row.GetComponentInChildren<Button>();
            if (btn != null) btn.interactable = false;
        }
    }

    private static string L(string key, string fallback) =>
        UnityLocalizationService.Instance != null
            ? UnityLocalizationService.Instance.GetText(key, fallback)
            : fallback;

    private void ShowStatus(string msg)
    {
        if (statusText == null) return;
        statusText.text = msg;
        var svc = UnityLocalizationService.Instance;
        if (svc != null) statusText.isRightToLeftText = svc.IsRightToLeft;
    }
}
