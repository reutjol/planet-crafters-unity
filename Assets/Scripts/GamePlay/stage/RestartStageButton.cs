using System.Collections;
using UnityEngine;

/// <summary>
/// Button handler for restarting the stage from the beginning
/// Resets server state and reloads the scene
/// </summary>
public class RestartStageButton : MonoBehaviour
{
    [SerializeField] private GameConfig gameConfig;

    private void Awake()
    {
        if (gameConfig == null)
        {
            gameConfig = Resources.Load<GameConfig>("GameConfig");
        }
    }

    /// <summary>
    /// Handles restart button click - resets stage on server and reloads scene
    /// </summary>
    public void OnRestartClicked()
    {
        StartCoroutine(ResetAndReload());
    }

    private IEnumerator ResetAndReload()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("[Restart] GameManager.Instance is null!");
            yield break;
        }

        bool resetDone = false;
        bool resetSuccess = false;

        // Reset stage on server
        GameManager.Instance.ResetCurrentStage(
            onSuccess: () =>
            {
                resetDone = true;
                resetSuccess = true;
            },
            onError: (err) =>
            {
                resetDone = true;
                resetSuccess = false;
                Debug.LogError($"[Restart] Reset failed: {err}");
            }
        );

        // Wait for reset
        float timeout = 10f;
        float elapsed = 0f;

        while (!resetDone && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (!resetSuccess)
        {
            Debug.LogError("[Restart] Failed to reset stage");
            yield break;
        }

        // Clear cache and reload
        GameManager.Instance.ClearCache();

        if (SceneLoader.Instance != null && gameConfig != null)
        {
            SceneLoader.Instance.LoadScene(gameConfig.gameplaySceneIndex);
        }
    }
}