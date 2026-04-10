using UnityEngine;

/// <summary>
/// Debug button: simulates receiving a bonus point from the server.
/// Attach to a UI Button's OnClick.
/// </summary>
public class DebugBonusButton : MonoBehaviour
{
    [SerializeField] private MapController mapController;

    public void OnClick()
    {
        if (mapController == null)
        {
            Debug.LogError("[DebugBonusButton] MapController not assigned");
            return;
        }

        mapController.DebugTriggerBonus(1);
        Debug.Log("[DebugBonusButton] Fired fake bonus +1");
    }
}
