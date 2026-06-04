public static class WheelRewardTextFormatter
{
    public static string Format(WheelRewardDto reward)
    {
        if (reward == null)
            return string.Empty;

        switch (reward.rewardType)
        {
            case WheelRewardType.Coins:
                return reward.amount.ToString();

            case WheelRewardType.Spin:
                return "+1";

            case WheelRewardType.Jocker:
            case WheelRewardType.Swap:
            case WheelRewardType.Refresh:
            case WheelRewardType.Random:
                return $"x{reward.amount}";

            default:
                return string.Empty;
        }
    }
}