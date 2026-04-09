using UnityEngine;
using UnityEngine.UI;

public class TopBarProfileController : MonoBehaviour
{
    [SerializeField] private Image profileImage;

    public void SetAvatar(Sprite avatarSprite)
    {
        if (profileImage != null)
            profileImage.sprite = avatarSprite;
    }
}