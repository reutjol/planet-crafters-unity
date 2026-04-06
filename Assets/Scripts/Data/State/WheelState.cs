using System;

[Serializable]
public class WheelState
{
    public long LastSpinUtcTicks;
    public int TotalSpins;
    public string LastRewardId;
}