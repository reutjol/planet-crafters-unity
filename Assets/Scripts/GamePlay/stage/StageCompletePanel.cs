using TMPro;
using UnityEngine;

/// <summary>
/// Shown when a stage is completed. Displays coins awarded (1-3).
/// Wire up coin GameObjects (e.g. star/coin icons) to coinObjects[].
/// </summary>
public class StageCompletePanel : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject panel;

    [Header("Coin icons (index 0=1 coin, 1=2 coins, 2=3 coins)")]
    [SerializeField] private GameObject[] coinObjects = new GameObject[3];

    [Header("Refs")]
    [SerializeField] private MapController mapController;

    private void Awake()
    {
        if (mapController == null)
            mapController = FindObjectOfType<MapController>();

        if (panel != null)
            panel.SetActive(false);
    }

    private void Start()
    {
        if (mapController != null)
            mapController.OnStageCompletedWithCoins += Show;
    }

    private void OnDestroy()
    {
        if (mapController != null)
            mapController.OnStageCompletedWithCoins -= Show;
    }

    public void Show(int coins)
    {
        if (panel != null)
        {
            panel.SetActive(true);
            PopupManager.OnPopupOpened();
        }

        // Light up the earned coin icons
        for (int i = 0; i < coinObjects.Length; i++)
        {
            if (coinObjects[i] != null)
                coinObjects[i].SetActive(i < coins);
        }
    }
}
