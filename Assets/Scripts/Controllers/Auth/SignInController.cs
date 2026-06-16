using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Sign In screen controller with error messages and password visibility toggle
/// Requires AuthApiClient to be present in the same scene
/// </summary>
public class SignInController : MonoBehaviour
{
    [Header("API & Config")]
    [SerializeField] private AuthApiClient api;
    [SerializeField] private GameConfig gameConfig;

    [Header("Input Fields")]
    [SerializeField] private TMP_InputField emailInput;
    [SerializeField] private TMP_InputField passwordInput;

    [Header("UI Elements")]
    [SerializeField] private Button submitButton;
    [SerializeField] private TMP_Text errorMessageText;
    [SerializeField] private Button showPasswordButton;
    [SerializeField] private Image showPasswordIcon;

    [Header("Password Visibility Icons")]
    [SerializeField] private Sprite showIcon;  // Eye icon (open)
    [SerializeField] private Sprite hideIcon;  // Eye icon (closed)

    private bool isPasswordVisible = false;
    private ILocalizationService localizationService;
    private string currentErrorMessage;

    private void Awake()
    {
        localizationService = UnityLocalizationService.Instance;

        if (gameConfig == null)
        {
            gameConfig = Resources.Load<GameConfig>("GameConfig");
        }

        // Setup password input to show asterisks
        if (passwordInput != null)
        {
            passwordInput.contentType = TMP_InputField.ContentType.Password;
        }

        // Hide error message initially
        if (errorMessageText != null)
        {
            errorMessageText.gameObject.SetActive(false);
        }

        // Setup show/hide password button
        if (showPasswordButton != null)
        {
            showPasswordButton.onClick.AddListener(TogglePasswordVisibility);
            UpdatePasswordButtonText();
        }
    }

    private void OnEnable()
    {
        if (localizationService == null)
            localizationService = UnityLocalizationService.Instance;

        localizationService.LanguageChanged += HandleLanguageChanged;
    }

    private void OnDisable()
    {
        if (localizationService != null)
            localizationService.LanguageChanged -= HandleLanguageChanged;
    }

    private void OnDestroy()
    {
        // Cleanup button listener
        if (showPasswordButton != null)
        {
            showPasswordButton.onClick.RemoveListener(TogglePasswordVisibility);
        }
    }

    /// <summary>
    /// Handles the login button click - authenticates user and navigates to planet selection
    /// </summary>
    public void OnClickConnect()
    {
        if (api == null)
        {
            Debug.LogError("[SignInController] AuthApiClient is not assigned in Inspector!");
            ShowError("Technical error, please try again");
            return;
        }

        // Clear previous error
        HideError();

        string email = emailInput.text.Trim();
        string pass = passwordInput.text;

        // Basic validation
        if (string.IsNullOrEmpty(email))
        {
            ShowError("Please enter email address");
            return;
        }

        if (string.IsNullOrEmpty(pass))
        {
            ShowError("Please enter password");
            return;
        }

        if (submitButton != null) submitButton.interactable = false;

        StartCoroutine(api.Login(email, pass,
            onSuccess: (resp) =>
            {
                HideError();

                if (AppSession.Instance == null)
                {
                    Debug.LogError("[SignIn] AppSession.Instance is null — cannot save tokens!");
                    ShowError("Session error, please restart the app");
                    return;
                }

                AppSession.Instance.SetTokens(resp.accessToken, resp.refreshToken);
                AppSession.Instance.SetUser(resp.user?.id, resp.user?.userName, resp.user?.selectedAvatar);

                if (SceneLoader.Instance == null)
                {
                    Debug.LogError("[SignIn] SceneLoader.Instance is null — falling back to direct scene load");
                    UnityEngine.SceneManagement.SceneManager.LoadScene(gameConfig != null ? gameConfig.planetSceneIndex : 5);
                    return;
                }

                if (gameConfig == null)
                {
                    Debug.LogError("[SignIn] gameConfig is null — falling back to hardcoded scene index");
                    SceneLoader.Instance.LoadScene(5);
                    return;
                }

                SceneLoader.Instance.LoadScene(gameConfig.planetSceneIndex);
            },
            onError: (err) =>
            {
                if (submitButton != null) submitButton.interactable = true;
                Debug.LogError($"[SignIn] Login failed: {err}");

                if (err.Contains("401") || err.Contains("Unauthorized") || err.Contains("Invalid credentials"))
                {
                    ShowError("Invalid email or password");
                }
                else if (err.Contains("network") || err.Contains("connection"))
                {
                    ShowError("Connection error, please try again");
                }
                else
                {
                    ShowError("Login failed, please try again");
                }
            }
        ));
    }

    /// <summary>
    /// Toggles password visibility between asterisks and plain text
    /// </summary>
    public void TogglePasswordVisibility()
    {
        if (passwordInput == null) return;

        isPasswordVisible = !isPasswordVisible;

        if (isPasswordVisible)
        {
            passwordInput.contentType = TMP_InputField.ContentType.Standard;
        }
        else
        {
            passwordInput.contentType = TMP_InputField.ContentType.Password;
        }

        // Force update the input field
        passwordInput.ForceLabelUpdate();

        UpdatePasswordButtonText();
    }

    private void UpdatePasswordButtonText()
    {
        if (showPasswordIcon != null)
        {
            showPasswordIcon.sprite = isPasswordVisible ? hideIcon : showIcon;
        }
    }

    private void ShowError(string message)
    {
        currentErrorMessage = message;

        if (errorMessageText != null)
        {
            errorMessageText.text = LocalizeMessage(message);
            errorMessageText.gameObject.SetActive(true);
        }
    }

    private void HideError()
    {
        currentErrorMessage = string.Empty;

        if (errorMessageText != null)
        {
            errorMessageText.gameObject.SetActive(false);
        }
    }

    private void HandleLanguageChanged(string languageCode)
    {
        if (errorMessageText == null || !errorMessageText.gameObject.activeSelf || string.IsNullOrEmpty(currentErrorMessage))
            return;

        errorMessageText.text = LocalizeMessage(currentErrorMessage);
    }

    private string LocalizeMessage(string message)
    {
        return localizationService != null
            ? localizationService.GetTextBySource(message)
            : message;
    }
}
