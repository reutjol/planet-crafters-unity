using UnityEngine;

/// <summary>
/// Manages the in-game pause menu.
/// Attach to the pause panel GameObject (or any persistent object in the scene).
/// </summary>
public class PauseMenuController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject pausePanel;

    [Header("Refs")]
    [SerializeField] private GameConfig gameConfig;
    [SerializeField] private RestartStageButton restartStageButton;

    private void Awake()
    {
        if (gameConfig == null)
            gameConfig = Resources.Load<GameConfig>("GameConfig");

        if (pausePanel != null)
            pausePanel.SetActive(false);
    }

    // ── Called by the Pause (hamburger / || ) button in the HUD ──
    public void OnPauseClicked()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
            PopupManager.OnPopupOpened();
        }
    }

    // ── X button or "Continue" button inside the panel ──
    public void OnResumeClicked()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
            PopupManager.OnPopupClosed();
        }
    }

    // ── "Return to Map" button ──
    public void OnReturnToMapClicked()
    {
        MatchManager.Instance?.CancelMatch();

        if (GameManager.Instance != null)
            GameManager.Instance.ClearCache();

        if (SceneLoader.Instance != null && gameConfig != null)
            SceneLoader.Instance.LoadScene(gameConfig.stagesMapSceneIndex);
    }

    // ── "Restart" button ──
    public void OnRestartClicked()
    {
        if (restartStageButton != null)
            restartStageButton.OnRestartClicked();
    }
}
