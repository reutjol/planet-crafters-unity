using System;
using UnityEngine;

public class ServerPlayerProfileService : MonoBehaviour, IPlayerProfileService
{
    private PlayerProfileDto profile;

    private void Awake()
    {
        profile = new PlayerProfileDto { selectedAvatarIndex = 0 };
    }

    public PlayerProfileDto GetProfile() => profile;

    public void SetSelectedAvatar(int avatarIndex)
    {
        if (profile != null)
            profile.selectedAvatarIndex = avatarIndex;
    }

    public void LoadProfileFromServer(Action onSuccess, Action<string> onError)
    {
        var client = PlayerProfileApiClient.Instance;
        if (client == null) { onError?.Invoke("PlayerProfileApiClient not found"); return; }

        client.StartCoroutine(client.GetMyProfile(
            AppSession.Instance?.AccessToken,
            user =>
            {
                if (profile == null) profile = new PlayerProfileDto();
                profile.user = user;
                onSuccess?.Invoke();
            },
            onError
        ));
    }

    public void UpdateProfile(UpdateProfileRequestDto request, Action<UserDto> onSuccess, Action<string> onError)
    {
        var client = PlayerProfileApiClient.Instance;
        if (client == null) { onError?.Invoke("PlayerProfileApiClient not found"); return; }

        client.StartCoroutine(client.UpdateMyProfile(
            AppSession.Instance?.AccessToken,
            request,
            user =>
            {
                if (profile == null) profile = new PlayerProfileDto();
                profile.user = user;
                onSuccess?.Invoke(user);
            },
            onError
        ));
    }
}
