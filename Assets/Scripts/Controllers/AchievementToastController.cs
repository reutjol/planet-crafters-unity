using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AchievementToastController : MonoBehaviour
{
    public static AchievementToastController Instance { get; private set; }

    [Header("Panel")]
    [SerializeField] private GameObject toastPanel;

    [Header("Text")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text rewardText;

    [Header("Settings")]
    [SerializeField] private float displayDuration = 3f;

    public bool IsShowing { get; private set; }

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void OnEnable()
    {
        AchievementNotifier.OnAchievementsUnlocked += HandleUnlocked;
        AchievementNotifier.FlushPending();
    }

    private void OnDisable()
    {
        AchievementNotifier.OnAchievementsUnlocked -= HandleUnlocked;
    }

    private void Start()
    {
        toastPanel?.SetActive(false);
    }

    private void HandleUnlocked(List<UnlockedAchievementDto> unlocked)
    {
        if (unlocked == null || unlocked.Count == 0) return;
        StartCoroutine(Show(unlocked));
    }

    private IEnumerator Show(List<UnlockedAchievementDto> unlocked)
    {
        IsShowing = true;

        if (unlocked.Count == 1)
        {
            var a = unlocked[0];
            if (titleText != null) titleText.text = a.title;
            if (rewardText != null)
                rewardText.text = a.rewardAmount > 0 ? $"+{a.rewardAmount} {a.rewardType}" : "";
        }
        else
        {
            if (titleText != null) titleText.text = "Congratulations!";
            if (rewardText != null) rewardText.text = "You unlocked new achievements!";
        }

        toastPanel?.SetActive(true);
        yield return new WaitForSeconds(displayDuration);
        toastPanel?.SetActive(false);

        IsShowing = false;
    }
}
