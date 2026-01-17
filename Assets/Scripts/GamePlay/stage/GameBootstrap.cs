using System.Collections;
using UnityEngine;

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
        // 0) sanity
        if (templateService == null) { Debug.LogError("[GameBootstrap] templateService is null"); yield break; }
        if (tileFactory == null)     { Debug.LogError("[GameBootstrap] tileFactory is null"); yield break; }
        if (handController == null)  { Debug.LogError("[GameBootstrap] handController is null"); yield break; }
        if (mapController == null)   { Debug.LogError("[GameBootstrap] mapController is null"); yield break; }
        if (planetStateApi == null)  { Debug.LogError("[GameBootstrap] planetStateApi is null"); yield break; }

        // 1) templates
        yield return templateService.LoadTemplates();
        if (!templateService.IsReady)
        {
            Debug.LogError("[GameBootstrap] Templates not ready");
            yield break;
        }

        // ⭐ 1.5) ודאי שה־TileFactory משתמש באותו TemplateService שנטען
        tileFactory.templateService = templateService;

        // 2) planetId + stageId
        var planetId = AppSession.Instance?.ActivePlanet?.planetId;
        var stageId  = AppSession.Instance?.SelectedStageId;

        if (string.IsNullOrEmpty(planetId) || string.IsNullOrEmpty(stageId))
        {
            Debug.LogError($"[GameBootstrap] Missing planetId/stageId. planetId={planetId} stageId={stageId}");
            yield break;
        }

        // 3) GET stage-state
        PlanetStageStateDto state = null;
        string err = null;

        yield return StartCoroutine(planetStateApi.GetPlanetStageState(
            planetId, stageId, deckSize,
            AppSession.Instance.AccessToken,
            onOk: dto => state = dto,
            onErr: e => err = e
        ));

        if (!string.IsNullOrEmpty(err))
        {
            Debug.LogError(err);
            yield break;
        }
        if (state == null)
        {
            Debug.LogError("[GameBootstrap] state is null");
            yield break;
        }

        // 4) validate state parts (לא להפיל, רק להבין)
        if (state.map?.placedTiles == null) Debug.LogWarning("[GameBootstrap] state.map.placedTiles is null");
        if (state.hand == null) Debug.LogWarning("[GameBootstrap] state.hand is null");
        if (state.deck == null) Debug.LogWarning("[GameBootstrap] state.deck is null");

        // 5) Hook refs
        handController.factory = tileFactory;
        handController.mapController = mapController;

        // 6) Apply
        mapController.LoadPlacedTilesFromServer(state.map?.placedTiles, tileFactory);
        handController.LoadFromServer(state.hand, state.deck);

        // 7) Autosave ready
        FindObjectOfType<StageStateAutoSave>()?.SetReady(true);
    }
}
