using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProfilePopupView : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Button closeButton;

    [Header("Header")]
    [SerializeField] private TMP_Text titleText;

    [Header("Profile Info")]
    [SerializeField] private Image profileImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text idText;

    [Header("Avatar Grid")]
    [SerializeField] private AvatarSlotView[] avatarSlots;

    public Button CloseButton => closeButton;
    public bool IsVisible => gameObject.activeSelf;

    public void BindProfile(PlayerProfileDto profile, Sprite[] avatarSprites, Action<int> onAvatarClicked)
    {
        if (profile == null)
            return;

        if (titleText != null)
            titleText.text = "Player Profile";

        if (profile.user != null)
        {
            if (nameText != null)
                nameText.text = profile.user.name;

            if (idText != null)
                idText.text = $"ID: {profile.user.id}";
        }

        if (avatarSprites == null || avatarSprites.Length == 0)
            return;

        int safeIndex = Mathf.Clamp(profile.selectedAvatarIndex, 0, avatarSprites.Length - 1);

        if (profileImage != null)
            profileImage.sprite = avatarSprites[safeIndex];

        if (avatarSlots == null)
            return;

        for (int i = 0; i < avatarSlots.Length; i++)
        {
            if (avatarSlots[i] == null || i >= avatarSprites.Length)
                continue;

            avatarSlots[i].Setup(i, avatarSprites[i], i == safeIndex, onAvatarClicked);
        }
    }

    public void UpdateSelectedAvatar(int selectedIndex, Sprite[] avatarSprites)
    {
        if (avatarSprites == null || avatarSprites.Length == 0)
            return;

        int safeIndex = Mathf.Clamp(selectedIndex, 0, avatarSprites.Length - 1);

        if (profileImage != null)
            profileImage.sprite = avatarSprites[safeIndex];

        if (avatarSlots == null)
            return;

        for (int i = 0; i < avatarSlots.Length; i++)
        {
            if (avatarSlots[i] == null)
                continue;

            avatarSlots[i].SetSelected(i == safeIndex);
        }
    }

    public void Show()
    {
        gameObject.SetActive(true);

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
    }

    public void Hide()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        gameObject.SetActive(false);
    }
}