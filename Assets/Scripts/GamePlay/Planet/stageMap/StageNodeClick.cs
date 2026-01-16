using UnityEngine;

public class StageNodeClick : MonoBehaviour
{
    private StageNodeView view;

    private void Awake()
    {
        view = GetComponent<StageNodeView>();
        if (view == null)
            Debug.LogError("StageNodeView missing on this GameObject");
    }

    private void OnMouseDown()
    {
        if (view == null) return;

        if (!view.isUnlocked)
        {
            Debug.Log($"[StagesMap] Stage {view.stageId} is locked");
            return;
        }

        if (AppSession.Instance == null)
        {
            Debug.LogError("AppSession.Instance is null");
            return;
        }

        if (string.IsNullOrEmpty(view.stageId))
        {
            Debug.LogError("stageId is empty on StageNodeView");
            return;
        }

        AppSession.Instance.SetSelectedStage(view.stageId);

        Debug.Log($"[StagesMap] Clicked unlocked stage: {view.stageId}");

        SceneLoader.Instance.LoadScene(7);
    }
}
