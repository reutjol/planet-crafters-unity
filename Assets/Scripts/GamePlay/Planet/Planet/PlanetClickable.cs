using UnityEngine;

/// <summary>
/// Simple clickable component for planet GameObjects.
/// Invokes a callback when the planet is clicked.
/// </summary>
public class PlanetClickable : MonoBehaviour
{
    public System.Action Clicked;

    private void OnMouseDown()
    {
        Clicked?.Invoke();
    }
}
