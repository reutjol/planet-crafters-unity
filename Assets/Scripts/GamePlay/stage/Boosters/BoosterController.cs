using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages booster inventory display and activation during single-player gameplay.
/// Place one instance in the gameplay scene. Wire up the 3 booster buttons and count labels in the inspector.
/// </summary>
public class BoosterController : MonoBehaviour
{
    public static BoosterController Instance { get; private set; }

    [Header("Double Score Booster")]
    [SerializeField] private Button _doubleScoreButton;
    [SerializeField] private TMP_Text _doubleScoreCountText;

    [Header("Cancel Placement Booster")]
    [SerializeField] private Button _cancelPlacementButton;
    [SerializeField] private TMP_Text _cancelPlacementCountText;

    [Header("Add Hex Booster")]
    [SerializeField] private Button _addHexButton;
    [SerializeField] private TMP_Text _addHexCountText;

    private BoosterInventoryDto _inventory = new BoosterInventoryDto();
    private bool _doubleScoreActive = false;

    public bool IsDoubleScoreActive => _doubleScoreActive;

    private MapController _mapController;
    private HandController _handController;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticState() => Instance = null;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnEnable()
    {
        if (_doubleScoreButton != null)
            _doubleScoreButton.onClick.AddListener(HandleDoubleScoreClicked);

        if (_cancelPlacementButton != null)
            _cancelPlacementButton.onClick.AddListener(HandleCancelPlacementClicked);

        if (_addHexButton != null)
            _addHexButton.onClick.AddListener(HandleAddHexClicked);
    }

    private void OnDisable()
    {
        if (_doubleScoreButton != null)
            _doubleScoreButton.onClick.RemoveListener(HandleDoubleScoreClicked);

        if (_cancelPlacementButton != null)
            _cancelPlacementButton.onClick.RemoveListener(HandleCancelPlacementClicked);

        if (_addHexButton != null)
            _addHexButton.onClick.RemoveListener(HandleAddHexClicked);
    }

    public void Initialize(MapController mapController, HandController handController)
    {
        _mapController = mapController;
        _handController = handController;
        StartCoroutine(FetchInventory());
    }

    private IEnumerator FetchInventory()
    {
        var token = AppSession.Instance?.AccessToken;
        var api = GameManager.Instance?.BoosterApi;
        if (string.IsNullOrEmpty(token) || api == null)
            yield break;

        yield return api.GetBoosters(
            token,
            onSuccess: inventory => RefreshFromInventory(inventory),
            onError: err => Debug.LogWarning($"[BoosterController] Failed to fetch boosters: {err}")
        );
    }

    public void RefreshFromInventory(BoosterInventoryDto inventory)
    {
        _inventory = inventory;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (_doubleScoreCountText != null)
            _doubleScoreCountText.text = _inventory.doubleScore.ToString();
        if (_cancelPlacementCountText != null)
            _cancelPlacementCountText.text = _inventory.cancelPlacement.ToString();
        if (_addHexCountText != null)
            _addHexCountText.text = _inventory.addHex.ToString();

        if (_doubleScoreButton != null)
            _doubleScoreButton.interactable = _inventory.doubleScore > 0;
        if (_cancelPlacementButton != null)
            _cancelPlacementButton.interactable = _inventory.cancelPlacement > 0;
        if (_addHexButton != null)
            _addHexButton.interactable = _inventory.addHex > 0;

        // Highlight the double score button when active
        if (_doubleScoreButton != null)
        {
            var colors = _doubleScoreButton.colors;
            colors.normalColor = _doubleScoreActive ? Color.yellow : Color.white;
            _doubleScoreButton.colors = colors;
        }
    }

    // ── Booster handlers ──────────────────────────────────────────────────────

    private void HandleDoubleScoreClicked()
    {
        if (_inventory.doubleScore <= 0) return;
        _doubleScoreActive = !_doubleScoreActive;
        UpdateUI();
    }

    /// Called by DraggableTile after a successful placement when doubleScore was active.
    public void OnDoubleScoreConsumed()
    {
        _doubleScoreActive = false;
        _inventory.doubleScore = Mathf.Max(0, _inventory.doubleScore - 1);
        UpdateUI();
    }

    private void HandleCancelPlacementClicked()
    {
        if (_inventory.cancelPlacement <= 0) return;
        StartCoroutine(ExecuteCancelPlacement());
    }

    private IEnumerator ExecuteCancelPlacement()
    {
        SetAllButtonsInteractable(false);

        var token = AppSession.Instance?.AccessToken;
        var planetId = AppSession.Instance?.ActivePlanet?.planetId;
        var stageId = AppSession.Instance?.SelectedStageId;

        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(planetId) || string.IsNullOrEmpty(stageId))
        {
            Debug.LogWarning("[BoosterController] Missing session data for cancel placement");
            SetAllButtonsInteractable(true);
            yield break;
        }

        var cancelApi = GameManager.Instance?.BoosterApi;
        if (cancelApi == null)
        {
            Debug.LogWarning("[BoosterController] BoosterApi not available for CancelLastTile");
            SetAllButtonsInteractable(true);
            yield break;
        }

        yield return cancelApi.CancelLastTile(
            planetId, stageId, token,
            onSuccess: state =>
            {
                _inventory.cancelPlacement = Mathf.Max(0, _inventory.cancelPlacement - 1);
                _mapController?.ApplyServerState(state);
                _handController?.LoadFromServer(state.hand, state.deck);
                AchievementNotifier.Notify(state.achievementRewards);
                UpdateUI();
            },
            onError: err =>
            {
                Debug.LogError($"[BoosterController] CancelTile failed: {err}");
                SetAllButtonsInteractable(true);
                UpdateUI();
            }
        );
    }

    private void HandleAddHexClicked()
    {
        if (_inventory.addHex <= 0) return;
        StartCoroutine(ExecuteAddHex());
    }

    private IEnumerator ExecuteAddHex()
    {
        SetAllButtonsInteractable(false);

        var token = AppSession.Instance?.AccessToken;
        var planetId = AppSession.Instance?.ActivePlanet?.planetId;
        var stageId = AppSession.Instance?.SelectedStageId;

        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(planetId) || string.IsNullOrEmpty(stageId))
        {
            Debug.LogWarning("[BoosterController] Missing session data for add hex");
            SetAllButtonsInteractable(true);
            yield break;
        }

        var addHexApi = GameManager.Instance?.BoosterApi;
        if (addHexApi == null)
        {
            Debug.LogWarning("[BoosterController] BoosterApi not available for AddHexToHand");
            SetAllButtonsInteractable(true);
            yield break;
        }

        yield return addHexApi.AddHexToHand(
            planetId, stageId, token,
            onSuccess: state =>
            {
                _inventory.addHex = Mathf.Max(0, _inventory.addHex - 1);
                _handController?.LoadFromServer(state.hand, state.deck);
                AchievementNotifier.Notify(state.achievementRewards);
                UpdateUI();
            },
            onError: err =>
            {
                Debug.LogError($"[BoosterController] AddHex failed: {err}");
                SetAllButtonsInteractable(true);
                UpdateUI();
            }
        );
    }

    private void SetAllButtonsInteractable(bool value)
    {
        if (_doubleScoreButton != null) _doubleScoreButton.interactable = value && _inventory.doubleScore > 0;
        if (_cancelPlacementButton != null) _cancelPlacementButton.interactable = value && _inventory.cancelPlacement > 0;
        if (_addHexButton != null) _addHexButton.interactable = value && _inventory.addHex > 0;
    }
}
