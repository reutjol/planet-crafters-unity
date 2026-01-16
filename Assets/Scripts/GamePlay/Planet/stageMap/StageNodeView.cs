using UnityEngine;

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
