using System;

public interface ISpinAvailabilityService
{
    bool CanSpin();
    TimeSpan GetRemainingCooldown();
    int GetSpinCredits();
    void ConsumeSpin();
    void ConsumeSpin(Action<WheelSpinConsumeResult> onCompleted);
    void Refresh(Action onCompleted = null);
}
