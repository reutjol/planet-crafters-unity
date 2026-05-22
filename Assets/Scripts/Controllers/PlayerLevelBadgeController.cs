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
        Refresh();
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
}
