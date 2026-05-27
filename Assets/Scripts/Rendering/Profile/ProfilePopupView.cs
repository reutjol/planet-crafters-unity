using System;
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

    public void BindProfile(PlayerProfileDto profile, Sprite[] avatarSprites, Action<int> onAvatarClicked)
    {
        if (profile == null) return;

        if (profile.user != null)
        {
            SetFieldValue(fullNameText, fullNameInput, profile.user.name);
            SetFieldValue(userNameText, userNameInput, profile.user.userName);
            SetFieldValue(emailText, emailInput, profile.user.email);
        }

        if (avatarSprites == null || avatarSprites.Length == 0) return;

        int safeIndex = Mathf.Clamp(profile.selectedAvatarIndex, 0, avatarSprites.Length - 1);

        if (profileImage != null)
            profileImage.sprite = avatarSprites[safeIndex];

        if (avatarSlots == null) return;

        for (int i = 0; i < avatarSlots.Length; i++)
        {
            if (avatarSlots[i] == null || i >= avatarSprites.Length) continue;
            avatarSlots[i].Setup(i, avatarSprites[i], i == safeIndex, onAvatarClicked);
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

    public void UpdateSelectedAvatar(int selectedIndex, Sprite[] avatarSprites)
    {
        if (avatarSprites == null || avatarSprites.Length == 0) return;

        int safeIndex = Mathf.Clamp(selectedIndex, 0, avatarSprites.Length - 1);

        if (profileImage != null)
            profileImage.sprite = avatarSprites[safeIndex];

        if (avatarSlots == null) return;

        for (int i = 0; i < avatarSlots.Length; i++)
        {
            if (avatarSlots[i] != null)
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

    private void SetEditMode(TMP_Text label, TMP_InputField input, Button editBtn, Button saveBtn, bool editing)
    {
        if (label != null) label.gameObject.SetActive(!editing);
        if (input != null) input.gameObject.SetActive(editing);
        if (editBtn != null) editBtn.gameObject.SetActive(!editing);
        if (saveBtn != null) saveBtn.gameObject.SetActive(editing);
    }

    private void SetFieldValue(TMP_Text label, TMP_InputField input, string value)
    {
        if (label != null) label.text = value ?? "";
        if (input != null) input.text = value ?? "";
    }
}
