using UnityEngine;
using UnityEngine.UI;

public class ShopController : MonoBehaviour
{
    [Header("Tabs")]
    [SerializeField] private Button boosterTabButton;
    [SerializeField] private Button avatarsTabButton;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private GameObject boostersContent;
    [SerializeField] private GameObject avatarsContent;

    [Header("Booster Buy Buttons")]
    [SerializeField] private Button buyDoubleScoreButton;
    [SerializeField] private Button buyCancelPlacementButton;
    [SerializeField] private Button buyAddHexButton;

    private ShopApiClient api;
    private string token;
    private ShopProfileDto shopProfile;

    private void Start()
    {
        api   = ShopApiClient.GetOrCreate();
        token = AppSession.Instance?.AccessToken;

        boosterTabButton?.onClick.AddListener(() => ShowTab(boosters: true));
        avatarsTabButton?.onClick.AddListener(() => ShowTab(boosters: false));

        buyDoubleScoreButton?.onClick.AddListener(() => BuyBooster("doubleScore"));
        buyCancelPlacementButton?.onClick.AddListener(() => BuyBooster("cancelPlacement"));
        buyAddHexButton?.onClick.AddListener(() => BuyBooster("addHex"));

        ShowTab(boosters: true);
        LoadProfile();
    }

    private void OnEnable()  => ShopAvatarSlotView.OnAvatarPurchased += HandleAvatarPurchased;
    private void OnDisable() => ShopAvatarSlotView.OnAvatarPurchased -= HandleAvatarPurchased;

    private void HandleAvatarPurchased(string avatarId, int newCoins)
    {
        if (shopProfile != null) shopProfile.coins = newCoins;
    }

    // ── Tabs ──────────────────────────────────────────────────────────────────

    private void ShowTab(bool boosters)
    {
        if (scrollRect != null)
            scrollRect.content = boosters
                ? boostersContent?.GetComponent<RectTransform>()
                : avatarsContent?.GetComponent<RectTransform>();

        if (boostersContent != null) boostersContent.SetActive(boosters);
        if (avatarsContent  != null) avatarsContent.SetActive(!boosters);

        if (boosterTabButton != null) boosterTabButton.interactable = !boosters;
        if (avatarsTabButton  != null) avatarsTabButton.interactable  = boosters;
    }

    // ── Profile ───────────────────────────────────────────────────────────────

    private void LoadProfile()
    {
        StartCoroutine(api.GetProfile(token,
            profile =>
            {
                shopProfile = profile;
                UserCoinsDisplay.UpdateCoins(profile.coins);
                SetupAvatarSlots(profile);
            },
            err => Debug.LogError($"[ShopController] Failed to load profile: {err}")
        ));
    }

    private void SetupAvatarSlots(ShopProfileDto profile)
    {
        if (avatarsContent == null) return;
        foreach (var slot in avatarsContent.GetComponentsInChildren<ShopAvatarSlotView>(true))
            slot.Setup(profile);
    }

    // ── Boosters ──────────────────────────────────────────────────────────────

    private void BuyBooster(string boosterType)
    {
        SetBoosterButtonsInteractable(false);

        StartCoroutine(api.BuyBooster(token, boosterType,
            result =>
            {
                SetBoosterButtonsInteractable(true);

                if (!result.success)
                {
                    Debug.LogWarning($"[ShopController] BuyBooster failed: {result.reason}");
                    return;
                }

                if (shopProfile != null) shopProfile.coins = result.coins;
                UserCoinsDisplay.UpdateCoins(result.coins);
            },
            err =>
            {
                SetBoosterButtonsInteractable(true);
                Debug.LogError($"[ShopController] BuyBooster failed: {err}");
            }
        ));
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private void SetBoosterButtonsInteractable(bool value)
    {
        if (buyDoubleScoreButton != null)     buyDoubleScoreButton.interactable     = value;
        if (buyCancelPlacementButton != null) buyCancelPlacementButton.interactable = value;
        if (buyAddHexButton != null)          buyAddHexButton.interactable          = value;
    }
}
