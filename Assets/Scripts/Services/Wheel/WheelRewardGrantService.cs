using UnityEngine;

public class WheelRewardGrantService : IWheelRewardGrantService
{
    public void GrantReward(WheelRewardDto reward)
    {
        if (reward == null)
            return;

        switch (reward.rewardType)
        {
            case WheelRewardType.Coins:
                GrantCoins(reward.amount);
                break;

            case WheelRewardType.Spin:
                GrantExtraSpin(reward.amount);
                break;

            case WheelRewardType.AddTile:
                GrantBooster("addHex", reward.amount);
                break;

            case WheelRewardType.RemoveTile:
                GrantBooster("cancelPlacement", reward.amount);
                break;

            case WheelRewardType.Multiply:
                GrantBooster("doubleScore", reward.amount);
                break;

            case WheelRewardType.Random:
                GrantRandomBooster(reward.amount);
                break;

            default:
                Debug.LogWarning($"[WheelRewardGrantService] Unknown reward type: {reward.rewardType}");
                break;
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
            result =>
            {
                Debug.Log($"[WheelRewardGrantService] +{amount} coins → total: {result.coins}");
                UserCoinsDisplay.UpdateCoins(result.coins);
            },
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
                inventory =>
                {
                    Debug.Log($"[WheelRewardGrantService] Granted {boosterType}");
                    BoosterController.Instance?.RefreshFromInventory(inventory);
                },
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
                status => Debug.Log($"[WheelRewardGrantService] +1 spin → credits: {status.spinCredits}"),
                err => Debug.LogError($"[WheelRewardGrantService] GrantSpin failed: {err}")
            ));
        }
    }

    private void GrantRandomBooster(int amount)
    {
        string[] types = { "addHex", "cancelPlacement", "doubleScore" };
        string chosen = types[Random.Range(0, types.Length)];
        GrantBooster(chosen, amount);
    }
}
