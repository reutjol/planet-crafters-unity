using UnityEngine;

/// <summary>
/// Simple logout button for testing. Attach to a UI Button's OnClick.
/// </summary>
public class LogoutButton : MonoBehaviour
{
    [SerializeField] private GameConfig gameConfig;

    private void Start()
    {
        if (gameConfig == null)
            gameConfig = Resources.Load<GameConfig>("GameConfig");
    }

    public void OnClick()
    {
        AppSession.Instance?.Logout();
        SceneLoader.Instance?.LoadScene(gameConfig.registrationMenuSceneIndex);
    }
}
