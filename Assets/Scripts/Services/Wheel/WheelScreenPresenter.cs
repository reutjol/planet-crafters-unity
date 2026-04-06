using System;

public class WheelScreenPresenter
{
    private readonly SpinButtonView spinButtonView;
    private readonly CooldownTextView cooldownTextView;
    private readonly ISpinAvailabilityService spinAvailabilityService;

    public WheelScreenPresenter(
        SpinButtonView spinButtonView,
        CooldownTextView cooldownTextView,
        ISpinAvailabilityService spinAvailabilityService)
    {
        this.spinButtonView = spinButtonView;
        this.cooldownTextView = cooldownTextView;
        this.spinAvailabilityService = spinAvailabilityService;
    }

    public void PresentInitial()
    {
        RefreshAvailability();
    }

    public void PresentSpinning()
    {
        spinButtonView?.SetInteractable(false);
    }

    public void PresentBlocked(TimeSpan remainingCooldown)
    {
        spinButtonView?.SetInteractable(false);

        if (remainingCooldown > TimeSpan.Zero)
            cooldownTextView?.SetText($"Next spin in {remainingCooldown:hh\\:mm\\:ss}");
        else
            cooldownTextView?.SetText("Spin not available");
    }

    public void PresentCompleted(WheelSpinExecutionResult result)
    {
        RefreshAvailability();
    }

    public void RefreshAvailability()
    {
        if (spinAvailabilityService == null)
            return;

        bool canSpin = spinAvailabilityService.CanSpin();
        TimeSpan remaining = spinAvailabilityService.GetRemainingCooldown();

        spinButtonView?.SetInteractable(canSpin);

        if (canSpin)
            cooldownTextView?.SetText("Free spin available");
        else
            cooldownTextView?.SetText($"Next spin in {remaining:hh\\:mm\\:ss}");
    }
}