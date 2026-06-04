using System;
using System.Collections;

public class BoosterApiClient : BaseApiClient
{
    public IEnumerator GetBoosters(
        string accessToken,
        Action<BoosterInventoryDto> onSuccess,
        Action<string> onError)
    {
        yield return GetRequest<BoosterInventoryDto>("/api/boosters", accessToken, onSuccess, onError);
    }

    public IEnumerator GrantBooster(
        string boosterType,
        string accessToken,
        Action<BoosterInventoryDto> onSuccess,
        Action<string> onError)
    {
        yield return PostRequest<object, BoosterInventoryDto>(
            $"/api/boosters/grant/{boosterType}", new object(), accessToken, onSuccess, onError);
    }

    public IEnumerator CancelLastTile(
        string planetId,
        string stageId,
        string accessToken,
        Action<PlanetStageStateDto> onSuccess,
        Action<string> onError)
    {
        var endpoint = $"/api/planet-state/{planetId}/{stageId}/cancel-tile";
        yield return PostRequest<object, PlanetStageStateDto>(endpoint, new object(), accessToken, onSuccess, onError);
    }

    public IEnumerator AddHexToHand(
        string planetId,
        string stageId,
        string accessToken,
        Action<PlanetStageStateDto> onSuccess,
        Action<string> onError)
    {
        var endpoint = $"/api/planet-state/{planetId}/{stageId}/add-hex";
        yield return PostRequest<object, PlanetStageStateDto>(endpoint, new object(), accessToken, onSuccess, onError);
    }
}
