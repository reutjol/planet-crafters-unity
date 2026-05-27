using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controls the multiplayer lobby panel shown on the planet screen.
/// Assign all UI references in the Inspector.
/// </summary>
public class MatchLobbyController : MonoBehaviour
{
    [Header("Panel Root")]
    [SerializeField] private GameObject lobbyPanel;

    [Header("Main Buttons Screen")]
    [SerializeField] private GameObject mainButtonsScreen;
    [SerializeField] private Button createButton;
    [SerializeField] private Button joinButton;
    [SerializeField] private Button closeButton;

    [Header("Create Room Screen")]
    [SerializeField] private GameObject createRoomScreen;
    [SerializeField] private TextMeshProUGUI roomCodeText;
    [SerializeField] private TextMeshProUGUI waitingStatusText;
    [SerializeField] private Button cancelCreateButton;

    [Header("Join Room Screen")]
    [SerializeField] private GameObject joinRoomScreen;
    [SerializeField] private TMP_InputField codeInputField;
    [SerializeField] private Button confirmJoinButton;
    [SerializeField] private Button cancelJoinButton;
    [SerializeField] private TextMeshProUGUI joinErrorText;

    private void Start()
    {
        createButton?.onClick.AddListener(OnCreateClicked);
        joinButton?.onClick.AddListener(OnJoinClicked);
        closeButton?.onClick.AddListener(OnCloseClicked);
        cancelCreateButton?.onClick.AddListener(OnCancelCreateClicked);
        confirmJoinButton?.onClick.AddListener(OnConfirmJoinClicked);
        cancelJoinButton?.onClick.AddListener(OnCancelJoinClicked);

        if (MatchManager.Instance != null)
            MatchManager.Instance.OnMatchUpdated += OnMatchUpdated;
    }

    private void OnDestroy()
    {
        if (MatchManager.Instance != null)
            MatchManager.Instance.OnMatchUpdated -= OnMatchUpdated;
    }

    // ── Public: open from PlanetScreenController ──

    public void Open()
    {
        lobbyPanel?.SetActive(true);
        ShowScreen(mainButtonsScreen);
        PopupManager.OnPopupOpened();
    }

    private void OnCloseClicked()
    {
        MatchManager.Instance?.CancelMatch();
        lobbyPanel?.SetActive(false);
        PopupManager.OnPopupClosed();
    }

    // ── Create Room ──

    private void OnCreateClicked()
    {
        ShowScreen(createRoomScreen);
        if (roomCodeText != null) roomCodeText.text = "...";
        if (waitingStatusText != null) waitingStatusText.text = "Creating room...";

        MatchManager.Instance?.CreateRoom(
            onSuccess: match =>
            {
                if (roomCodeText != null) roomCodeText.text = match.code;
                if (waitingStatusText != null) waitingStatusText.text = "Waiting for opponent...";
            },
            onError: err =>
            {
                if (waitingStatusText != null) waitingStatusText.text = $"Error: {err}";
                Debug.LogError($"[MatchLobby] CreateRoom error: {err}");
            });
    }

    private void OnCancelCreateClicked()
    {
        MatchManager.Instance?.CancelMatch();
        ShowScreen(mainButtonsScreen);
    }

    // ── Join Room ──

    private void OnJoinClicked()
    {
        if (joinErrorText != null) joinErrorText.text = "";
        if (codeInputField != null) codeInputField.text = "";
        ShowScreen(joinRoomScreen);
    }

    private void OnConfirmJoinClicked()
    {
        var code = codeInputField?.text?.Trim().ToUpper();
        if (string.IsNullOrEmpty(code))
        {
            if (joinErrorText != null) joinErrorText.text = "Enter a room code";
            return;
        }

        if (confirmJoinButton != null) confirmJoinButton.interactable = false;
        if (joinErrorText != null) joinErrorText.text = "Joining...";

        MatchManager.Instance?.JoinRoom(code,
            onSuccess: _ => { /* EnterMatch is called inside MatchManager */ },
            onError: err =>
            {
                if (joinErrorText != null) joinErrorText.text = err;
                if (confirmJoinButton != null) confirmJoinButton.interactable = true;
            });
    }

    private void OnCancelJoinClicked()
    {
        ShowScreen(mainButtonsScreen);
    }

    // ── Match state updates (while waiting in lobby) ──

    private void OnMatchUpdated(MatchDto match)
    {
        if (match.status == "active" && waitingStatusText != null)
            waitingStatusText.text = "Match starting...";
    }

    // ── Helpers ──

    private void ShowScreen(GameObject screen)
    {
        if (mainButtonsScreen != null) mainButtonsScreen.SetActive(mainButtonsScreen == screen);
        if (createRoomScreen != null) createRoomScreen.SetActive(createRoomScreen == screen);
        if (joinRoomScreen != null) joinRoomScreen.SetActive(joinRoomScreen == screen);
    }
}
