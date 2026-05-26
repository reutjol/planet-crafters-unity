using UnityEngine;

[RequireComponent(typeof(StageNodeView))]
public class StageNodeClick : MonoBehaviour
{
    [SerializeField] private GameConfig _gameConfig;
    private StageNodeView _view;

    private void Awake()
    {
        _view = GetComponent<StageNodeView>();
        if (_view == null)
            Debug.LogError("[StageNodeClick] StageNodeView missing on this GameObject");

        if (_gameConfig == null)
            _gameConfig = Resources.Load<GameConfig>("GameConfig");
    }

    private void OnMouseDown()
    {
        if (_view == null) return;
        if (PopupManager.IsAnyPopupOpen) return;

        if (!_view.IsUnlocked)
        {
            Debug.Log($"[StageNodeClick] Stage {_view.StageId} is locked");
            return;
        }

        if (AppSession.Instance == null)
        {
            Debug.LogError("[StageNodeClick] AppSession.Instance is null");
            return;
        }

        if (string.IsNullOrEmpty(_view.StageId))
        {
            Debug.LogError("[StageNodeClick] stageId is empty on StageNodeView");
            return;
        }

        AppSession.Instance.SetSelectedStage(_view.StageId);

        if (SceneLoader.Instance != null && _gameConfig != null)
            SceneLoader.Instance.LoadScene(_gameConfig.gameplaySceneIndex);
    }
}
