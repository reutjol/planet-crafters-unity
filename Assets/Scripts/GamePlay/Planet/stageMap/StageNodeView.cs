using UnityEngine;

/// <summary>
/// Visual representation of a stage node on the stage map.

/// Stores stage metadata like ID, unlock status, and completion state.

/// Stores stage metadata and shows a progress percentage for in-progress stages,
/// or stars (1-3) for completed stages.

/// </summary>
public class StageNodeView : MonoBehaviour
{
    [Header("Stage Data")]
    public string stageId;
    public bool isUnlocked;
    public bool isCompleted;


    

    public string resourceType;
    public int score;

    [Header("UI")]
    [SerializeField] private TMPro.TextMeshPro progressText;
    [SerializeField] private TMPro.TextMeshPro starsText;

    public void Init(string id, bool unlocked, bool completed, string resType, int stageScore, float developedPercent, int coinsAwarded)

    {
        stageId = id;
        isUnlocked = unlocked;
        isCompleted = completed;

        resourceType = resType;
        score = stageScore;

        // Progress text (in-progress only)
        if (progressText != null)
        {
            bool showProgress = unlocked && !completed && developedPercent > 0;
            progressText.gameObject.SetActive(showProgress);
            if (showProgress)
                progressText.text = $"{Mathf.Min(Mathf.RoundToInt(developedPercent), 99)}%";
        }

        // Stars text (completed only)
        if (starsText != null)
        {
            starsText.gameObject.SetActive(completed);
            if (completed)
            {
                int stars = coinsAwarded > 0 ? coinsAwarded : 1;
                starsText.text = $"{stars}/3";
            }
        }

    }
}
