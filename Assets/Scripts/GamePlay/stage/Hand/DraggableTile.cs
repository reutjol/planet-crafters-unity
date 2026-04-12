using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Handles tile drag-and-drop gameplay for placing tiles on the hex map.
/// Supports rotation with 'R' key and snapping to hex cells.
/// On drop, sends the action to the server; server validates and returns updated state.
/// </summary>
public class DraggableTile : MonoBehaviour
{
    [Header("Refs (Injected by HandController)")]
    public HandController handController;
    public MapController mapController;

    [Header("Drag Settings")]
    public LayerMask hexCellMask;
    public string templateId;

    private Camera cam;
    private bool dragging;
    private HexCell hoveredCell;

    private Transform homeParent;
    private Vector3 homeLocalPos;

    private int rotation = 0;
    private Collider[] myColliders;

    // ===============================
    // Init
    // ===============================
    private void Awake()
    {
        cam = Camera.main;
        myColliders = GetComponentsInChildren<Collider>(true);
    }

    // Called by HandController
    public void SetHome(Transform parent)
    {
        homeParent = parent;
        homeLocalPos = Vector3.zero;
    }

    // Called by HandController
    public void SetDraggable(bool canDrag)
    {
        enabled = canDrag;
    }

    // ===============================
    // Mouse
    // ===============================
    private void OnMouseDown()
    {
        if (!enabled) return;
        if (PopupManager.IsAnyPopupOpen) return;

        dragging = true;

        // Switch to Default layer so Main Camera renders it during drag
        // (HandCamera is fixed; dragging must follow Main Camera's raycast)
        SetLayerRecursively(gameObject, LayerMask.NameToLayer("Default"));

        // Refresh and disable all colliders (includes resource prefabs spawned after Awake)
        myColliders = GetComponentsInChildren<Collider>(true);
        foreach (var c in myColliders)
            c.enabled = false;

        transform.SetParent(null, true);
    }

    private void Update()
    {
        if (!dragging) return;

        // Rotation with R key
        if (Input.GetKeyDown(KeyCode.R))
        {
            rotation = (rotation + 1) % 6;
            transform.rotation = Quaternion.Euler(0, rotation * 60f, 0);
        }

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        // First try to snap to a hex cell
        if (Physics.Raycast(ray, out RaycastHit hit, 200f, hexCellMask))
        {
            HexCell cell = hit.collider.GetComponentInParent<HexCell>();
            if (cell != null)
            {
                if (hoveredCell != cell)
                {
                    if (hoveredCell != null)
                        hoveredCell.SetHighlight(false);

                    hoveredCell = cell;
                    hoveredCell.SetHighlight(true);
                }

                Vector3 snap = cell.transform.position;
                snap.y = mapController.tileHeightY;
                transform.position = snap;
                return;
            }
        }

        // Fallback: free drag on plane
        Plane plane = new Plane(Vector3.up, new Vector3(0, mapController.tileHeightY, 0));
        if (plane.Raycast(ray, out float enter))
        {
            transform.position = ray.GetPoint(enter);
        }
    }

    private void OnMouseUp()
    {
        dragging = false;

        // Re-enable colliders
        foreach (var c in myColliders)
            c.enabled = true;

        if (hoveredCell != null &&
            hoveredCell.isPlusCell &&
            !hoveredCell.occupied)
        {
            int q = hoveredCell.q;
            int r = hoveredCell.r;

            hoveredCell.SetHighlight(false);
            hoveredCell = null;

            // Disable while waiting for server response
            enabled = false;

            StartCoroutine(SendPlaceTileToServer(q, r, rotation));
            return;
        }

        ReturnHome();
    }

    // ===============================
    // Server call
    // ===============================
    private IEnumerator SendPlaceTileToServer(int q, int r, int rot)
    {
        var planetId = AppSession.Instance?.ActivePlanet?.planetId;
        var stageId = AppSession.Instance?.SelectedStageId;
        var token = AppSession.Instance?.AccessToken;

        if (string.IsNullOrEmpty(planetId) || string.IsNullOrEmpty(stageId) || string.IsNullOrEmpty(token))
        {
            Debug.LogError("[DraggableTile] Missing session data for place-tile request");
            ReturnHome();
            enabled = true;
            yield break;
        }

        var dto = new PlaceTileRequestDto
        {
            tileId = templateId,
            coord = new CoordDto { q = q, r = r },
            rotation = rot
        };

        PlanetStageStateDto newState = null;
        string error = null;

        yield return PlanetStateApiClient.Instance.PlaceTile(
            planetId, stageId, token, dto,
            onSuccess: state => newState = state,
            onError: err => error = err
        );

        if (newState != null)
        {
            // Re-render map and hand from server state
            // LoadFromServer will destroy this GameObject via ClearHandVisuals (tiles[0])
            mapController.ApplyServerState(newState);
            handController.LoadFromServer(newState.hand, newState.deck);
        }
        else
        {
            Debug.LogError($"[DraggableTile] Place tile failed: {error}");
            ReturnHome();
            enabled = true;
        }
    }

    // ===============================
    // Helpers
    // ===============================
    private void ReturnHome()
    {
        transform.SetParent(homeParent, true);
        transform.localPosition = homeLocalPos;
        transform.localRotation = Quaternion.identity;
        rotation = 0;

        // Back in slot — restore Hand layer so HandCamera renders it on top
        SetLayerRecursively(gameObject, LayerMask.NameToLayer("Hand"));
    }

    private static void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
            SetLayerRecursively(child.gameObject, layer);
    }
}
