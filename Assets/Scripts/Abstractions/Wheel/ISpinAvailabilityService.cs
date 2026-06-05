using System;

public interface ISpinAvailabilityService
{
    bool CanSpin();
    TimeSpan GetRemainingCooldown();
    void ConsumeSpin();
    void ConsumeSpin(Action<WheelSpinConsumeResult> onCompleted);
    void Refresh(Action onCompleted = null);
}
