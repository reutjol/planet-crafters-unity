using System.Collections;
using UnityEngine;

public class RestartStageButton : MonoBehaviour
{
    [SerializeField] private PlanetStateApiClient planetStateApi;

    [SerializeField] private int gameplaySceneIndex = 7;

    public void OnRestartClicked()
    {
        StartCoroutine(RestartFlow());
    }

    private IEnumerator RestartFlow()
    {
        var planetId = AppSession.Instance?.ActivePlanet?.planetId;
        var stageId  = AppSession.Instance?.SelectedStageId;
        var token    = AppSession.Instance?.AccessToken;

        if (string.IsNullOrEmpty(planetId) || string.IsNullOrEmpty(stageId))
        {
            Debug.LogError("[Restart] Missing planetId/stageId");
            yield break;
        }

        if (planetStateApi == null)
        {
            Debug.LogError("[Restart] planetStateApi is null (assign in Inspector)");
            yield break;
        }

        string err = null;

        yield return StartCoroutine(planetStateApi.ResetStage(
            planetId, stageId, token,
            onOk: () => Debug.Log("[Restart] ✅ reset ok"),
            onErr: e => err = e
        ));

        if (!string.IsNullOrEmpty(err))
        {
            Debug.LogError("[Restart] " + err);
            yield break;
        }

        SceneLoader.Instance.LoadScene(gameplaySceneIndex);
    }
}
