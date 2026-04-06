using System;

public class WheelAvailabilityService : ISpinAvailabilityService
{
    private readonly IWheelStateRepository stateRepository;
    private readonly TimeSpan cooldown;

    public WheelAvailabilityService(IWheelStateRepository stateRepository, TimeSpan cooldown)
    {
        this.stateRepository = stateRepository;
        this.cooldown = cooldown;
    }

    public bool CanSpin()
    {
        WheelState state = stateRepository.Load();

        if (state.LastSpinUtcTicks <= 0)
            return true;

        DateTime lastSpinUtc = new DateTime(state.LastSpinUtcTicks, DateTimeKind.Utc);
        TimeSpan elapsed = DateTime.UtcNow - lastSpinUtc;

        return elapsed >= cooldown;
    }

    public TimeSpan GetRemainingCooldown()
    {
        WheelState state = stateRepository.Load();

        if (state.LastSpinUtcTicks <= 0)
            return TimeSpan.Zero;

        DateTime lastSpinUtc = new DateTime(state.LastSpinUtcTicks, DateTimeKind.Utc);
        TimeSpan elapsed = DateTime.UtcNow - lastSpinUtc;
        TimeSpan remaining = cooldown - elapsed;

        return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
    }

    public void ConsumeSpin()
    {
        WheelState state = stateRepository.Load();

        state.LastSpinUtcTicks = DateTime.UtcNow.Ticks;
        state.TotalSpins += 1;

        stateRepository.Save(state);
    }
}