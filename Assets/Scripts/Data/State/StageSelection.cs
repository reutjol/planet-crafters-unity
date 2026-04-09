using UnityEngine;

/// <summary>
/// Static utility class for tracking the currently selected stage coordinates.
/// (Note: Consider migrating to AppSession for better state management)
/// </summary>
public static class StageSelection
{
    public static Vector2Int CurrentStageKey;
}
