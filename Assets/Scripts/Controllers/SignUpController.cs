using UnityEngine;
using TMPro;

/// <summary>
/// Sign Up screen controller with error messages and password confirmation
/// Requires AuthApiClient to be present in the same scene
/// </summary>
public class SignUpController : MonoBehaviour
{
    [Header("API & Config")]
    [SerializeField] private AuthApiClient api;
    [SerializeField] private GameConfig gameConfig;

    [Header("Input Fields")]
    [SerializeField] private TMP_InputField nameInput;
    [SerializeField] private TMP_InputField userNameInput;
    [SerializeField] private TMP_InputField emailInput;
    [SerializeField] private TMP_InputField passwordInput;
    [SerializeField] private TMP_InputField confirmPasswordInput;

    [Header("UI Elements")]
    [SerializeField] private TMP_Text errorMessageText;
    [SerializeField] private TMP_Text successMessageText;
    private void Awake()
    {
        if (gameConfig == null)
        {
            gameConfig = Resources.Load<GameConfig>("GameConfig");
        }

        // Setup password inputs to show asterisks
        if (passwordInput != null)
        {
            passwordInput.contentType = TMP_InputField.ContentType.Password;
        }
        if (confirmPasswordInput != null)
        {
            confirmPasswordInput.contentType = TMP_InputField.ContentType.Password;
        }

        // Hide messages initially
        if (errorMessageText != null)
        {
            errorMessageText.gameObject.SetActive(false);
        }
        if (successMessageText != null)
        {
            successMessageText.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Handles the sign up button click - registers new user and navigates to sign in
    /// </summary>
    public void OnClickSignUp()
    {
        if (api == null)
        {
            Debug.LogError("[SignUpController] AuthApiClient is not assigned in Inspector!");
            ShowError("Technical error, please try again");
            return;
        }

        // Clear previous messages
        HideError();
        HideSuccess();

        var name = nameInput.text.Trim();
        var userName = userNameInput.text.Trim();
        var email = emailInput.text.Trim();
        var pass = passwordInput.text;
        var confirmPass = confirmPasswordInput.text;

        // Validation
        if (string.IsNullOrEmpty(name))
        {
            ShowError("Please enter full name");
            return;
        }

        if (string.IsNullOrEmpty(userName))
        {
            ShowError("Please enter username");
            return;
        }

        if (string.IsNullOrEmpty(email))
        {
            ShowError("Please enter email address");
            return;
        }

        if (!IsValidEmail(email))
        {
            ShowError("Invalid email format");
            return;
        }

        if (string.IsNullOrEmpty(pass))
        {
            ShowError("Please enter password");
            return;
        }

        if (pass.Length < 6)
        {
            ShowError("Password must be at least 6 characters");
            return;
        }

        if (string.IsNullOrEmpty(confirmPass))
        {
            ShowError("Please confirm your password");
            return;
        }

        if (pass != confirmPass)
        {
            ShowError("Passwords do not match");
            return;
        }

        Debug.Log($"[SignUp] Attempting registration for email: {email}");

        StartCoroutine(api.Register(name, email, userName, pass,
            onSuccess: (resp) =>
            {
                Debug.Log("[SignUp] Registration successful!");
                ShowSuccess("Registration successful! Redirecting to login...");

                // Wait a bit to show success message before navigating
                StartCoroutine(NavigateAfterDelay(1.5f));
            },
            onError: (err) =>
            {
                Debug.LogError($"[SignUp] Registration failed: {err}");

                // Parse error and show user-friendly message
                if (err.Contains("409") || err.Contains("already exists") || err.Contains("duplicate"))
                {
                    ShowError("Email or username already exists");
                }
                else if (err.Contains("400") || err.Contains("validation"))
                {
                    ShowError("Invalid details, please check and try again");
                }
                else if (err.Contains("network") || err.Contains("connection"))
                {
                    ShowError("Connection error, please try again");
                }
                else
                {
                    ShowError("Registration failed, please try again");
                }
            }
        ));
    }

    private System.Collections.IEnumerator NavigateAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (SceneLoader.Instance != null && gameConfig != null)
        {
            SceneLoader.Instance.LoadScene(gameConfig.signInSceneIndex);
        }
    }

    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    private void ShowError(string message)
    {
        if (errorMessageText != null)
        {
            errorMessageText.text = message;
            errorMessageText.gameObject.SetActive(true);
        }
    }

    private void HideError()
    {
        if (errorMessageText != null)
        {
            errorMessageText.gameObject.SetActive(false);
        }
    }

    private void ShowSuccess(string message)
    {
        if (successMessageText != null)
        {
            successMessageText.text = message;
            successMessageText.gameObject.SetActive(true);
        }
    }

    private void HideSuccess()
    {
        if (successMessageText != null)
        {
            successMessageText.gameObject.SetActive(false);
        }
    }
}
