using UnityEngine;

public class PlanetScreenController : MonoBehaviour
{
    [Header("Model")]
    [SerializeField] PlanetData planet;

    [Header("View")]
    [SerializeField] GameObject screenRoot;
    [SerializeField] PlanetSpawner spawner;

    void Start()
    {
        Open();
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

        if (SceneLoader.Instance != null)
        {
            Debug.Log("[PlanetScreenController] Using SceneLoader -> Load 6");
            SceneLoader.Instance.LoadScene(6);
        }
        else
        {
            Debug.LogWarning("[PlanetScreenController] SceneLoader.Instance is NULL -> using SceneManager.LoadScene(6)");
            UnityEngine.SceneManagement.SceneManager.LoadScene(6);
        }
    }

}
