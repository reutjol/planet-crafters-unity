public interface IPlayerProfileService
{
    PlayerProfileDto GetProfile();
    void SetSelectedAvatar(int avatarIndex);
}