using System.Collections.Generic;
using UnityEngine;

public class WheelRewardIconProvider : IWheelRewardIconProvider
{
    private const string IconsFolderPath = "Sprites/Icons";
    private readonly Dictionary<string, Sprite> cache = new Dictionary<string, Sprite>();

    public Sprite GetIcon(WheelRewardDto reward)
    {
        if (reward == null)
        {
            Debug.LogWarning("IconProvider: reward is null");
            return null;
        }

        if (string.IsNullOrWhiteSpace(reward.iconKey))
        {
            Debug.LogWarning($"IconProvider: iconKey missing for reward {reward.id}");
            return null;
        }

        if (cache.TryGetValue(reward.iconKey, out Sprite cachedSprite))
            return cachedSprite;

        string resourcePath = $"{IconsFolderPath}/{reward.iconKey}";
        Sprite sprite = Resources.Load<Sprite>(resourcePath);

        if (sprite == null)
        {
            Debug.LogWarning($"IconProvider: Sprite not found at Resources path: {resourcePath}");
            return null;
        }

        cache[reward.iconKey] = sprite;
        return sprite;
    }
}