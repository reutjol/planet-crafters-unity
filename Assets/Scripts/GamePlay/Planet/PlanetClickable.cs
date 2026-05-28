using System;
using UnityEngine;

public class PlanetClickable : MonoBehaviour
{
    public event Action Clicked;

    private void OnMouseDown()
    {
        if (PopupManager.IsAnyPopupOpen) return;
        Clicked?.Invoke();
    }
}
