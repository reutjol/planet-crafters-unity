using System.Collections;
using UnityEngine;
using System.Collections.Generic;


public class StageStateAutoSave : MonoBehaviour
{
    private bool readyToSave = false;
    public void SetReady(bool ready) => readyToSave = ready;
    [SerializeField] private MapController mapController;
    [SerializeField] private HandController handController;
    [SerializeField] private PlanetStateApiClient planetStateApi;

    [SerializeField] private float debounceSeconds = 0.25f;

    private Coroutine pendingSave;

    private void OnEnable()
    {
        mapController.OnMapStateChanged += RequestSave;
        handController.OnHandStateChanged += RequestSave;
    }

    private void OnDisable()
    {
        mapController.OnMapStateChanged -= RequestSave;
        handController.OnHandStateChanged -= RequestSave;
    }

    private void RequestSave()
    {
        if (!readyToSave) return;   

        if (pendingSave != null) StopCoroutine(pendingSave);
        pendingSave = StartCoroutine(SaveAfterDelay());
    }


    private IEnumerator SaveAfterDelay()
    {
        yield return new WaitForSeconds(debounceSeconds);
        pendingSave = null;

        var planetId = AppSession.Instance?.ActivePlanet?.planetId;
        var stageId  = AppSession.Instance?.SelectedStageId;

        if (string.IsNullOrEmpty(planetId) || string.IsNullOrEmpty(stageId)) yield break;

        // build full snapshot
       var reqDto = new SaveStageStateRequestDto
        {
            planetId = planetId,
            stageId = stageId,
            state = new StageStateInnerDto
            {
                map = new MapDto { placedTiles = new List<PlacedTileDto>(mapController.GetPlacedTiles()) },
                hand = handController.BuildHandDto(),
                deck = handController.BuildDeckDto(),
                progress = new ProgressDto { developedPercent = 0f, score = 0, isCompleted = false }
            }
        };


        string err = null;

        yield return StartCoroutine(planetStateApi.SavePlanetStageState(
            planetId, stageId,
            AppSession.Instance.AccessToken,
            reqDto,
            onOk: () => Debug.Log("[AutoSave] saved"),
            onErr: e => err = e
        ));

        if (!string.IsNullOrEmpty(err))
            Debug.LogError("[AutoSave] " + err);
    }
   
}
