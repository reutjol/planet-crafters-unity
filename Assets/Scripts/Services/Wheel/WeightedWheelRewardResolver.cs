using System.Collections.Generic;
using UnityEngine;

public class WeightedWheelRewardResolver : IWheelRewardResolver
{
    public WheelRewardDto ResolveReward(List<WheelRewardDto> rewards)
    {
        if (rewards == null || rewards.Count == 0)
            return null;

        float totalWeight = 0f;

        for (int i = 0; i < rewards.Count; i++)
        {
            if (rewards[i] != null && rewards[i].weight > 0f)
                totalWeight += rewards[i].weight;
        }

        if (totalWeight <= 0f)
            return rewards[0];

        float randomValue = Random.Range(0f, totalWeight);
        float currentWeight = 0f;

        for (int i = 0; i < rewards.Count; i++)
        {
            WheelRewardDto reward = rewards[i];

            if (reward == null || reward.weight <= 0f)
                continue;

            currentWeight += reward.weight;

            if (randomValue <= currentWeight)
                return reward;
        }

        return rewards[rewards.Count - 1];
    }
}