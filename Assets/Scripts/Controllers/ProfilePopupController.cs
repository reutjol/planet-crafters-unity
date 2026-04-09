using UnityEngine;

public class ProfilePopupController : MonoBehaviour, IClosablePopup
{
    [SerializeField] private ProfilePopupView view;
    [SerializeField] private LocalPlayerProfileService profileService;
    [SerializeField] private Sprite[] avatarSprites;
    [SerializeField] private TopBarProfileController topBarProfileController;

    private IPlayerProfileService profileServiceAbstraction;
    private PlayerProfileDto currentProfile;

    public bool IsOpen => view != null && view.IsVisible;

    private void Awake()
    {
        profileServiceAbstraction = profileService;

        if (view == null)
        {
            Debug.LogError("ProfilePopupController: view reference is missing.");
            return;
        }

        if (profileServiceAbstraction == null)
        {
            Debug.LogError("ProfilePopupController: profile service reference is missing.");
            return;
        }

        if (view.CloseButton != null)
            view.CloseButton.onClick.AddListener(ClosePopup);

        view.Hide();
        RefreshTopBarAvatar();
    }

    public void OpenPopup()
    {
        currentProfile = profileServiceAbstraction.GetProfile();

        if (currentProfile == null)
        {
            Debug.LogWarning("ProfilePopupController: profile data is null.");
            return;
        }

        view.BindProfile(currentProfile, avatarSprites, OnAvatarSelected);
        view.Show();
    }

    public void ClosePopup()
    {
        if (view != null)
            view.Hide();
    }

    private void OnAvatarSelected(int avatarIndex)
    {
        profileServiceAbstraction.SetSelectedAvatar(avatarIndex);

        if (currentProfile != null)
            currentProfile.selectedAvatarIndex = avatarIndex;

        if (view != null)
            view.UpdateSelectedAvatar(avatarIndex, avatarSprites);

        RefreshTopBarAvatar();
    }

    private void RefreshTopBarAvatar()
    {
        if (topBarProfileController == null || avatarSprites == null || avatarSprites.Length == 0)
            return;

        PlayerProfileDto profile = profileServiceAbstraction.GetProfile();
        if (profile == null)
            return;

        int safeIndex = Mathf.Clamp(profile.selectedAvatarIndex, 0, avatarSprites.Length - 1);
        topBarProfileController.SetAvatar(avatarSprites[safeIndex]);
    }
}