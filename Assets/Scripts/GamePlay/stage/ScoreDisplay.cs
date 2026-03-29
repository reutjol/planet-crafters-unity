using TMPro;
using UnityEngine;

/// <summary>
/// Displays the current score on screen.
/// Subscribes to MapController.OnProgressChanged and updates a TMP_Text element.
/// </summary>
public class ScoreDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private MapController mapController;

    private void OnEnable()
    {
        if (mapController != null)
            mapController.OnProgressChanged += UpdateDisplay;
    }

    private void OnDisable()
    {
        if (mapController != null)
            mapController.OnProgressChanged -= UpdateDisplay;
    }

    private void UpdateDisplay(ProgressDto progress)
    {
        if (scoreText != null)
            scoreText.text = $"Score: {progress.score}";
    }
}
