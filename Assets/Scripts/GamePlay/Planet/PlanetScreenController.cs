using UnityEngine;

/// <summary>
/// Controls the planet selection screen.
/// Spawns the planet visual and handles navigation to stage map on click.
/// </summary>
public class PlanetScreenController : MonoBehaviour
{
    [Header("Model")]
    [SerializeField] PlanetData planet;

    [Header("View")]
    [SerializeField] GameObject screenRoot;
    [SerializeField] PlanetSpawner spawner;
    [SerializeField] private GameConfig gameConfig;

    void Start()
    {
        if (gameConfig == null)
            gameConfig = Resources.Load<GameConfig>("GameConfig");

        Open();

        if (GameManager.Instance != null)
            GameManager.Instance.RequestActivePlanet(forceRefresh: true);
    }

    public void Open()
    {
        if (screenRoot != null)
            screenRoot.SetActive(true);

        if (spawner == null)
        {
            Debug.LogError("PlanetSpawner not assigned");
            return;
        }

        if (planet == null)
        {
            Debug.LogError("PlanetData (planet) not assigned");
            return;
        }

        spawner.Spawn(planet.planetPrefab, OnPlanetClicked);
    }


    public void Close()
    {
        if (screenRoot != null)
            screenRoot.SetActive(false);

        spawner.Clear();
    }

   public void OnPlanetClicked()
    {
        Debug.Log("[PlanetScreenController] OnPlanetClicked");

        if (SceneLoader.Instance != null && gameConfig != null)
        {
            SceneLoader.Instance.LoadScene(gameConfig.stagesMapSceneIndex);
        }
        else
        {
            Debug.LogWarning("[PlanetScreenController] SceneLoader or GameConfig is null");
        }
    }

}
