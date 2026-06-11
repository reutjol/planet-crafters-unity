using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Initial authentication menu controller.
/// Provides navigation to Sign In and Sign Up screens.
/// On start, attempts auto-login via stored refresh token.
/// </summary>
public class RegistrationMenuController : MonoBehaviour
{
    [SerializeField] private AuthApiClient authApi;
    [SerializeField] private GameConfig gameConfig;
    [SerializeField] private Button googleSignInButton;
    [SerializeField] private string googleWebClientId = "283464708395-4cbj9ei0j60i3d3bei059nqdt77tkbpp.apps.googleusercontent.com";

    private static RegistrationMenuController activeController;

    private bool googleSignInInProgress;

    private void Awake()
    {
        if (activeController != null && activeController != this)
        {
            enabled = false;
            return;
        }

        activeController = this;
    }

    private void Start()
    {
        if (gameConfig == null)
            gameConfig = Resources.Load<GameConfig>("GameConfig");

        BindGoogleSignInButton();

        if (AppSession.Instance != null && AppSession.Instance.HasRefresh())
            StartCoroutine(TryAutoLogin());
    }

    private void OnDestroy()
    {
        if (googleSignInButton != null)
            googleSignInButton.onClick.RemoveListener(OnClickGoogleSignIn);

        if (activeController == this)
            activeController = null;
    }

    private void BindGoogleSignInButton()
    {
        if (googleSignInButton == null)
        {
            Button[] buttons = FindObjectsOfType<Button>(true);
            foreach (Button button in buttons)
            {
                string buttonName = button.name.Replace(" ", "");
                bool looksLikeGoogleButton =
                    buttonName.IndexOf("connect", StringComparison.OrdinalIgnoreCase) >= 0 &&
                    buttonName.IndexOf("google", StringComparison.OrdinalIgnoreCase) >= 0;

                if (!looksLikeGoogleButton) continue;

                googleSignInButton = button;
                break;
            }
        }

        if (googleSignInButton == null)
        {
            Debug.LogWarning("[RegistrationMenu] Google Sign-In button not found");
            return;
        }

        googleSignInButton.onClick.RemoveListener(OnClickGoogleSignIn);
        googleSignInButton.onClick.AddListener(OnClickGoogleSignIn);
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
        string refreshError = null;
        yield return authApi.Refresh(
            AppSession.Instance.RefreshToken,
            onSuccess: t => { newToken = t; AppSession.Instance.SetAccess(t); },
            onError: err => { refreshError = err; }
        );

        if (!string.IsNullOrEmpty(refreshError))
        {
            bool isAuthFailure = refreshError.Contains("401") || refreshError.Contains("403") ||
                                 refreshError.Contains("Unauthorized") || refreshError.Contains("Forbidden") ||
                                 refreshError.Contains("invalid") || refreshError.Contains("expired");
            if (isAuthFailure)
            {
                Debug.LogWarning($"[RegistrationMenu] Refresh token invalid/expired, logging out: {refreshError}");
                AppSession.Instance.Logout();
            }
            else
            {
                Debug.LogWarning($"[RegistrationMenu] Auto-login skipped due to network error: {refreshError}");
            }
            yield break;
        }

        if (string.IsNullOrEmpty(newToken)) yield break;

        // Fetch user info if not already stored
        if (string.IsNullOrEmpty(AppSession.Instance.UserId) || string.IsNullOrEmpty(AppSession.Instance.Username))
        {
            var profileApi = FindObjectOfType<PlayerProfileApiClient>(true);
            if (profileApi != null)
            {
                yield return profileApi.GetMyProfile(newToken,
                    user => AppSession.Instance.SetUser(user.id, user.userName, user.selectedAvatar),
                    _ => { });
            }
        }

        SceneLoader.Instance.LoadScene(gameConfig.planetSceneIndex);
    }

    public void OnClickSignIn()
    {
        SceneLoader.Instance.LoadScene(gameConfig.signInSceneIndex);
    }

    public void OnClickSignUp()
    {
        SceneLoader.Instance.LoadScene(gameConfig.signUpSceneIndex);
    }

    public void OnClickGoogleSignIn()
    {
        if (googleSignInInProgress)
            return;

        googleSignInInProgress = true;

        if (authApi == null)
            authApi = FindObjectOfType<AuthApiClient>(true);

        if (authApi == null)
        {
            googleSignInInProgress = false;
            Debug.LogError("[RegistrationMenu] AuthApiClient not found, cannot sign in with Google");
            return;
        }

        if (AppSession.Instance == null)
        {
            googleSignInInProgress = false;
            Debug.LogError("[RegistrationMenu] AppSession not found, cannot save Google login tokens");
            return;
        }

        if (gameConfig == null)
            gameConfig = Resources.Load<GameConfig>("GameConfig");

        if (string.IsNullOrWhiteSpace(googleWebClientId))
        {
            googleSignInInProgress = false;
            Debug.LogError("[RegistrationMenu] Missing Google Web Client ID");
            return;
        }

        StartCoroutine(GoogleSignInRoutine());
    }

    private IEnumerator GoogleSignInRoutine()
    {
#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
        Google.GoogleSignIn.Configuration = new Google.GoogleSignInConfiguration
        {
            WebClientId = googleWebClientId,
            RequestIdToken = true,
            RequestEmail = true,
            RequestProfile = true
        };

        var signInTask = Google.GoogleSignIn.DefaultInstance.SignIn();
        while (!signInTask.IsCompleted)
            yield return null;

        if (signInTask.IsCanceled)
        {
            googleSignInInProgress = false;
            Debug.Log("[RegistrationMenu] Google Sign-In canceled");
            yield break;
        }

        if (signInTask.IsFaulted)
        {
            googleSignInInProgress = false;
            Debug.LogError($"[RegistrationMenu] Google Sign-In failed: {signInTask.Exception}");
            yield break;
        }

        var googleUser = signInTask.Result;
        if (googleUser == null || string.IsNullOrEmpty(googleUser.IdToken))
        {
            googleSignInInProgress = false;
            Debug.LogError("[RegistrationMenu] Google Sign-In failed: missing idToken");
            yield break;
        }

        yield return authApi.GoogleLogin(
            googleUser.IdToken,
            onSuccess: resp =>
            {
                AppSession.Instance.SetTokens(resp.accessToken, resp.refreshToken);
                AppSession.Instance.SetUser(resp.user?.id, resp.user?.userName, resp.user?.selectedAvatar);
                SceneLoader.Instance.LoadScene(gameConfig.planetSceneIndex);
            },
            onError: err =>
            {
                Debug.LogError($"[RegistrationMenu] Google backend login failed: {err}");
            }
        );

        googleSignInInProgress = false;
#else
        googleSignInInProgress = false;
        Debug.LogWarning("[RegistrationMenu] Google Sign-In must be tested in an Android or iOS build, not inside the Unity Editor");
        yield break;
#endif
    }
}
