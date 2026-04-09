using TMPro;
using UnityEngine;

public class ScoreDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text currentScoreText;
    [SerializeField] private TMP_Text targetScoreText;
    [SerializeField] private MapController mapController;

    private void Awake()
    {
        if (mapController == null)
            mapController = FindObjectOfType<MapController>();
    }

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

    public void SetTargetScore(int target)
    {
        if (targetScoreText != null)
            targetScoreText.text = target.ToString();
    }

    private void UpdateDisplay(ProgressDto progress)
    {
        if (currentScoreText != null)
            currentScoreText.text = progress.score.ToString();
    }
}
