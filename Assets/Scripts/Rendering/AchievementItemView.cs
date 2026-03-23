using TMPro;
using UnityEngine;

public class AchievementItemView : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private TMP_Text rewardText;

    public void Bind(AchievementDto achievement, int currentProgress = 0)
    {
        if (achievement == null)
        {
            Debug.LogWarning("AchievementItemView.Bind called with null achievement.");
            return;
        }

        titleText.text = achievement.title;
        descriptionText.text = achievement.description;

        progressText.text = $"{currentProgress} / {achievement.targetValue}";

        rewardText.text = $"{achievement.rewardAmount} {achievement.rewardType}";
    }
}