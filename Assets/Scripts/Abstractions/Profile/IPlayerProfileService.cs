using System;

public interface IPlayerProfileService
{
    PlayerProfileDto GetProfile();
    void SetSelectedAvatar(int avatarIndex);
    void LoadProfileFromServer(Action onSuccess, Action<string> onError);
    void UpdateProfile(UpdateProfileRequestDto request, Action<UserDto> onSuccess, Action<string> onError);
}
