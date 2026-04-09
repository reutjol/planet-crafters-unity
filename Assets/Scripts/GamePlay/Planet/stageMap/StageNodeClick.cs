using UnityEngine;

/// <summary>
/// Handles click interactions on stage nodes.
/// Only allows entering unlocked stages, sets session state, and loads gameplay scene.
/// </summary>
public class StageNodeClick : MonoBehaviour
{
    [SerializeField] private GameConfig gameConfig;
    private StageNodeView view;

    private void Awake()
    {
        view = GetComponent<StageNodeView>();
        if (view == null)
            Debug.LogError("StageNodeView missing on this GameObject");

        if (gameConfig == null)
        {
            gameConfig = Resources.Load<GameConfig>("GameConfig");
        }
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

        if (SceneLoader.Instance != null && gameConfig != null)
        {
            SceneLoader.Instance.LoadScene(gameConfig.gameplaySceneIndex);
        }
    }
}
