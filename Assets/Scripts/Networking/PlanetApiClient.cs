using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// API client for planet-related operations (fetching active planet data).
/// Singleton that persists across scenes.
/// </summary>
public class PlanetApiClient : BaseApiClient
{
    public static PlanetApiClient Instance { get; private set; }

    protected override void Awake()
    {
        base.Awake();

        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public IEnumerator AddCoins(
        string planetId,
        int amount,
        string accessToken,
        Action<int> onSuccess,
        Action<string> onError)
    {
        yield return PostRequest<object, AddCoinsResponseDto>(
            $"/api/planets/{planetId}/add-coins",
            new AddCoinsRequestDto { amount = amount },
            accessToken,
            response => onSuccess?.Invoke(response.totalCoins),
            onError
        );
    }

    public IEnumerator GetActivePlanet(
        string accessToken,
        Action<PlanetDto> onSuccess,
        Action<string> onError)
    {
        if (string.IsNullOrEmpty(accessToken))
        {
            Debug.LogWarning("[PlanetApiClient] WARNING: Authorization header NOT set (empty token).");
        }

        yield return GetRequest<PlanetDto>(
            "/api/planets/active",
            accessToken,
            planet =>
            {
                if (string.IsNullOrEmpty(planet.planetId))
                {
                    onError?.Invoke("GetActivePlanet: invalid response (missing planetId)");
                    return;
                }
                onSuccess?.Invoke(planet);
            },
            onError
        );
    }
}
