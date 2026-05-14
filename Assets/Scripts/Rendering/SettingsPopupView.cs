using System;
using UnityEngine;
using UnityEngine.UI;

public class SettingsPopupView : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Profile")]
    [SerializeField] private Image profileImage;

    [Header("Buttons")]
    [SerializeField] private Button closeButton;
    [SerializeField] private Button logoutButton;
    [SerializeField] private Button languageButton;
    [SerializeField] private Button privacyPolicyButton;
    [SerializeField] private Button termsOfUseButton;

    public event Action CloseClicked;
    public event Action LogoutClicked;
    public event Action LanguageClicked;
    public event Action PrivacyPolicyClicked;
    public event Action TermsOfUseClicked;

    public bool IsVisible => gameObject.activeSelf;

    private void Awake()
    {
        closeButton?.onClick.AddListener(() => CloseClicked?.Invoke());
        logoutButton?.onClick.AddListener(() => LogoutClicked?.Invoke());
        languageButton?.onClick.AddListener(() => LanguageClicked?.Invoke());
        privacyPolicyButton?.onClick.AddListener(() => PrivacyPolicyClicked?.Invoke());
        termsOfUseButton?.onClick.AddListener(() => TermsOfUseClicked?.Invoke());

        Hide();
    }

    public void SetProfileImage(Sprite avatarSprite)
    {
        if (profileImage == null || avatarSprite == null)
            return;

        profileImage.sprite = avatarSprite;
    }

    public void Show()
    {
        gameObject.SetActive(true);

        if (canvasGroup == null)
            return;

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
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