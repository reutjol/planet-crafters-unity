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
                email = "player23@example.com"
            },
            selectedAvatarIndex = selectedAvatarIndex
        };
    }

    public PlayerProfileDto GetProfile()
    {
        return profile;
    }

    public void SetSelectedAvatar(int avatarIndex)
    {
        if (profile == null)
            return;

        profile.selectedAvatarIndex = avatarIndex;
    }
}