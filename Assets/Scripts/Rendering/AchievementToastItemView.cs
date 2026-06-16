using TMPro;
using UnityEngine;

public class AchievementToastItemView : MonoBehaviour
{
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text rewardText;

    public void Bind(UnlockedAchievementDto achievement)
    {
        if (titleText != null)
            titleText.text = achievement.title;

        if (rewardText != null && achievement.rewardAmount > 0)
            rewardText.text = $"+{achievement.rewardAmount} {achievement.rewardType}";
        else if (rewardText != null)
            rewardText.text = "";
    }
}
