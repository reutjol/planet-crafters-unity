using UnityEngine;
using UnityEngine.Serialization;

public class StageNodeView : MonoBehaviour
{
    [Header("UI")]
    [FormerlySerializedAs("progressText")] [SerializeField] private TMPro.TextMeshPro _progressText;
    [FormerlySerializedAs("starsText")]    [SerializeField] private TMPro.TextMeshPro _starsText;

    public string StageId { get; private set; }
    public bool IsUnlocked { get; private set; }
    public bool IsCompleted { get; private set; }
    public string ResourceType { get; private set; }
    public int Score { get; private set; }

    public void Init(string id, bool unlocked, bool completed, string resType, int stageScore, float developedPercent, int coinsAwarded)
    {
        StageId = id;
        IsUnlocked = unlocked;
        IsCompleted = completed;
        ResourceType = resType;
        Score = stageScore;

        if (_progressText != null)
        {
            bool showProgress = unlocked && !completed && developedPercent > 0;
            _progressText.gameObject.SetActive(showProgress);
            if (showProgress)
                _progressText.text = $"{Mathf.Min(Mathf.RoundToInt(developedPercent), 99)}%";
        }

        if (_starsText != null)
        {
            _starsText.gameObject.SetActive(completed);
            if (completed)
            {
                int stars = Mathf.Clamp(coinsAwarded > 0 ? coinsAwarded : 1, 1, 3);
                _starsText.text = $"{stars}/3";
            }
        }
    }
}
