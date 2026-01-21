using UnityEngine;

/// <summary>
/// Visual representation of a stage node on the stage map.
/// Stores stage metadata like ID, unlock status, and completion state.
/// </summary>
public class StageNodeView : MonoBehaviour
{
    [Header("Stage Data")]
    public string stageId;
    public bool isUnlocked;
    public bool isCompleted;

    public void Init(string id, bool unlocked, bool completed)
    {
        stageId = id;
        isUnlocked = unlocked;
        isCompleted = completed;
    }
}
