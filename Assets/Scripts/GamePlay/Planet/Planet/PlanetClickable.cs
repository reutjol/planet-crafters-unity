using UnityEngine;

public class PlanetClickable : MonoBehaviour
{
    public System.Action Clicked;

    private void OnMouseDown()
    {
        Clicked?.Invoke();
    }
}
