using UnityEngine;

/// <summary>
/// Debug only — simulates stage completion with a chosen coin count.
/// Remove before release.
/// </summary>
public class DebugCompleteButton : MonoBehaviour
{
    [SerializeField] private StageCompletePanel stageCompletePanel;
    [Range(1, 3)]
    [SerializeField] private int coinsToTest = 2;

    public void SimulateComplete()
    {
        stageCompletePanel?.Show(coinsToTest);
    }
}
