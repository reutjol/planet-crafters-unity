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
        // Clear stage state cache
        if (GameManager.Instance != null)
            GameManager.Instance.ClearCache();

        // Clear planet cache so stage map always fetches fresh data (updated isUnlocked, progress)
        if (AppSession.Instance != null)
            AppSession.Instance.ActivePlanet = null;

        if (SceneLoader.Instance != null)
        {
            if (SceneLoader.Instance.PreviousSceneIndex >= 0)
                SceneLoader.Instance.GoBack();
            else
                SceneLoader.Instance.LoadScene(gameConfig.stagesMapSceneIndex);
        }
    }
}
