using System;

public class WheelScreenPresenter
{
    private readonly SpinButtonView spinButtonView;
    private readonly CooldownTextView cooldownTextView;
    private readonly ISpinAvailabilityService spinAvailabilityService;
    private readonly ILocalizationService localizationService;

    public WheelScreenPresenter(
        SpinButtonView spinButtonView,
        CooldownTextView cooldownTextView,
        ISpinAvailabilityService spinAvailabilityService,
        ILocalizationService localizationService)
    {
        this.spinButtonView = spinButtonView;
        this.cooldownTextView = cooldownTextView;
        this.spinAvailabilityService = spinAvailabilityService;
        this.localizationService = localizationService;
    }

    public void PresentInitial()
    {
        if (spinAvailabilityService == null)
            return;

        spinAvailabilityService.Refresh(RefreshAvailability);
    }

    public void PresentSpinning()
    {
        spinButtonView?.SetInteractable(false);
    }

    public void PresentBlocked(TimeSpan remainingCooldown)
    {
        spinButtonView?.SetInteractable(false);

        if (remainingCooldown > TimeSpan.Zero)
            cooldownTextView?.SetText(GetCooldownText(remainingCooldown));
        else
            cooldownTextView?.SetText(GetText("text.spin_not_available", "Spin not available"));
    }

    public void PresentCompleted(WheelSpinExecutionResult result)
    {
        if (spinAvailabilityService == null)
            return;

        spinAvailabilityService.Refresh(RefreshAvailability);
    }

    public void RefreshAvailability()
    {
        if (spinAvailabilityService == null)
            return;

        bool canSpin = spinAvailabilityService.CanSpin();
        TimeSpan remaining = spinAvailabilityService.GetRemainingCooldown();

        spinButtonView?.SetInteractable(canSpin);

        if (canSpin)
        {
            int credits = spinAvailabilityService.GetSpinCredits();
            string creditsText = credits > 0 ? $"Spins: {credits}" : GetText("text.free_spin_available", "Free spin available");
            cooldownTextView?.SetText(creditsText);
        }
        else if (remaining > TimeSpan.Zero)
        {
            cooldownTextView?.SetText(GetCooldownText(remaining));
        }
        else
        {
            cooldownTextView?.SetText(GetText("text.spin_not_available", "Spin not available"));
        }
    }

    private string GetCooldownText(TimeSpan remaining)
    {
        return remaining.ToString(@"hh\:mm\:ss");
    }

    private string GetText(string key, string fallbackText)
    {
        return localizationService != null
            ? localizationService.GetText(key, fallbackText)
            : fallbackText;
    }
}
