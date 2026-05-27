using UnityEngine;

/// <summary>
/// Controls visibility of a GameObject based on whether a multiplayer match is active.
/// Attach to any UI element that should appear only in single-player or only in multiplayer.
/// </summary>
public class MatchModeObject : MonoBehaviour
{
    public enum Visibility { SinglePlayerOnly, MultiplayerOnly }

    [SerializeField] private Visibility showIn = Visibility.SinglePlayerOnly;

    private void Awake()
    {
        bool inMatch = MatchSession.Instance != null && MatchSession.Instance.IsActive;
        bool shouldShow = showIn == Visibility.MultiplayerOnly ? inMatch : !inMatch;
        gameObject.SetActive(shouldShow);
    }
}
