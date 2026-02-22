using System;
using System.Collections;

/// <summary>
/// API client for planet stage state operations
/// Singleton that persists across scenes
/// </summary>
public class PlanetStateApiClient : BaseApiClient
{
    public static PlanetStateApiClient Instance { get; private set; }

    protected override void Awake()
    {
        base.Awake(); // Load GameConfig first

        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    public IEnumerator GetPlanetStageState(
        string planetId,
        string stageId,
        int deckSize,
        string accessToken,
        Action<PlanetStageStateDto> onSuccess,
        Action<string> onError)
    {
        var endpoint = $"/api/planet-state/{planetId}/{stageId}?deckSize={deckSize}";
        yield return GetRequest<PlanetStageStateDto>(endpoint, accessToken, onSuccess, onError);
    }

    public IEnumerator SavePlanetStageState(
        string planetId,
        string stageId,
        string accessToken,
        SaveStageStateRequestDto dto,
        Action onSuccess,
        Action<string> onError)
    {
        var endpoint = $"/api/planet-state/{planetId}/{stageId}";
        yield return PutRequest(endpoint, dto, accessToken, onSuccess, onError);
    }

    public IEnumerator ResetStage(
        string planetId,
        string stageId,
        string accessToken,
        Action onSuccess,
        Action<string> onError)
    {
        var endpoint = $"/api/planet-state/{planetId}/{stageId}/reset";
        yield return PostRequestNoResponse(endpoint, accessToken, onSuccess, onError);
    }
}


