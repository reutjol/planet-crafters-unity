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

            case WheelRewardType.AddTile:
            case WheelRewardType.RemoveTile:
            case WheelRewardType.Multiply:
            case WheelRewardType.Random:
                return reward.amount.ToString();

            default:
                return string.Empty;
        }
    }
}