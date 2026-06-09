using TMPro;
using UnityEngine;

/// <summary>
/// Displays the planet's total coins on the stage map screen.
/// </summary>
public class TotalCoinsDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text coinsText;

    private void OnEnable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnPlanetLoaded += OnPlanetLoaded;

        UserCoinsDisplay.OnCoinsChanged += Refresh;

        var planet = AppSession.Instance?.ActivePlanet;
        if (planet != null)
            Refresh(planet.totalCoins);
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnPlanetLoaded -= OnPlanetLoaded;

        UserCoinsDisplay.OnCoinsChanged -= Refresh;
    }

    private void OnPlanetLoaded(PlanetDto planet)
    {
        Refresh(planet.totalCoins);
    }

    public void Refresh(int totalCoins)
    {
        if (coinsText != null)
            coinsText.text = totalCoins.ToString();
    }
}
