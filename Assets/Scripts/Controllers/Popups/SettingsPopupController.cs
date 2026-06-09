using UnityEngine;

public class SettingsPopupController : MonoBehaviour, IClosablePopup
{
    [Header("View")]
    [SerializeField] private SettingsPopupView view;

    [Header("Other Popups")]
    [SerializeField] private ProfilePopupController profilePopupController;

    [Header("Profile")]
    [SerializeField] private ServerPlayerProfileService profileService;

    [Header("Existing Logout")]
    [SerializeField] private LogoutButton logoutButton;

    [Header("Localization")]
    [SerializeField] private UnityLocalizationService localizationService;

    [Header("External Links")]
    [SerializeField] private string privacyPolicyUrl = "";
    [SerializeField] private string termsOfUseUrl = "";

    private IPlayerProfileService playerProfileService;
    private System.Collections.Generic.Dictionary<string, Sprite> avatarSpriteMap;

    public bool IsOpen => view != null && view.IsVisible;

    private void Awake()
    {
        playerProfileService = profileService;
        EnsureAvatarSpriteMap();

        if (profilePopupController == null)
            profilePopupController = FindObjectOfType<ProfilePopupController>(true);

        if (localizationService == null)
            localizationService = UnityLocalizationService.Instance;

        if (view == null)
            Debug.LogError("[SettingsPopupController] View is missing.");

        if (playerProfileService == null)
            Debug.LogError("[SettingsPopupController] Profile service is missing.");

        if (logoutButton == null)
            Debug.LogError("[SettingsPopupController] LogoutButton is missing.");
    }

    private void OnEnable()
    {
        if (view == null)
            return;

        view.CloseClicked += ClosePopup;
        view.LogoutClicked += Logout;
        view.LanguageClicked += OpenLanguage;
        view.PrivacyPolicyClicked += OpenPrivacyPolicy;
        view.TermsOfUseClicked += OpenTermsOfUse;
    }

    private void OnDisable()
    {
        if (view == null)
            return;

        view.CloseClicked -= ClosePopup;
        view.LogoutClicked -= Logout;
        view.LanguageClicked -= OpenLanguage;
        view.PrivacyPolicyClicked -= OpenPrivacyPolicy;
        view.TermsOfUseClicked -= OpenTermsOfUse;
    }

    public void OpenPopup()
    {
        if (view == null)
            return;

        profilePopupController?.ClosePopup();

        LoadProfileImage();

        if (!IsOpen)
            PopupManager.OnPopupOpened();

        view.Show();
    }

    public void ClosePopup()
    {
        if (!IsOpen)
            return;

        PopupManager.OnPopupClosed();
        view?.Hide();
    }

    private void EnsureAvatarSpriteMap()
    {
        if (avatarSpriteMap != null) return;
        var allSprites = Resources.LoadAll<Sprite>("Sprites/avatar Sprites");
        avatarSpriteMap = new System.Collections.Generic.Dictionary<string, Sprite>();
        foreach (var s in allSprites)
            avatarSpriteMap[s.name.ToLower()] = s;
    }

    private void LoadProfileImage()
    {
        // Use session avatar first (set at login) — always immediate
        var sessionAvatar = AppSession.Instance?.SelectedAvatar;
        if (!string.IsNullOrEmpty(sessionAvatar))
        {
            ApplyAvatarSprite(sessionAvatar);
            return;
        }

        // Fall back to cached profile
        var cached = playerProfileService?.GetProfile();
        if (cached?.user != null)
        {
            ApplyAvatarSprite(cached.user.selectedAvatar);
            return;
        }

        // Last resort: load from server
        playerProfileService?.LoadProfileFromServer(
            onSuccess: () =>
            {
                var profile = playerProfileService.GetProfile();
                var avatarId = profile?.user?.selectedAvatar;
                AppSession.Instance?.SetSelectedAvatar(avatarId);
                // Only update view if popup is still open
                if (IsOpen)
                    ApplyAvatarSprite(avatarId);
            },
            onError: error =>
            {
                Debug.LogError($"[SettingsPopupController] Failed to load profile: {error}");
            }
        );
    }

    private void ApplyAvatarSprite(string avatarId)
    {
        if (string.IsNullOrEmpty(avatarId))
            avatarId = "avatar";

        EnsureAvatarSpriteMap();

        if (avatarSpriteMap.TryGetValue(avatarId.ToLower(), out var sprite) && view != null)
            view.SetProfileImage(sprite);
    }

    private void Logout()
    {
        ClosePopup();

        if (logoutButton == null)
        {
            Debug.LogError("[SettingsPopupController] Cannot logout because LogoutButton is missing.");
            return;
        }

        logoutButton.OnClick();
    }

    private void OpenLanguage()
    {
        if (localizationService == null)
            localizationService = UnityLocalizationService.Instance;

        localizationService.ToggleLanguage();
    }

    private void OpenPrivacyPolicy()
    {
        if (!string.IsNullOrWhiteSpace(privacyPolicyUrl))
            Application.OpenURL(privacyPolicyUrl);
    }

    private void OpenTermsOfUse()
    {
        if (!string.IsNullOrWhiteSpace(termsOfUseUrl))
            Application.OpenURL(termsOfUseUrl);
    }
}
