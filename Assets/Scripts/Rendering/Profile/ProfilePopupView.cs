using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProfilePopupView : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Button closeButton;

    [Header("Profile Image")]
    [SerializeField] private Image profileImage;

    [Header("Full Name")]
    [SerializeField] private TMP_Text fullNameText;
    [SerializeField] private TMP_InputField fullNameInput;
    [SerializeField] private Button fullNameEditButton;
    [SerializeField] private Button fullNameSaveButton;

    [Header("Username")]
    [SerializeField] private TMP_Text userNameText;
    [SerializeField] private TMP_InputField userNameInput;
    [SerializeField] private Button userNameEditButton;
    [SerializeField] private Button userNameSaveButton;

    [Header("Email")]
    [SerializeField] private TMP_Text emailText;
    [SerializeField] private TMP_InputField emailInput;
    [SerializeField] private Button emailEditButton;
    [SerializeField] private Button emailSaveButton;

    [Header("Avatar Grid")]
    [SerializeField] private AvatarSlotView[] avatarSlots;

    public Button CloseButton => closeButton;
    public bool IsVisible => gameObject.activeSelf;

    public event Action<string> OnFullNameSave;
    public event Action<string> OnUserNameSave;
    public event Action<string> OnEmailSave;

    private void Awake()
    {
        fullNameEditButton?.onClick.AddListener(() => SetEditMode(fullNameText, fullNameInput, fullNameEditButton, fullNameSaveButton, true));
        fullNameSaveButton?.onClick.AddListener(() => { OnFullNameSave?.Invoke(fullNameInput.text); SetEditMode(fullNameText, fullNameInput, fullNameEditButton, fullNameSaveButton, false); });

        userNameEditButton?.onClick.AddListener(() => SetEditMode(userNameText, userNameInput, userNameEditButton, userNameSaveButton, true));
        userNameSaveButton?.onClick.AddListener(() => { OnUserNameSave?.Invoke(userNameInput.text); SetEditMode(userNameText, userNameInput, userNameEditButton, userNameSaveButton, false); });

        emailEditButton?.onClick.AddListener(() => SetEditMode(emailText, emailInput, emailEditButton, emailSaveButton, true));
        emailSaveButton?.onClick.AddListener(() => { OnEmailSave?.Invoke(emailInput.text); SetEditMode(emailText, emailInput, emailEditButton, emailSaveButton, false); });
    }

    public void BindProfile(PlayerProfileDto profile, Sprite[] avatarSprites, string[] avatarIds,
        List<string> ownedAvatars, ShopProfileDto shopProfile, Action<string> onAvatarClicked)
    {
        if (profile == null) return;

        if (profile.user != null)
        {
            SetFieldValue(fullNameText, fullNameInput, profile.user.name);
            SetFieldValue(userNameText, userNameInput, profile.user.userName);
            SetFieldValue(emailText, emailInput, profile.user.email);
        }

        if (avatarSprites == null || avatarSprites.Length == 0) return;

        string selected = profile.user?.selectedAvatar ?? "avatar";

        if (profileImage != null)
        {
            int selIdx = Array.IndexOf(avatarIds, selected);
            if (selIdx >= 0 && selIdx < avatarSprites.Length)
                profileImage.sprite = avatarSprites[selIdx];
        }

        if (avatarSlots == null) return;

        for (int i = 0; i < avatarSlots.Length; i++)
        {
            if (avatarSlots[i] == null || i >= avatarSprites.Length || i >= avatarIds.Length) continue;

            string id = avatarIds[i];
            bool isOwned = ownedAvatars != null && ownedAvatars.Contains(id);
            bool isSelected = id == selected;
            AvatarSlotView.SlotState slotState = AvatarSlotView.SlotState.Owned;
            int stageRequired = 0;

            if (!isOwned && shopProfile?.catalog != null)
            {
                var entry = shopProfile.catalog.Find(c => c.id == id);
                if (entry != null && entry.unlockType == "stage")
                {
                    slotState = AvatarSlotView.SlotState.StageUnlock;
                    stageRequired = entry.stageRequired;
                }
                else
                {
                    slotState = AvatarSlotView.SlotState.Locked;
                }
            }

            avatarSlots[i].Setup(id, avatarSprites[i], slotState, isSelected, onAvatarClicked, stageRequired);
        }
    }

    public void UpdateFieldValue(string fieldName, string newValue)
    {
        switch (fieldName)
        {
            case "name":     SetFieldValue(fullNameText, fullNameInput, newValue); break;
            case "userName": SetFieldValue(userNameText, userNameInput, newValue); break;
            case "email":    SetFieldValue(emailText, emailInput, newValue);       break;
        }
    }

    public void UpdateSelectedAvatar(string selectedId, string[] avatarIds, Sprite[] avatarSprites)
    {
        if (avatarSprites == null || avatarSprites.Length == 0) return;

        int selIdx = Array.IndexOf(avatarIds, selectedId);
        if (profileImage != null && selIdx >= 0 && selIdx < avatarSprites.Length)
            profileImage.sprite = avatarSprites[selIdx];

        if (avatarSlots == null) return;

        for (int i = 0; i < avatarSlots.Length && i < avatarIds.Length; i++)
        {
            if (avatarSlots[i] != null)
                avatarSlots[i].SetSelected(avatarIds[i] == selectedId);
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

    private void SetEditMode(TMP_Text label, TMP_InputField input, Button editBtn, Button saveBtn, bool editing)
    {
        if (label != null) label.gameObject.SetActive(!editing);
        if (input != null) input.gameObject.SetActive(editing);
        if (editBtn != null) editBtn.gameObject.SetActive(!editing);
        if (saveBtn != null) saveBtn.gameObject.SetActive(editing);
    }

    private void SetFieldValue(TMP_Text label, TMP_InputField input, string value)
    {
        bool rtl = ContainsHebrew(value);
        if (label != null)
        {
            label.isRightToLeftText = rtl;
            label.text = value ?? "";
        }
        if (input != null)
        {
            input.textComponent.isRightToLeftText = rtl;
            input.text = value ?? "";
        }
    }

    private static bool ContainsHebrew(string text)
    {
        if (string.IsNullOrEmpty(text)) return false;
        foreach (char c in text)
            if (c >= '֐' && c <= '׿') return true;
        return false;
    }
}
