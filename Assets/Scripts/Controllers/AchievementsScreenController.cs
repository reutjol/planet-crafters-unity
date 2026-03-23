using UnityEngine;

public class AchievementsScreenController : MonoBehaviour
{
    [Header("Section References")]
    [SerializeField] private GameObject singleListSection;
    [SerializeField] private GameObject multiListSection;

    [Header("List View References")]
    [SerializeField] private AchievementListView singleListView;
    [SerializeField] private AchievementListView multiListView;

    private AchievementService achievementService;

    private void Awake()
    {
        IAchievementRepository achievementRepository = new JsonAchievementRepository();
        achievementService = new AchievementService(achievementRepository);
    }

    private void Start()
    {
        LoadAchievementLists();
        OpenSingleTab();
    }

    private void LoadAchievementLists()
    {
        if (singleListView != null)
        {
            singleListView.Show(achievementService.GetSingleAchievements());
        }

        if (multiListView != null)
        {
            multiListView.Show(achievementService.GetMultiAchievements());
        }
    }

    public void OpenSingleTab()
    {
        if (singleListSection != null)
        {
            singleListSection.SetActive(true);
        }

        if (multiListSection != null)
        {
            multiListSection.SetActive(false);
        }
    }

    public void OpenMultiTab()
    {
        if (singleListSection != null)
        {
            singleListSection.SetActive(false);
        }

        if (multiListSection != null)
        {
            multiListSection.SetActive(true);
        }
    }
}