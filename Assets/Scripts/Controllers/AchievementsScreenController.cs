using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AchievementsScreenController : MonoBehaviour
{
    [Header("Scroll")]
    [SerializeField] private ScrollRect scrollRect;

    [Header("Tab Buttons")]
    [SerializeField] private Button tabSingleButton;
    [SerializeField] private Button tabMultiButton;

    [Header("List View References")]
    [SerializeField] private AchievementListView singleListView;
    [SerializeField] private AchievementListView multiListView;

    private AchievementService fallbackService;

    private void Awake()
    {
        fallbackService = new AchievementService(new JsonAchievementRepository());
    }

    private void Start()
    {
        OpenSingleTab();

        if (AchievementApiClient.Instance != null && AppSession.Instance != null && AppSession.Instance.HasAccess())
            StartCoroutine(LoadFromServer());
        else
            LoadFromLocalJson();
    }

    private IEnumerator LoadFromServer()
    {
        yield return AchievementApiClient.Instance.GetUserAchievements(
            AppSession.Instance.AccessToken,
            onSuccess: response =>
            {
                if (response?.achievements == null)
                {
                    LoadFromLocalJson();
                    return;
                }
                ShowAchievements(response.achievements);
            },
            onError: _ => LoadFromLocalJson()
        );
    }

    private void LoadFromLocalJson()
    {
        ShowAchievements(fallbackService.GetAllAchievements());
    }

    private void ShowAchievements(List<AchievementDto> achievements)
    {
        var single = achievements.FindAll(a => a.category == "Single");
        var multi = achievements.FindAll(a => a.category == "Multi");

        singleListView?.Show(single);
        multiListView?.Show(multi);
    }

    public void OpenSingleTab()
    {
        singleListView?.gameObject.SetActive(true);
        multiListView?.gameObject.SetActive(false);
        if (scrollRect != null && singleListView != null)
            scrollRect.content = singleListView.GetComponent<RectTransform>();
        if (tabSingleButton != null) tabSingleButton.interactable = false;
        if (tabMultiButton != null)  tabMultiButton.interactable  = true;
    }

    public void OpenMultiTab()
    {
        singleListView?.gameObject.SetActive(false);
        multiListView?.gameObject.SetActive(true);
        if (scrollRect != null && multiListView != null)
            scrollRect.content = multiListView.GetComponent<RectTransform>();
        if (tabSingleButton != null) tabSingleButton.interactable = true;
        if (tabMultiButton != null)  tabMultiButton.interactable  = false;
    }
}
