using System;
using System.Collections;
using UnityEngine;

public class DraggableTile : MonoBehaviour
{
    [SerializeField] private LayerMask _hexCellMask;
    [SerializeField] private float _touchOffsetY = 150f;

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
    private int _activeFingerId = -1;

    public static bool IsDragging { get; private set; }
    public static DraggableTile ActiveTile { get; private set; }
    public static event Action<bool> OnDragStateChanged;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticState()
    {
        IsDragging = false;
        ActiveTile = null;
        OnDragStateChanged = null;
    }

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

    // Called by the rotate UI button (and R key on PC).
    public void Rotate()
    {
        _rotation = (_rotation + 1) % 6;
        transform.rotation = Quaternion.Euler(0, _rotation * 60f, 0);
    }

    private void OnMouseDown()
    {
        if (!enabled) return;
        if (PopupManager.IsAnyPopupOpen) return;

        _activeFingerId = -1;
        for (int i = 0; i < Input.touchCount; i++)
        {
            if (Input.GetTouch(i).phase == UnityEngine.TouchPhase.Began)
            {
                _activeFingerId = Input.GetTouch(i).fingerId;
                break;
            }
        }

        _dragging = true;
        IsDragging = true;
        ActiveTile = this;
        OnDragStateChanged?.Invoke(true);

        SetLayerRecursively(gameObject, LayerMask.NameToLayer("Default"));

        foreach (var c in _myColliders)
            c.enabled = false;

        transform.SetParent(null, true);
    }

    private void Update()
    {
        if (!_dragging) return;

        if (Input.GetKeyDown(KeyCode.R))
            Rotate();

        Vector2 screenPos = GetPointerScreenPosition();
        Ray ray = _cam.ScreenPointToRay(screenPos);

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

    // Follows the specific finger that started the drag, or falls back to mouse position.
    // On touch, shifts the ray upward by _touchOffsetY pixels so the tile appears above the finger.
    private Vector2 GetPointerScreenPosition()
    {
        Vector2 touchOffset = Input.touchCount > 0 ? Vector2.up * _touchOffsetY : Vector2.zero;

        if (_activeFingerId >= 0)
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                var t = Input.GetTouch(i);
                if (t.fingerId == _activeFingerId)
                    return t.position + touchOffset;
            }
        }
        if (Input.touchCount > 0)
            return Input.GetTouch(0).position + touchOffset;
        return Input.mousePosition;
    }

    private void OnMouseUp()
    {
        _dragging = false;
        IsDragging = false;
        ActiveTile = null;
        OnDragStateChanged?.Invoke(false);

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
        bool usingDoubleScore = BoosterController.Instance != null && BoosterController.Instance.IsDoubleScoreActive;
        var dto = new PlaceTileRequestDto
        {
            tileId = _templateId,
            coord = new CoordDto { q = q, r = r },
            rotation = serverRot,
            activeBooster = usingDoubleScore ? "doubleScore" : null
        };

        var placeApi = PlanetStateApiClient.Instance;
        if (placeApi == null)
        {
            Debug.LogError("[DraggableTile] PlanetStateApiClient.Instance is null — cannot place tile");
            ReturnHome();
            enabled = true;
            yield break;
        }

        PlanetStageStateDto newState = null;
        string error = null;

        yield return placeApi.PlaceTile(
            planetId, stageId, token, dto,
            onSuccess: state => newState = state,
            onError: err => error = err
        );

        if (newState != null)
        {
            if (usingDoubleScore)
                BoosterController.Instance?.OnDoubleScoreConsumed();

            AchievementNotifier.Notify(newState.achievementRewards);

            _mapController.ApplyServerState(newState);
            _handController.LoadFromServer(newState.hand, newState.deck);

            if (newState.userCoins.HasValue && newState.userCoins.Value > 0)
            {
                UserCoinsDisplay.UpdateCoins(newState.userCoins.Value);
                if (AppSession.Instance?.ActivePlanet != null)
                    AppSession.Instance.ActivePlanet.totalCoins = newState.userCoins.Value;
            }
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
