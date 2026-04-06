using System;
using UnityEngine;

[Serializable]
public class WheelSpinResult
{
    [SerializeField] private WheelRewardDto reward;
    [SerializeField] private int sliceIndex;
    [SerializeField] private float targetAngle;

    public WheelRewardDto Reward => reward;
    public int SliceIndex => sliceIndex;
    public float TargetAngle => targetAngle;

    public WheelSpinResult(WheelRewardDto reward, int sliceIndex, float targetAngle)
    {
        this.reward = reward;
        this.sliceIndex = sliceIndex;
        this.targetAngle = targetAngle;
    }
}