using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ProfilePopupController : MonoBehaviour, IClosablePopup
{
    private static readonly string[] AvatarIds =
    {
        "avatar", "boy", "girl", "cat", "colorfully",
        "dinosaur", "alien", "mexican", "superman"
    };

    [SerializeField] private ProfilePopupView view;
    [SerializeField] private ServerPlayerProfileService profileService;
    [SerializeField] private TopBarProfileController topBarProfileController;
    [SerializeField] private SettingsPopupController settingsPopupController;

    private IPlayerProfileService service;
    private PlayerProfileDto currentProfile;
    private ShopProfileDto shopProfile;
    private Sprite[] avatarSprites;
    private Dictionary<string, Sprite> avatarSpriteMap;
    private int openRequestId;

    public bool IsOpen => view != null && view.IsVisible;

    private void OnEnable()  => ShopAvatarSlotView.OnAvatarPurchased += HandleAvatarPurchased;
    private void OnDisable() => ShopAvatarSlotView.OnAvatarPurchased -= HandleAvatarPurchased;

    private void HandleAvatarPurchased(string avatarId, int newCoins)
    {
        if (shopProfile != null)
        {
            shopProfile.coins = newCoins;
            if (!shopProfile.ownedAvatars.Contains(avatarId))
                shopProfile.ownedAvatars.Add(avatarId);
        }

        if (!IsOpen) return;

        List<string> owned = shopProfile?.ownedAvatars ?? currentProfile?.user?.ownedAvatars ?? new List<string> { "avatar" };
        view.BindProfile(currentProfile, avatarSprites, AvatarIds, owned, shopProfile, OnAvatarSlotClicked);
    }

    private void Awake()
    {
        var allSprites = Resources.LoadAll<Sprite>("Sprites/avatar Sprites");
        avatarSpriteMap = allSprites.ToDictionary(s => s.name.ToLower(), s => s);
        avatarSprites = AvatarIds.Select(id =>
            avatarSpriteMap.TryGetValue(id.ToLower(), out var sp) ? sp : null
        ).ToArray();
        service = profileService;

        if (settingsPopupController == null)
            settingsPopupController = FindObjectOfType<SettingsPopupController>(true);

        if (topBarProfileController == null)
            topBarProfileController = FindObjectOfType<TopBarProfileController>(true);

        if (view == null) { Debug.LogError("[ProfilePopupController] view is missing"); return; }
        if (service == null) { Debug.LogError("[ProfilePopupController] profileService is missing"); return; }

        view.CloseButton?.onClick.AddListener(ClosePopup);

        view.OnFullNameSave += name => SaveField(new UpdateProfileRequestDto { name = name });
        view.OnUserNameSave += userName => SaveField(new UpdateProfileRequestDto { userName = userName });
        view.OnEmailSave    += email => SaveField(new UpdateProfileRequestDto { email = email });

        view.Hide();
        RefreshTopBarAvatar();
    }

    private void Start()
    {
        if (service == null) return;
        currentProfile = null;
        shopProfile = null;
        service.LoadProfileFromServer(
            onSuccess: () =>
            {
                currentProfile = service.GetProfile();
                RefreshTopBarAvatar();
            },
            onError: _ => { }
        );
    }

    public void OpenPopup()
    {
        if (view == null || service == null) return;

        int requestId = ++openRequestId;
        settingsPopupController?.ClosePopup();
        bool wasOpen = IsOpen;

        // Show immediately with cached data if available, refresh in background
        if (currentProfile != null)
        {
            ShowPopup(wasOpen);
            RefreshInBackground(requestId);
            return;
        }

        service.LoadProfileFromServer(
            onSuccess: () =>
            {
                if (requestId != openRequestId) return;
                currentProfile = service.GetProfile();
                if (currentProfile == null) return;

                LoadShopProfileThenShow(requestId, wasOpen);
            },
            onError: err => Debug.LogError($"[ProfilePopupController] Failed to load profile: {err}")
        );
    }

    private void RefreshInBackground(int requestId)
    {
        service.LoadProfileFromServer(
            onSuccess: () =>
            {
                if (requestId != openRequestId) return;
                currentProfile = service.GetProfile();
                if (currentProfile == null) return;

                var token = AppSession.Instance?.AccessToken;
                var api = ShopApiClient.GetOrCreate();
                api.StartCoroutine(api.GetProfile(token,
                    profile =>
                    {
                        if (requestId != openRequestId) return;
                        shopProfile = profile;
                        if (IsOpen) ShowPopup(true);
                    },
                    err =>
                    {
                        if (requestId != openRequestId) return;
                        shopProfile = null;
                        if (IsOpen) ShowPopup(true);
                    }
                ));
            },
            onError: err => Debug.LogWarning($"[ProfilePopupController] Background refresh failed: {err}")
        );
    }

    private void LoadShopProfileThenShow(int requestId, bool wasOpen)
    {
        var token = AppSession.Instance?.AccessToken;
        var api = ShopApiClient.GetOrCreate();

        api.StartCoroutine(api.GetProfile(token,
            profile =>
            {
                if (requestId != openRequestId) return;
                shopProfile = profile;
                ShowPopup(wasOpen);
            },
            err =>
            {
                if (requestId != openRequestId) return;
                Debug.LogWarning($"[ProfilePopupController] Could not load shop profile: {err}");
                shopProfile = null;
                ShowPopup(wasOpen);
            }
        ));
    }

    private void ShowPopup(bool wasOpen)
    {
        List<string> owned = shopProfile?.ownedAvatars ?? currentProfile?.user?.ownedAvatars ?? new List<string> { "avatar" };
        view.BindProfile(currentProfile, avatarSprites, AvatarIds, owned, shopProfile, OnAvatarSlotClicked);
        RefreshTopBarAvatar();

        if (!wasOpen) PopupManager.OnPopupOpened();
        view.Show();
    }

    public void ClosePopup()
    {
        openRequestId++;
        if (!IsOpen) return;
        PopupManager.OnPopupClosed();
        view?.Hide();
    }

    private void SaveField(UpdateProfileRequestDto request)
    {
        service.UpdateProfile(request,
            onSuccess: user =>
            {
                view.UpdateFieldValue("name", user.name);
                view.UpdateFieldValue("userName", user.userName);
                view.UpdateFieldValue("email", user.email);
            },
            onError: err => Debug.LogError($"[ProfilePopupController] Update failed: {err}")
        );
    }

    private void OnAvatarSlotClicked(string avatarId)
    {
        List<string> owned = shopProfile?.ownedAvatars ?? new List<string> { "avatar" };

        if (!owned.Contains(avatarId))
            return;

        // Select the avatar
        var token = AppSession.Instance?.AccessToken;
        var api = ShopApiClient.GetOrCreate();

        api.StartCoroutine(api.SetAvatar(token, avatarId,
            result =>
            {
                if (!result.success) return;

                if (currentProfile?.user != null)
                    currentProfile.user.selectedAvatar = result.selectedAvatar;

                if (shopProfile != null)
                    shopProfile.selectedAvatar = result.selectedAvatar;

                AppSession.Instance?.SetSelectedAvatar(result.selectedAvatar);

                List<string> ownedList = shopProfile?.ownedAvatars ?? new List<string> { "avatar" };
                view.UpdateSelectedAvatar(result.selectedAvatar, AvatarIds, avatarSprites);
                RefreshTopBarAvatar();
            },
            err => Debug.LogError($"[ProfilePopupController] SetAvatar failed: {err}")
        ));
    }

    private void RefreshTopBarAvatar()
    {
        if (topBarProfileController == null || avatarSpriteMap == null) return;

        string selectedId = shopProfile?.selectedAvatar
            ?? currentProfile?.user?.selectedAvatar
            ?? AppSession.Instance?.SelectedAvatar
            ?? "avatar";
        if (avatarSpriteMap.TryGetValue(selectedId.ToLower(), out var sprite))
            topBarProfileController.SetAvatar(sprite);
    }
}
