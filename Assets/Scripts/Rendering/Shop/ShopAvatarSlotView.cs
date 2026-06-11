using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopAvatarSlotView : MonoBehaviour
{
    public static event Action<string, int> OnAvatarPurchased; // avatarId, newCoins

    [SerializeField] private Image    avatarImage;
    [SerializeField] private TMP_Text nameLabel;
    [SerializeField] private TMP_Text statusLabel;
    [SerializeField] private GameObject priceObject;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private Button   actionButton;

    [SerializeField] private string avatarId;

    private ShopProfileDto profile;
    private UnityLocalizationService _locSvc;

    private void Start()
    {
        if (actionButton != null)
            actionButton.onClick.AddListener(OnBuyClicked);
    }

    private void OnEnable()
    {
        _locSvc = UnityLocalizationService.Instance;
        if (_locSvc != null) _locSvc.LanguageChanged += OnLanguageChanged;
    }

    private void OnDisable()
    {
        if (_locSvc != null) _locSvc.LanguageChanged -= OnLanguageChanged;
        _locSvc = null;
    }

    private void OnLanguageChanged(string _) => LoadAvatarVisuals();

    public void Setup(ShopProfileDto shopProfile)
    {
        profile = shopProfile;

        LoadAvatarVisuals();

        bool owned = shopProfile?.ownedAvatars != null && shopProfile.ownedAvatars.Contains(avatarId);
        if (owned)
        {
            ApplyOwnedState();
            return;
        }

        var entry = shopProfile?.catalog?.Find(c => c.id == avatarId);

        if (entry != null && entry.unlockType == "stage")
        {
            var svcRtl = UnityLocalizationService.Instance;
            bool isRtl = svcRtl != null && svcRtl.IsRightToLeft;
            string availableText = L("text.available_from_stage", "Available from stage");
            SetStatus(isRtl ? $"{entry.stageRequired} {availableText}" : $"{availableText} {entry.stageRequired}");
            SetButtonState(false);
            DimAvatar(true);
            return;
        }

        // Purchasable
        int price = entry?.price ?? 50;
        if (priceText != null) priceText.text = price.ToString();
        SetStatus("");
        SetButtonState(true);
        DimAvatar(false);
    }

    private void LoadAvatarVisuals()
    {
        if (!string.IsNullOrEmpty(avatarId))
        {
            var sprite = Resources.Load<Sprite>($"Sprites/avatar Sprites/{avatarId}");
            if (avatarImage != null && sprite != null)
                avatarImage.sprite = sprite;

            if (nameLabel != null)
            {
                string displayName = char.ToUpper(avatarId[0]) + avatarId[1..];
                string translatedName = L($"text.avatar_name_{avatarId}", displayName);
                var svc = UnityLocalizationService.Instance;
                bool rtl = svc != null && svc.IsRightToLeft;
                string avatarWord = L("text.avatar", "avatar");
                nameLabel.text = rtl ? $"{avatarWord} {translatedName}" : $"{translatedName} {avatarWord}";
                nameLabel.isRightToLeftText = rtl;
                nameLabel.horizontalAlignment = LocalizedTextView.FlipAlignment(nameLabel.horizontalAlignment, rtl);
            }
        }
    }

    private void ApplyOwnedState()
    {
        SetStatus(L("text.already_owned", "Already owned"));
        if (actionButton != null) actionButton.interactable = false;
        if (priceObject != null) priceObject.SetActive(false);
        DimAvatar(false);
    }

    private void OnBuyClicked()
    {
        if (string.IsNullOrEmpty(avatarId))
        {
            Debug.LogError($"[ShopAvatarSlotView] avatarId is not set on {gameObject.name} — set it in the Inspector");
            SetStatus("Config error");
            return;
        }

        var token = AppSession.Instance?.AccessToken;
        if (string.IsNullOrEmpty(token)) return;

        actionButton.interactable = false;
        StartCoroutine(ShopApiClient.GetOrCreate().BuyAvatar(token, avatarId,
            result =>
            {
                if (!result.success)
                {
                    actionButton.interactable = true;
                    SetStatus(result.reason == "insufficient_coins"
                        ? L("text.not_enough_coins", "Not enough coins")
                        : L("text.purchase_failed", "Purchase failed"));
                    return;
                }

                if (profile != null)
                {
                    profile.coins = result.coins;
                    if (result.ownedAvatars != null)
                        profile.ownedAvatars = result.ownedAvatars;
                }

                ApplyOwnedState();
                UserCoinsDisplay.UpdateCoins(result.coins);
                OnAvatarPurchased?.Invoke(avatarId, result.coins);
            },
            err =>
            {
                actionButton.interactable = true;
                SetStatus(L("text.error_try_again", "Error, try again"));
                Debug.LogError($"[ShopAvatarSlotView] BuyAvatar {avatarId} failed: {err}");
            }
        ));
    }

    public void SetStatus(string message)
    {
        if (statusLabel == null) return;
        statusLabel.text = message;
        var svc = UnityLocalizationService.Instance;
        if (svc != null) statusLabel.isRightToLeftText = svc.IsRightToLeft;
    }

    private void SetButtonState(bool interactable)
    {
        if (actionButton != null) actionButton.interactable = interactable;
    }

    private static string L(string key, string fallback) =>
        UnityLocalizationService.Instance != null
            ? UnityLocalizationService.Instance.GetText(key, fallback)
            : fallback;

    private void DimAvatar(bool dim)
    {
        if (avatarImage != null)
            avatarImage.color = dim ? new Color(1f, 1f, 1f, 0.4f) : Color.white;
    }
}
