using UnityEngine;

public interface IWheelRewardIconProvider
{
    Sprite GetIcon(WheelRewardDto reward);
}