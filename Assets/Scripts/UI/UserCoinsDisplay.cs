using System;
using TMPro;
using UnityEngine;

public class UserCoinsDisplay : MonoBehaviour
{
    public static event Action<int> OnCoinsChanged;

    public static void UpdateCoins(int coins) => OnCoinsChanged?.Invoke(coins);

    [SerializeField] private TMP_Text coinsText;

    private void OnEnable()  => OnCoinsChanged += SetCoins;
    private void OnDisable() => OnCoinsChanged -= SetCoins;

    private void SetCoins(int coins)
    {
        if (coinsText != null)
            coinsText.text = coins.ToString("N0");
    }
}
