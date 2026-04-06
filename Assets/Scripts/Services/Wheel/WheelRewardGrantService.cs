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
                GrantJocker(reward.amount);
                break;

            case WheelRewardType.Swap:
                GrantSwap(reward.amount);
                break;

            case WheelRewardType.Refresh:
                GrantRefresh(reward.amount);
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
        Debug.Log($"Grant Coins: {amount}");
        // TODO: connect to your real coins system
    }

    private void GrantExtraSpin(int amount)
    {
        Debug.Log($"Grant Extra Spin: {amount}");
        // TODO: connect to your real spin system
    }

    private void GrantJocker(int amount)
    {
        Debug.Log($"Grant Booster A (Joker) x{amount}");
        // TODO: connect to your real booster inventory
    }

    private void GrantSwap(int amount)
    {
        Debug.Log($"Grant Booster B (Swap 2 placed tiles) x{amount}");
        // TODO: connect to your real booster inventory
    }

    private void GrantRefresh(int amount)
    {
        Debug.Log($"Grant Booster C (Refresh 3 algorithm tiles) x{amount}");
        // TODO: connect to your real booster inventory
    }

    private void GrantRandomBooster(int amount)
    {
        int randomIndex = Random.Range(0, 3);

        switch (randomIndex)
        {
            case 0:
                GrantJocker(amount);
                break;

            case 1:
                GrantSwap(amount);
                break;

            case 2:
                GrantRefresh(amount);
                break;
        }
    }
}