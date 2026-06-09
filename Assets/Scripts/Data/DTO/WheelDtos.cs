using System;

[Serializable]
public class WheelSpinRequestDto
{
}

[Serializable]
public class WheelStatusDto
{
    public bool success;
    public bool canSpin;
    public int spinCredits;
    public int remainingSeconds;
    public string lastSpinAt;
    public string nextSpinAt;
    public string msg;
}
