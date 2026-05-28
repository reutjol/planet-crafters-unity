using System.Collections;
using UnityEngine;

public class DraggableTile : MonoBehaviour
{
    [SerializeField] private LayerMask _hexCellMask;

    private Camera _cam;
    private bool _dragging;
    private HexCell _hoveredCell;

    private Transform _homeParent;
    private Vector3 _homeLocalPos;

    private int _rotation = 0;
    private Collider[] _myColliders;
    private HexTileView _hexTileView;

    private HandController _handController;
    private MapController _mapController;
    private string _templateId;

    public void Initialize(string templateId, HandController hand, MapController map)
    {
        _templateId = templateId;
        _handController = hand;
        _mapController = map;
    }

    private void Awake()
    {
        _cam = Camera.main;
        _myColliders = GetComponentsInChildren<Collider>(true);
        _hexTileView = GetComponent<HexTileView>();
    }

    public void SetHome(Transform parent)
    {
        _homeParent = parent;
        _homeLocalPos = Vector3.zero;
    }

    public void SetDraggable(bool canDrag)
    {
        enabled = canDrag;
    }

    private void OnMouseDown()
    {
        if (!enabled) return;
        if (PopupManager.IsAnyPopupOpen) return;

        _dragging = true;

        SetLayerRecursively(gameObject, LayerMask.NameToLayer("Default"));

        foreach (var c in _myColliders)
            c.enabled = false;

        transform.SetParent(null, true);
    }

    private void Update()
    {
        if (!_dragging) return;

        if (Input.GetKeyDown(KeyCode.R))
        {
            _rotation = (_rotation + 1) % 6;
            transform.rotation = Quaternion.Euler(0, _rotation * 60f, 0);
        }

        Ray ray = _cam.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 200f, _hexCellMask))
        {
            if (HexCell.TryGetFromCollider(hit.collider, out HexCell cell))
            {
                if (_hoveredCell != cell)
                {
                    if (_hoveredCell != null)
                        _hoveredCell.SetHighlight(false);

                    _hoveredCell = cell;
                    _hoveredCell.SetHighlight(true);
                }

                Vector3 snap = cell.transform.position;
                snap.y = _mapController.TileHeightY;
                transform.position = snap;
                return;
            }
        }

        Plane plane = new Plane(Vector3.up, new Vector3(0, _mapController.TileHeightY, 0));
        if (plane.Raycast(ray, out float enter))
            transform.position = ray.GetPoint(enter);
    }

    private void OnMouseUp()
    {
        _dragging = false;

        foreach (var c in _myColliders)
            c.enabled = true;

        if (_hoveredCell != null &&
            _hoveredCell.isPlusCell &&
            !_hoveredCell.occupied)
        {
            int q = _hoveredCell.q;
            int r = _hoveredCell.r;

            _hoveredCell.SetHighlight(false);
            _hoveredCell = null;

            enabled = false;
            StartCoroutine(SendPlaceTileToServer(q, r, _rotation));
            return;
        }

        ReturnHome();
    }

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

        int serverRot = ((rot % 6) + 6) % 6;
        var dto = new PlaceTileRequestDto
        {
            tileId = _templateId,
            coord = new CoordDto { q = q, r = r },
            rotation = serverRot
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
            _mapController.ApplyServerState(newState);
            _handController.LoadFromServer(newState.hand, newState.deck);
        }
        else
        {
            Debug.LogError($"[DraggableTile] Place tile failed: {error}");
            ReturnHome();
            enabled = true;
        }
    }

    private void ReturnHome()
    {
        transform.SetParent(_homeParent, true);
        transform.localPosition = _homeLocalPos;
        transform.localRotation = Quaternion.identity;
        _rotation = 0;

        SetLayerRecursively(gameObject, LayerMask.NameToLayer("Hand"));
    }

    private static void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
            SetLayerRecursively(child.gameObject, layer);
    }

}
