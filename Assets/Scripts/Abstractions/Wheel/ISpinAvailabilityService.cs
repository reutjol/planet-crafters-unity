using System;

public interface ISpinAvailabilityService
{
    bool CanSpin();
    TimeSpan GetRemainingCooldown();
    void ConsumeSpin();
}