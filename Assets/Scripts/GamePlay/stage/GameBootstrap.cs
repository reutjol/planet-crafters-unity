using UnityEngine;
using System.Collections;

public class GameBootstrap : MonoBehaviour
{
    [SerializeField] private HexTileTemplateService templateService;
    [SerializeField] private TileFactory tileFactory;
    [SerializeField] private HandController handController;
    [SerializeField] private MapController mapController;
    [SerializeField] private PlanetStateApiClient planetStateApi;

    [SerializeField] private int deckSize = 30;

    private IEnumerator Start()
    {
        // 1) templates
        yield return templateService.LoadTemplates();
        if (!templateService.IsReady) yield break;

        // 2) זיהוי planetId + stageId
        var planetId = AppSession.Instance?.ActivePlanet?.planetId; // או ממה שאת שומרת
        var stageId  = AppSession.Instance?.SelectedStageId;

        if (string.IsNullOrEmpty(planetId) || string.IsNullOrEmpty(stageId))
        {
            Debug.LogError($"Missing planetId/stageId. planetId={planetId} stageId={stageId}");
            yield break;
        }

        // 3) GET planet-state
        PlanetStageStateDto state = null;
        string err = null;

        yield return StartCoroutine(planetStateApi.GetPlanetStageState(
            planetId, stageId, deckSize,
            AppSession.Instance.AccessToken,
            onOk: (dto) => state = dto,
            onErr: (e) => err = e
        ));

        if (!string.IsNullOrEmpty(err))
        {
            Debug.LogError(err);
            yield break;
        }

        // 4) apply state to game
        handController.factory = tileFactory;
        handController.mapController = mapController;
        // 4) apply state to game
        if (handController == null) { Debug.LogError("[GameBootstrap] handController is null"); yield break; }
        if (mapController == null)  { Debug.LogError("[GameBootstrap] mapController is null");  yield break; }
        if (tileFactory == null)    { Debug.LogError("[GameBootstrap] tileFactory is null");    yield break; }
        if (templateService == null){ Debug.LogError("[GameBootstrap] templateService is null");yield break; }

        if (state == null)
        {
            Debug.LogError("[GameBootstrap] state is null (API returned ok but dto wasn't set?)");
            yield break;
        }

        // ⬇️ אם זה נופל אצלך בשורה 51, זה כנראה אחד מאלה:
        if (state.map == null)  Debug.LogWarning("[GameBootstrap] state.map is null");
        if (state.hand == null) Debug.LogWarning("[GameBootstrap] state.hand is null");
        if (state.deck == null) Debug.LogWarning("[GameBootstrap] state.deck is null");

        // Hook refs
        handController.factory = tileFactory;
        handController.mapController = mapController;

        var placedTiles = state.map?.placedTiles;   
        mapController.LoadPlacedTilesFromServer(placedTiles, tileFactory);

        handController.LoadFromServer(state.hand, state.deck);

        mapController.LoadPlacedTilesFromServer(state.map.placedTiles, tileFactory);
        handController.LoadFromServer(state.hand, state.deck);              
        FindObjectOfType<StageStateAutoSave>()?.SetReady(true);

    }
}
