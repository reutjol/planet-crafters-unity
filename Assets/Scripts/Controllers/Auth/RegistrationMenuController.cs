using System.Collections;
using UnityEngine;

/// <summary>
/// Initial authentication menu controller.
/// Provides navigation to Sign In and Sign Up screens.
/// On start, attempts auto-login via stored refresh token.
/// </summary>
public class RegistrationMenuController : MonoBehaviour
{
    [SerializeField] private AuthApiClient authApi;
    [SerializeField] private GameConfig gameConfig;

    private void Start()
    {
        if (gameConfig == null)
            gameConfig = Resources.Load<GameConfig>("GameConfig");

        if (AppSession.Instance != null && AppSession.Instance.HasRefresh())
            StartCoroutine(TryAutoLogin());
    }

    private IEnumerator TryAutoLogin()
    {
        if (authApi == null)
            authApi = FindObjectOfType<AuthApiClient>(true);

        if (authApi == null)
        {
            Debug.LogWarning("[RegistrationMenu] AuthApiClient not found, skipping auto-login");
            yield break;
        }

        string newToken = null;
        yield return authApi.Refresh(
            AppSession.Instance.RefreshToken,
            onSuccess: t => { newToken = t; AppSession.Instance.SetAccess(t); },
            onError: err => { Debug.Log($"[RegistrationMenu] Refresh expired ({err})"); AppSession.Instance.Logout(); }
        );

        if (string.IsNullOrEmpty(newToken)) yield break;

        Debug.Log("[RegistrationMenu] Auto-login successful");

        // Fetch user info if not already stored
        if (string.IsNullOrEmpty(AppSession.Instance.UserId) || string.IsNullOrEmpty(AppSession.Instance.Username))
        {
            var profileApi = FindObjectOfType<PlayerProfileApiClient>(true);
            if (profileApi != null)
            {
                yield return profileApi.GetMyProfile(newToken,
                    user => AppSession.Instance.SetUser(user.id, user.userName),
                    _ => { });
            }
        }

        SceneLoader.Instance.LoadScene(gameConfig.planetSceneIndex);
    }

    public void OnClickSignIn()
    {
        SceneLoader.Instance.LoadScene(3);
    }

    public void OnClickSignUp()
    {
        SceneLoader.Instance.LoadScene(4);
    }
}
