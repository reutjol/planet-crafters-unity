using System;

[Serializable]
public class WheelSpinExecutionResult
{
    public bool success;
    public WheelRewardDto reward;
    public float targetAngle;
    public TimeSpan remainingCooldown;
    public string message;
}