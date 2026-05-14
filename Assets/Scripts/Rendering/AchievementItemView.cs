using TMPro;
using UnityEngine;

public class AchievementItemView : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private TMP_Text rewardText;

    private ILocalizationService localizationService;
    private AchievementDto currentAchievement;
    private int currentProgress;

    private void Awake()
    {
        localizationService = UnityLocalizationService.Instance;
    }

    private void OnEnable()
    {
        if (localizationService == null)
            localizationService = UnityLocalizationService.Instance;

        localizationService.LanguageChanged += HandleLanguageChanged;
    }

    private void OnDisable()
    {
        if (localizationService != null)
            localizationService.LanguageChanged -= HandleLanguageChanged;
    }

    public void Bind(AchievementDto achievement, int currentProgress = 0)
    {
        if (achievement == null)
        {
            Debug.LogWarning("AchievementItemView.Bind called with null achievement.");
            return;
        }

        currentAchievement = achievement;
        this.currentProgress = currentProgress;
        ApplyAchievementText();
    }

    private void ApplyAchievementText()
    {
        if (currentAchievement == null)
            return;

        SetLocalizedText(titleText, LocalizeSource(currentAchievement.title));
        SetLocalizedText(descriptionText, LocalizeSource(currentAchievement.description));

        progressText.text = $"{currentProgress} / {currentAchievement.targetValue}";

        SetLocalizedText(rewardText, $"{currentAchievement.rewardAmount} {LocalizeSource(currentAchievement.rewardType)}");
    }

    private void HandleLanguageChanged(string languageCode)
    {
        ApplyAchievementText();
    }

    private string LocalizeSource(string sourceText)
    {
        return localizationService != null
            ? localizationService.GetTextBySource(sourceText)
            : sourceText;
    }

    private void SetLocalizedText(TMP_Text target, string value)
    {
        if (target == null)
            return;

        target.text = value;

        if (localizationService != null)
            target.isRightToLeftText = localizationService.IsRightToLeft;
    }
}
