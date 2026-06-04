using System.Collections;
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

            case WheelRewardType.Jocker:
                GrantServerBooster("doubleScore", reward.amount);
                break;

            case WheelRewardType.Swap:
                GrantServerBooster("cancelPlacement", reward.amount);
                break;

            case WheelRewardType.Refresh:
                GrantServerBooster("addHex", reward.amount);
                break;

            case WheelRewardType.Random:
                GrantRandomBooster(reward.amount);
                break;

            default:
                Debug.Log("Unknown reward type");
                break;
        }
    }

    private void GrantCoins(int amount)
    {
        var token = AppSession.Instance?.AccessToken;
        var planetId = AppSession.Instance?.ActivePlanet?.planetId;
        var api = PlanetApiClient.Instance;

        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(planetId) || api == null)
        {
            Debug.LogWarning($"[WheelRewardGrantService] Cannot grant coins — missing session data");
            return;
        }

        api.StartCoroutine(api.AddCoins(
            planetId, amount, token,
            onSuccess: total =>
            {
                Debug.Log($"[WheelRewardGrantService] +{amount} coins → total: {total}");
                GameManager.Instance?.NotifyCoinsChanged(total);
            },
            onError: err => Debug.LogError($"[WheelRewardGrantService] AddCoins failed: {err}")
        ));
    }

    private void GrantExtraSpin(int amount)
    {
        Debug.Log($"Grant Extra Spin: {amount}");
    }

    private void GrantServerBooster(string boosterType, int amount)
    {
        var token = AppSession.Instance?.AccessToken;
        if (string.IsNullOrEmpty(token))
        {
            Debug.LogWarning($"[WheelRewardGrantService] No token — cannot grant booster {boosterType}");
            return;
        }

        var api = GameManager.Instance?.BoosterApi;
        if (api == null)
        {
            Debug.LogWarning("[WheelRewardGrantService] BoosterApiClient not available via GameManager");
            return;
        }

        api.StartCoroutine(GrantBoosterCoroutine(boosterType, amount, token));
    }

    private IEnumerator GrantBoosterCoroutine(string boosterType, int amount, string token)
    {
        for (int i = 0; i < amount; i++)
        {
            bool done = false;
            yield return GameManager.Instance.BoosterApi.GrantBooster(
                boosterType,
                token,
                onSuccess: inventory =>
                {
                    Debug.Log($"[WheelRewardGrantService] Granted {boosterType}. New counts: " +
                              $"doubleScore={inventory.doubleScore}, " +
                              $"cancelPlacement={inventory.cancelPlacement}, " +
                              $"addHex={inventory.addHex}");
                    BoosterController.Instance?.RefreshFromInventory(inventory);
                    done = true;
                },
                onError: err =>
                {
                    Debug.LogError($"[WheelRewardGrantService] Failed to grant {boosterType}: {err}");
                    done = true;
                }
            );
        }
    }

    private void GrantRandomBooster(int amount)
    {
        string[] types = { "doubleScore", "cancelPlacement", "addHex" };
        string chosen = types[Random.Range(0, types.Length)];
        GrantServerBooster(chosen, amount);
    }
}
