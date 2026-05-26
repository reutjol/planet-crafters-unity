using UnityEngine;
using UnityEngine.Serialization;

public class MapCameraController : MonoBehaviour
{
    [Header("Pan Bounds (camera XZ position clamp)")]
    [FormerlySerializedAs("panBoundX")] [SerializeField] private float _panBoundX = 25f;
    [FormerlySerializedAs("panBoundZ")] [SerializeField] private float _panBoundZ = 25f;

    [Header("Keyboard Pan")]
    [FormerlySerializedAs("keyPanSpeed")] [SerializeField] private float _keyPanSpeed = 8f;

    [Header("Zoom (Orthographic Size)")]
    [FormerlySerializedAs("zoomSpeed")] [SerializeField] private float _zoomSpeed = 2f;
    [FormerlySerializedAs("minSize")]   [SerializeField] private float _minSize = 2f;
    [FormerlySerializedAs("maxSize")]   [SerializeField] private float _maxSize = 15f;

    [Header("Sync")]
    [SerializeField] private Camera _handCamera;

    private Camera _cam;
    private bool _panning;
    private Vector3 _grabWorldPoint;

    private static readonly Plane PanPlane = new Plane(Vector3.up, Vector3.zero);

    private void Awake()
    {
        _cam = GetComponent<Camera>();
        if (_cam == null) _cam = Camera.main;
    }

    private void Update()
    {
        HandleKeyboardPan();
        HandleMousePan();
        HandleZoom();
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
        if (dir.sqrMagnitude > 0f) dir.Normalize();

        Vector3 newPos = transform.position + dir * _keyPanSpeed * Time.deltaTime;
        newPos.x = Mathf.Clamp(newPos.x, -_panBoundX, _panBoundX);
        newPos.z = Mathf.Clamp(newPos.z, -_panBoundZ, _panBoundZ);
        transform.position = newPos;
    }

    private void HandleMousePan()
    {
        if (Input.GetMouseButtonDown(1))
        {
            if (TryGetWorldPoint(Input.mousePosition, out Vector3 wp))
                _grabWorldPoint = wp;
            _panning = true;
        }

        if (Input.GetMouseButtonUp(1))
            _panning = false;

        if (!_panning) return;
        if (!TryGetWorldPoint(Input.mousePosition, out Vector3 current)) return;

        Vector3 delta = _grabWorldPoint - current;
        Vector3 newPos = transform.position + delta;
        newPos.x = Mathf.Clamp(newPos.x, -_panBoundX, _panBoundX);
        newPos.z = Mathf.Clamp(newPos.z, -_panBoundZ, _panBoundZ);
        transform.position = newPos;
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
