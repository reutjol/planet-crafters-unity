using System;
using UnityEngine;
using UnityEngine.UI;

public class SpinButtonView : MonoBehaviour
{
    [SerializeField] private Button button;

    public event Action Clicked;

    private void Awake()
    {
        if (button != null)
            button.onClick.AddListener(HandleClick);
    }

    private void OnDestroy()
    {
        if (button != null)
            button.onClick.RemoveListener(HandleClick);
    }

    public void SetInteractable(bool isInteractable)
    {
        if (button != null)
            button.interactable = isInteractable;
    }

    private void HandleClick()
    {
        Clicked?.Invoke();
    }
}