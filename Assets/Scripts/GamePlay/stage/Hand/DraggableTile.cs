using UnityEngine;

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

    // נקרא מה-HandController
    public void SetHome(Transform parent)
    {
        homeParent = parent;
        homeLocalPos = Vector3.zero;
    }

    // נקרא מה-HandController
    public void SetDraggable(bool canDrag)
    {
        enabled = canDrag;
    }

    // נועל אחרי הנחה
    public void LockPlaced()
    {
        enabled = false;
    }

    // ===============================
    // Mouse
    // ===============================
    private void OnMouseDown()
    {
        if (!enabled) return;

        dragging = true;

        // לכבות colliders כדי לא לחסום Raycast
        foreach (var c in myColliders)
            c.enabled = false;

        transform.SetParent(null, true);
    }

    private void Update()
    {
        if (!dragging) return;

        // סיבוב
        if (Input.GetKeyDown(KeyCode.R))
        {
            rotation = (rotation + 1) % 6;
            transform.rotation = Quaternion.Euler(0, rotation * 60f, 0);
        }

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        // קודם מנסים תא
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

        // fallback – גרירה חופשית על מישור
        Plane plane = new Plane(Vector3.up, new Vector3(0, mapController.tileHeightY, 0));
        if (plane.Raycast(ray, out float enter))
        {
            transform.position = ray.GetPoint(enter);
        }
    }

    private void OnMouseUp()
    {
        dragging = false;

        // להחזיר colliders
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

            bool placed = mapController.TryPlaceTile(q, r, rotation, gameObject, templateId);
            if (placed)
            {
                LockPlaced(); 
                handController.OnTilePlacedFromSlot1(this);
                return;
            }

        }
        ReturnHome();
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
    }
}
