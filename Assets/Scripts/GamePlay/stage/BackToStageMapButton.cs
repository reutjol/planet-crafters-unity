using UnityEngine;

/// <summary>
/// Returns to stage map and clears cache so next load will be fresh from server
/// </summary>
public class BackToStageMapButton : MonoBehaviour
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
    /// Handles back button click - clears cache and returns to stage map
    /// </summary>
    public void OnBackClicked()
    {
        // Clear cache so when we return, GameBootstrap will load fresh state
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ClearCache();
        }

        if (SceneLoader.Instance != null && gameConfig != null)
        {
            SceneLoader.Instance.LoadScene(gameConfig.stagesMapSceneIndex);
        }
    }
}
