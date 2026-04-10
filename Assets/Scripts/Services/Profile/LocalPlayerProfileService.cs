using System;
using UnityEngine;

public class LocalPlayerProfileService : MonoBehaviour, IPlayerProfileService
{
    [SerializeField] private int selectedAvatarIndex = 0;

    private PlayerProfileDto profile;

    private void Awake()
    {
        profile = new PlayerProfileDto
        {
            user = new UserDto
            {
                id = "2AA7T",
                name = "Player23",
                userName = "player23",
                email = "player23@example.com"
            },
            selectedAvatarIndex = selectedAvatarIndex
        };
    }

    public PlayerProfileDto GetProfile() => profile;

    public void SetSelectedAvatar(int avatarIndex)
    {
        if (profile != null)
            profile.selectedAvatarIndex = avatarIndex;
    }

    public void LoadProfileFromServer(Action onSuccess, Action<string> onError)
    {
        onSuccess?.Invoke();
    }

    public void UpdateProfile(UpdateProfileRequestDto request, Action<UserDto> onSuccess, Action<string> onError)
    {
        if (profile?.user == null) return;

        if (!string.IsNullOrEmpty(request.name)) profile.user.name = request.name;
        if (!string.IsNullOrEmpty(request.userName)) profile.user.userName = request.userName;
        if (!string.IsNullOrEmpty(request.email)) profile.user.email = request.email;

        onSuccess?.Invoke(profile.user);
    }
}
