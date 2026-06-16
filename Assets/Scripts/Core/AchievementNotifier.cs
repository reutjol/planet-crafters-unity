using System;
using System.Collections.Generic;

public static class AchievementNotifier
{
    public static event Action<List<UnlockedAchievementDto>> OnAchievementsUnlocked;

    private static List<UnlockedAchievementDto> _pending;

    public static void Notify(List<UnlockedAchievementDto> unlocked)
    {
        if (unlocked == null || unlocked.Count == 0) return;

        if (OnAchievementsUnlocked == null || OnAchievementsUnlocked.GetInvocationList().Length == 0)
        {
            _pending = unlocked;
        }
        else
        {
            OnAchievementsUnlocked.Invoke(unlocked);
        }
    }

    public static void FlushPending()
    {
        if (_pending == null || _pending.Count == 0) return;
        var toSend = _pending;
        _pending = null;
        OnAchievementsUnlocked?.Invoke(toSend);
    }
}
