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
                return $"x{reward.amount}";

            default:
                return string.Empty;
        }
    }

    public static string GetDescription(WheelRewardDto reward)
    {
        if (reward == null)
            return string.Empty;

        switch (reward.rewardType)
        {
            case WheelRewardType.Coins:
                return $"You got {reward.amount} coins!";

            case WheelRewardType.Spin:
                return "You got a bonus spin!";

            case WheelRewardType.AddTile:
                return $"Draw {reward.amount} extra tile(s) from your deck!";

            case WheelRewardType.RemoveTile:
                return $"Remove {reward.amount} tile(s) from your hand!";

            case WheelRewardType.Multiply:
                return $"Score multiplied x{reward.amount}!";

            case WheelRewardType.Random:
                return $"Random booster x{reward.amount} activated!";

            default:
                return string.Empty;
        }
    }
}