using UnityEngine;
using UnityEngine.UI;

// Attach to a UI Button in the gameplay canvas.
// The button shows only while a tile is being dragged and calls Rotate() on it.
public class RotateButton : MonoBehaviour
{
    [SerializeField] private Button _button;

    private DraggableTile _activeTile;

    private void Awake()
    {
        if (_button == null)
            _button = GetComponent<Button>();

        _button.onClick.AddListener(OnRotateClicked);
        _button.gameObject.SetActive(false);

        // Subscribe here and never unsubscribe until destroyed,
        // so the event is received even while the button is hidden.
        DraggableTile.OnDragStateChanged += HandleDragStateChanged;
    }

    private void OnDestroy()
    {
        DraggableTile.OnDragStateChanged -= HandleDragStateChanged;
    }

    private void HandleDragStateChanged(bool isDragging)
    {
        _button.gameObject.SetActive(isDragging);
        _activeTile = isDragging ? DraggableTile.ActiveTile : null;
    }

    private void OnRotateClicked()
    {
        _activeTile?.Rotate();
    }
}
