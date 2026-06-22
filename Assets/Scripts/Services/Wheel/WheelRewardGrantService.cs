using UnityEngine;

public class WheelRewardGrantService : IWheelRewardGrantService
{
    private readonly ISpinAvailabilityService spinAvailabilityService;

    public WheelRewardGrantService(ISpinAvailabilityService spinAvailabilityService)
    {
        this.spinAvailabilityService = spinAvailabilityService;
    }

    public string GrantReward(WheelRewardDto reward)
    {
        if (reward == null)
            return string.Empty;

        switch (reward.rewardType)
        {
            case WheelRewardType.Coins:
                GrantCoins(reward.amount);
                return string.Empty;

            case WheelRewardType.Spin:
                GrantExtraSpin(reward.amount);
                return "You got a bonus spin!";

            case WheelRewardType.AddTile:
                GrantBooster("addHex", reward.amount);
                return $"You got Add Tile x{reward.amount}!";

            case WheelRewardType.RemoveTile:
                GrantBooster("cancelPlacement", reward.amount);
                return $"You got Remove Tile x{reward.amount}!";

            case WheelRewardType.Multiply:
                GrantBooster("doubleScore", reward.amount);
                return $"You got Double Score x{reward.amount}!";

            case WheelRewardType.Random:
                return GrantRandomBooster(reward.amount);

            default:
                Debug.LogWarning($"[WheelRewardGrantService] Unknown reward type: {reward.rewardType}");
                return string.Empty;
        }
    }

    private void GrantCoins(int amount)
    {
        var token = AppSession.Instance?.AccessToken;
        var api = ShopApiClient.GetOrCreate();

        if (string.IsNullOrEmpty(token))
        {
            Debug.LogWarning("[WheelRewardGrantService] Cannot grant coins — no token");
            return;
        }

        api.StartCoroutine(api.AddCoins(
            token, amount,
            result => UserCoinsDisplay.UpdateCoins(result.coins),
            err => Debug.LogError($"[WheelRewardGrantService] AddCoins failed: {err}")
        ));
    }

    private void GrantBooster(string boosterType, int amount)
    {
        var token = AppSession.Instance?.AccessToken;
        var api = GameManager.Instance?.BoosterApi;

        if (string.IsNullOrEmpty(token) || api == null)
        {
            Debug.LogWarning($"[WheelRewardGrantService] Cannot grant booster {boosterType} — missing session data");
            return;
        }

        for (int i = 0; i < amount; i++)
        {
            api.StartCoroutine(api.GrantBooster(
                boosterType, token,
                inventory => BoosterController.Instance?.RefreshFromInventory(inventory),
                err => Debug.LogError($"[WheelRewardGrantService] GrantBooster {boosterType} failed: {err}")
            ));
        }
    }

    private void GrantExtraSpin(int amount)
    {
        var token = AppSession.Instance?.AccessToken;
        var api = WheelApiClient.Instance;

        if (string.IsNullOrEmpty(token) || api == null)
        {
            Debug.LogWarning("[WheelRewardGrantService] Cannot grant spin — missing session data");
            return;
        }

        for (int i = 0; i < amount; i++)
        {
            api.StartCoroutine(api.GrantSpin(
                token,
                _ => spinAvailabilityService?.Refresh(null),
                err => Debug.LogError($"[WheelRewardGrantService] GrantSpin failed: {err}")
            ));
        }
    }

    private string GrantRandomBooster(int amount)
    {
        string[] types = { "addHex", "cancelPlacement", "doubleScore" };
        string chosen = types[Random.Range(0, types.Length)];
        GrantBooster(chosen, amount);

        string boosterName = chosen switch
        {
            "addHex"           => "Add Tile",
            "cancelPlacement"  => "Remove Tile",
            "doubleScore"      => "Double Score",
            _                  => chosen
        };
        return $"Random booster: {boosterName} x{amount}!";
    }
}
