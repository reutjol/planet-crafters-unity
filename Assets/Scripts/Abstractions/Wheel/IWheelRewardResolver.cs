using System.Collections.Generic;

public interface IWheelRewardResolver
{
    WheelRewardDto ResolveReward(List<WheelRewardDto> rewards);
}