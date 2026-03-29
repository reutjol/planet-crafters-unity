using UnityEngine;

/// <summary>
/// Camera controller for the gameplay scene.
/// Works with an Orthographic camera at any rotation (isometric, top-down, etc.).
///
/// Controls:
///   Pan  — Arrow keys / WASD  OR  hold Right Mouse Button and drag
///   Zoom — Mouse Scroll Wheel (changes orthographicSize)
/// </summary>
public class MapCameraController : MonoBehaviour
{
    [Header("Pan Bounds (camera XZ position clamp)")]
    [Tooltip("Max camera X position offset from origin (±)")]
    public float panBoundX = 25f;
    [Tooltip("Max camera Z position offset from origin (±)")]
    public float panBoundZ = 25f;

    [Header("Keyboard Pan")]
    [Tooltip("Camera pan speed with arrow keys / WASD (world units per second)")]
    public float keyPanSpeed = 8f;

    [Header("Zoom (Orthographic Size)")]
    [Tooltip("How fast the scroll wheel zooms")]
    public float zoomSpeed = 2f;
    [Tooltip("Minimum orthographic size (most zoomed in)")]
    public float minSize = 2f;
    [Tooltip("Maximum orthographic size (most zoomed out)")]
    public float maxSize = 15f;

    [Header("Sync")]
    [Tooltip("HandCamera — will mirror orthographicSize so hand tiles scale with the map")]
    [SerializeField] private Camera handCamera;

    // -------------------------------------------------------
    private Camera cam;
    private bool panning;
    private Vector3 grabWorldPoint;

    // Pan plane: flat XZ at y=0 (where the hex map lives)
    private static readonly Plane panPlane = new Plane(Vector3.up, Vector3.zero);

    // -------------------------------------------------------
    private void Awake()
    {
        cam = GetComponent<Camera>();
        if (cam == null) cam = Camera.main;
    }

    private void Update()
    {
        HandleKeyboardPan();
        HandleMousePan();
        HandleZoom();
        SyncHandCamera();
    }

    // -------------------------------------------------------
    // Keyboard pan — Arrow keys or WASD
    // Moves camera along its XZ-projected right/forward vectors.
    // -------------------------------------------------------
    private void HandleKeyboardPan()
    {
        Vector3 dir = Vector3.zero;

        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) dir += transform.right;
        if (Input.GetKey(KeyCode.LeftArrow)  || Input.GetKey(KeyCode.A)) dir -= transform.right;
        if (Input.GetKey(KeyCode.UpArrow)    || Input.GetKey(KeyCode.W)) dir += transform.forward;
        if (Input.GetKey(KeyCode.DownArrow)  || Input.GetKey(KeyCode.S)) dir -= transform.forward;

        if (dir == Vector3.zero) return;

        dir.y = 0f; // keep movement flat on XZ
        if (dir.sqrMagnitude > 0f) dir.Normalize();

        Vector3 newPos = transform.position + dir * keyPanSpeed * Time.deltaTime;
        newPos.x = Mathf.Clamp(newPos.x, -panBoundX, panBoundX);
        newPos.z = Mathf.Clamp(newPos.z, -panBoundZ, panBoundZ);
        transform.position = newPos;
    }

    // -------------------------------------------------------
    // Mouse pan — right-mouse drag
    // Raycasts to XZ plane (Y=0) so the world point grabbed on
    // mouse-down stays exactly under the cursor ("grab world" feel).
    // Works correctly with any camera rotation.
    // -------------------------------------------------------
    private void HandleMousePan()
    {
        if (Input.GetMouseButtonDown(1))
        {
            if (TryGetWorldPoint(Input.mousePosition, out Vector3 wp))
                grabWorldPoint = wp;
            panning = true;
        }

        if (Input.GetMouseButtonUp(1))
            panning = false;

        if (!panning) return;

        if (!TryGetWorldPoint(Input.mousePosition, out Vector3 current)) return;

        // Move camera so the grabbed world point stays under the cursor
        Vector3 delta = grabWorldPoint - current; // delta.y == 0 (both on Y=0 plane)
        Vector3 newPos = transform.position + delta;

        // Clamp camera XZ position (Y stays unchanged — camera keeps its angle/height)
        newPos.x = Mathf.Clamp(newPos.x, -panBoundX, panBoundX);
        newPos.z = Mathf.Clamp(newPos.z, -panBoundZ, panBoundZ);
        transform.position = newPos;
    }

    // -------------------------------------------------------
    // Zoom — scroll wheel changes orthographicSize
    // Scroll up = smaller size = zoom in
    // Scroll down = larger size = zoom out
    // -------------------------------------------------------
    private void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) < 0.001f) return;

        float newSize = Mathf.Clamp(
            cam.orthographicSize - scroll * zoomSpeed,
            minSize,
            maxSize
        );

        cam.orthographicSize = newSize;
    }

    // -------------------------------------------------------
    // Sync HandCamera position + size to match Main Camera
    // -------------------------------------------------------
    private void SyncHandCamera()
    {
        // Hand camera stays fully fixed — position and size unchanged.
        // Hand tiles always appear at the same screen location and scale.
    }

    // -------------------------------------------------------
    // Raycast from screen position to the XZ pan plane (Y=0)
    // -------------------------------------------------------
    private bool TryGetWorldPoint(Vector2 screenPos, out Vector3 worldPoint)
    {
        Ray ray = cam.ScreenPointToRay(screenPos);
        if (panPlane.Raycast(ray, out float enter))
        {
            worldPoint = ray.GetPoint(enter);
            return true;
        }
        worldPoint = Vector3.zero;
        return false;
    }
}
