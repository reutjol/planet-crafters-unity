using UnityEngine;

public class ProfilePopupController : MonoBehaviour, IClosablePopup
{
    [SerializeField] private ProfilePopupView view;
    [SerializeField] private ServerPlayerProfileService profileService;
    [SerializeField] private TopBarProfileController topBarProfileController;

    private IPlayerProfileService service;
    private PlayerProfileDto currentProfile;
    private Sprite[] avatarSprites;

    public bool IsOpen => view != null && view.IsVisible;

    private void Awake()
    {
        avatarSprites = Resources.LoadAll<Sprite>("Sprites/avatar Sprites");

        service = profileService;

        if (view == null) { Debug.LogError("[ProfilePopupController] view is missing"); return; }
        if (service == null) { Debug.LogError("[ProfilePopupController] profileService is missing"); return; }

        view.CloseButton?.onClick.AddListener(ClosePopup);

        view.OnFullNameSave += name => SaveField(new UpdateProfileRequestDto { name = name });
        view.OnUserNameSave += userName => SaveField(new UpdateProfileRequestDto { userName = userName });
        view.OnEmailSave    += email => SaveField(new UpdateProfileRequestDto { email = email });

        view.Hide();
        RefreshTopBarAvatar();
    }

    public void OpenPopup()
    {
        service.LoadProfileFromServer(
            onSuccess: () =>
            {
                currentProfile = service.GetProfile();
                if (currentProfile == null) { Debug.LogWarning("[ProfilePopupController] profile is null"); return; }
                view.BindProfile(currentProfile, avatarSprites, OnAvatarSelected);
                PopupManager.OnPopupOpened();
                view.Show();
            },
            onError: err => Debug.LogError($"[ProfilePopupController] Failed to load profile: {err}")
        );
    }

    public void ClosePopup()
    {
        PopupManager.OnPopupClosed();
        view?.Hide();
    }

    private void SaveField(UpdateProfileRequestDto request)
    {
        service.UpdateProfile(
            request,
            onSuccess: user =>
            {
                view.UpdateFieldValue("name", user.name);
                view.UpdateFieldValue("userName", user.userName);
                view.UpdateFieldValue("email", user.email);
            },
            onError: err => Debug.LogError($"[ProfilePopupController] Update failed: {err}")
        );
    }

    private void OnAvatarSelected(int avatarIndex)
    {
        service.SetSelectedAvatar(avatarIndex);

        if (currentProfile != null)
            currentProfile.selectedAvatarIndex = avatarIndex;

        view?.UpdateSelectedAvatar(avatarIndex, avatarSprites);
        RefreshTopBarAvatar();
    }

    private void RefreshTopBarAvatar()
    {
        if (topBarProfileController == null || avatarSprites == null || avatarSprites.Length == 0) return;

        PlayerProfileDto profile = service?.GetProfile();
        if (profile == null) return;

        int safeIndex = Mathf.Clamp(profile.selectedAvatarIndex, 0, avatarSprites.Length - 1);
        topBarProfileController.SetAvatar(avatarSprites[safeIndex]);
    }
}
