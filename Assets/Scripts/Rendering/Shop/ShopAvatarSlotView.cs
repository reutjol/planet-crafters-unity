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

    private void Start()
    {
        if (actionButton != null)
            actionButton.onClick.AddListener(OnBuyClicked);
    }

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
            SetStatus($"Available from stage {entry.stageRequired}");
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
                nameLabel.text = $"{displayName} avatar";
            }
        }
    }

    private void ApplyOwnedState()
    {
        SetStatus("Already owned");
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
                        ? "Not enough coins"
                        : "Purchase failed");
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
                SetStatus("Error, try again");
                Debug.LogError($"[ShopAvatarSlotView] BuyAvatar {avatarId} failed: {err}");
            }
        ));
    }

    public void SetStatus(string message)
    {
        if (statusLabel != null)
            statusLabel.text = message;
    }

    private void SetButtonState(bool interactable)
    {
        if (actionButton != null) actionButton.interactable = interactable;
    }

    private void DimAvatar(bool dim)
    {
        if (avatarImage != null)
            avatarImage.color = dim ? new Color(1f, 1f, 1f, 0.4f) : Color.white;
    }
}
