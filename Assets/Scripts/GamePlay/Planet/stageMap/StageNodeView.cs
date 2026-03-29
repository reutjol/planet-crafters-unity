using UnityEngine;

/// <summary>
/// Visual representation of a stage node on the stage map.
/// Stores stage metadata and shows a progress percentage for in-progress stages.
/// </summary>
public class StageNodeView : MonoBehaviour
{
    [Header("Stage Data")]
    public string stageId;
    public bool isUnlocked;
    public bool isCompleted;
    public int score;

    [Header("UI")]
    [SerializeField] private TMPro.TextMeshPro progressText;

    public void Init(string id, bool unlocked, bool completed, int stageScore, int targetScore)
    {
        stageId = id;
        isUnlocked = unlocked;
        isCompleted = completed;
        score = stageScore;

        if (progressText == null) return;

        // Show percentage only for unlocked, non-completed stages that have been started
        bool showProgress = unlocked && !completed && stageScore > 0;
        progressText.gameObject.SetActive(showProgress);

        if (showProgress)
        {
            int percent = targetScore > 0
                ? Mathf.RoundToInt((float)stageScore / targetScore * 100f)
                : 0;
            progressText.text = $"{Mathf.Min(percent, 100)}%";
        }
    }
}
