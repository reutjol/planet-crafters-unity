using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public class MapCameraController : MonoBehaviour
{
    [Header("Pan Bounds")]
    [FormerlySerializedAs("panBoundX")] [SerializeField] private float _panBoundX = 8f;
    [FormerlySerializedAs("panBoundZ")] [SerializeField] private float _panBoundZ = 8f;

    [Header("Keyboard Pan")]
    [FormerlySerializedAs("keyPanSpeed")] [SerializeField] private float _keyPanSpeed = 8f;

    [Header("Zoom")]
    [FormerlySerializedAs("zoomSpeed")] [SerializeField] private float _zoomSpeed = 2f;
    [FormerlySerializedAs("minSize")]   [SerializeField] private float _minSize = 2f;
    [FormerlySerializedAs("maxSize")]   [SerializeField] private float _maxSize = 15f;

    private Camera _cam;
    private Vector3 _originPos;
    private Vector3 _camRight;
    private Vector3 _camFwd;
    private bool _mousePanning;
    private bool _touchPanning;
    private Vector3 _grabWorldPoint;
    private int _panFingerId = -1;

    private static readonly Plane PanPlane = new Plane(Vector3.up, Vector3.zero);

    private void Awake()
    {
        _cam = GetComponent<Camera>();
        if (_cam == null) _cam = Camera.main;

        _originPos = transform.position;
        _camRight = transform.right;   _camRight.y = 0f; _camRight.Normalize();
        _camFwd   = transform.forward; _camFwd.y   = 0f; _camFwd.Normalize();
    }

    private void Update()
    {
        HandleKeyboardPan();
        HandleMousePan();
        HandleZoom();
        HandleTouchPan();
        HandlePinchZoom();
    }

    private void HandleKeyboardPan()
    {
        Vector3 dir = Vector3.zero;

        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) dir += transform.right;
        if (Input.GetKey(KeyCode.LeftArrow)  || Input.GetKey(KeyCode.A)) dir -= transform.right;
        if (Input.GetKey(KeyCode.UpArrow)    || Input.GetKey(KeyCode.W)) dir += transform.forward;
        if (Input.GetKey(KeyCode.DownArrow)  || Input.GetKey(KeyCode.S)) dir -= transform.forward;

        if (dir == Vector3.zero) return;

        dir.y = 0f;
        dir.Normalize();

        transform.position = ClampToMapBounds(transform.position + dir * _keyPanSpeed * Time.deltaTime);
    }

    private void HandleMousePan()
    {
        if (Input.GetMouseButtonDown(1))
        {
            if (TryGetWorldPoint(Input.mousePosition, out Vector3 wp))
                _grabWorldPoint = wp;
            _mousePanning = true;
        }

        if (Input.GetMouseButtonUp(1))
            _mousePanning = false;

        if (!_mousePanning) return;
        if (!TryGetWorldPoint(Input.mousePosition, out Vector3 current)) return;

        transform.position = ClampToMapBounds(transform.position + (_grabWorldPoint - current));
    }

    private void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) < 0.001f) return;

        _cam.orthographicSize = Mathf.Clamp(
            _cam.orthographicSize - scroll * _zoomSpeed,
            _minSize,
            _maxSize
        );
    }

    private void HandleTouchPan()
    {
        if (DraggableTile.IsDragging)
        {
            _touchPanning = false;
            _panFingerId = -1;
            return;
        }

        if (Input.touchCount != 1) return;

        Touch touch = Input.GetTouch(0);

        if (touch.phase == TouchPhase.Began)
        {
            _touchPanning = false;
            _panFingerId = -1;

            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                return;

            if (TryGetWorldPoint(touch.position, out Vector3 wp))
            {
                _grabWorldPoint = wp;
                _panFingerId = touch.fingerId;
                _touchPanning = true;
            }
        }
        else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
        {
            if (touch.fingerId == _panFingerId)
            {
                _touchPanning = false;
                _panFingerId = -1;
            }
        }
        else if (_touchPanning && touch.fingerId == _panFingerId)
        {
            if (!TryGetWorldPoint(touch.position, out Vector3 current)) return;

            transform.position = ClampToMapBounds(transform.position + (_grabWorldPoint - current));
        }
    }

    private void HandlePinchZoom()
    {
        if (Input.touchCount != 2) return;

        _touchPanning = false;

        Touch t0 = Input.GetTouch(0);
        Touch t1 = Input.GetTouch(1);

        float currentDist = Vector2.Distance(t0.position, t1.position);
        float prevDist    = Vector2.Distance(t0.position - t0.deltaPosition, t1.position - t1.deltaPosition);

        float delta = prevDist - currentDist;
        if (Mathf.Abs(delta) < 0.5f) return;

        _cam.orthographicSize = Mathf.Clamp(
            _cam.orthographicSize + delta * _zoomSpeed * 0.02f,
            _minSize,
            _maxSize
        );
    }

    private Vector3 ClampToMapBounds(Vector3 pos)
    {
        Vector3 offset = pos - _originPos;
        float right = Mathf.Clamp(Vector3.Dot(offset, _camRight), -_panBoundX, _panBoundX);
        float fwd   = Mathf.Clamp(Vector3.Dot(offset, _camFwd),   -_panBoundZ, _panBoundZ);
        return _originPos + _camRight * right + _camFwd * fwd;
    }

    private bool TryGetWorldPoint(Vector2 screenPos, out Vector3 worldPoint)
    {
        Ray ray = _cam.ScreenPointToRay(screenPos);
        if (PanPlane.Raycast(ray, out float enter))
        {
            worldPoint = ray.GetPoint(enter);
            return true;
        }
        worldPoint = Vector3.zero;
        return false;
    }
}
