using System.Collections;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Automatically saves stage state to the server when map or hand changes
/// Uses debouncing to avoid excessive API calls
/// </summary>
public class StageStateAutoSave : MonoBehaviour
{
    [SerializeField] private MapController mapController;
    [SerializeField] private HandController handController;
    [SerializeField] private float debounceSeconds = 0.25f;

    private bool readyToSave = false;
    private Coroutine pendingSave;

    public void SetReady(bool ready)
    {
        readyToSave = ready;
        Debug.Log($"[AutoSave] Ready to save: {ready}");
    }

    private void OnEnable()
    {
        if (mapController != null)
            mapController.OnMapStateChanged += RequestSave;
        if (handController != null)
            handController.OnHandStateChanged += RequestSave;
    }

    private void OnDisable()
    {
        if (mapController != null)
            mapController.OnMapStateChanged -= RequestSave;
        if (handController != null)
            handController.OnHandStateChanged -= RequestSave;
    }

    private void RequestSave()
    {
        if (!readyToSave)
        {
            Debug.Log("[AutoSave] Not ready to save yet, skipping");
            return;
        }

        if (GameManager.Instance == null)
        {
            Debug.LogError("[AutoSave] GameManager.Instance is null!");
            return;
        }

        // Cancel any pending save and restart the timer
        if (pendingSave != null)
            StopCoroutine(pendingSave);

        pendingSave = StartCoroutine(SaveAfterDelay());
    }

    private IEnumerator SaveAfterDelay()
    {
        yield return new WaitForSeconds(debounceSeconds);
        pendingSave = null;

        // Validate session
        var planetId = AppSession.Instance?.ActivePlanet?.planetId;
        var stageId = AppSession.Instance?.SelectedStageId;

        if (string.IsNullOrEmpty(planetId) || string.IsNullOrEmpty(stageId))
        {
            Debug.LogWarning("[AutoSave] Missing planetId or stageId, cannot save");
            yield break;
        }

        // Build state snapshot
        var stateDto = new SaveStageStateRequestDto
        {
            planetId = planetId,
            stageId = stageId,
            state = new StageStateInnerDto
            {
                map = new MapDto
                {
                    placedTiles = new List<PlacedTileDto>(mapController.GetPlacedTiles())
                },
                hand = handController.BuildHandDto(),
                deck = handController.BuildDeckDto(),
                progress = new ProgressDto
                {
                    developedPercent = 0f,
                    score = 0,
                    isCompleted = false
                }
            }
        };

        Debug.Log("[AutoSave] Saving state...");

        // Use GameManager to save
        bool saveDone = false;
        bool saveSuccess = false;

        GameManager.Instance.SavePlanetStageState(
            stateDto,
            onSuccess: () =>
            {
                saveDone = true;
                saveSuccess = true;
            },
            onError: (err) =>
            {
                saveDone = true;
                saveSuccess = false;
                Debug.LogError($"[AutoSave] Save failed: {err}");
            }
        );

        // Wait for save to complete
        float timeout = 5f;
        float elapsed = 0f;

        while (!saveDone && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (!saveDone)
        {
            Debug.LogError("[AutoSave] Save timed out");
        }
        else if (saveSuccess)
        {
            Debug.Log("[AutoSave] State saved successfully");
        }
    }
}
