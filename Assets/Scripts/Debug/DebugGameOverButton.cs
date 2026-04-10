using UnityEngine;

/// <summary>
/// Debug button: simulates game over (hand and deck empty).
/// Attach to a UI Button's OnClick.
/// </summary>
public class DebugGameOverButton : MonoBehaviour
{
    [SerializeField] private HandController handController;

    public void OnClick()
    {
        if (handController == null)
        {
            Debug.LogError("[DebugGameOverButton] HandController not assigned");
            return;
        }

        handController.SimulateGameOver();
        Debug.Log("[DebugGameOverButton] Fired fake game over");
    }
}
