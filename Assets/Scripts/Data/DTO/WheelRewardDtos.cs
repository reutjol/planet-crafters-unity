using System;

[System.Serializable]
public class WheelRewardDto
{
    public string id;
    public string title;
    public WheelRewardType rewardType;
    public int amount;
    public float weight;
    public int sliceIndex;
    public string iconKey;
}