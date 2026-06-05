using System;

[Serializable]
public class WheelSpinConsumeResult
{
    public bool success;
    public TimeSpan remainingCooldown;
    public string message;
}
