using UnityEngine;

public sealed class PlayerLevelBadgeController : MonoBehaviour
{
    [SerializeField] private MonoBehaviour levelProviderBehaviour;
    [SerializeField] private PlayerLevelBadgeView view;

    private IPlayerLevelProvider levelProvider;

    private void Awake()
    {
        ResolveDependencies();
    }

    private void OnEnable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnPlanetLoaded += HandlePlanetLoaded;

        Refresh();
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnPlanetLoaded -= HandlePlanetLoaded;
    }

    private void OnValidate()
    {
        if (view == null)
            view = GetComponent<PlayerLevelBadgeView>();
    }

    public void Refresh()
    {
        ResolveDependencies();

        if (levelProvider == null || view == null)
            return;

        view.SetLevel(levelProvider.CurrentLevel);
    }

    private void ResolveDependencies()
    {
        if (view == null)
            view = GetComponent<PlayerLevelBadgeView>();

        levelProvider = levelProviderBehaviour as IPlayerLevelProvider;
    }

    private void HandlePlanetLoaded(PlanetDto planet)
    {
        Refresh();
    }
}
