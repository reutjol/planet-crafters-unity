using System.Collections.Generic;
using UnityEngine;


public class AchievementListView : MonoBehaviour
{
    [Header("List References")]
    [SerializeField] private Transform contentRoot;
    [SerializeField] private AchievementItemView itemPrefab;

    public void Show(List<AchievementDto> achievements)
    {
        Clear();

        if (achievements == null || achievements.Count == 0)
            return;

        for (int i = 0; i < achievements.Count; i++)
        {
            AchievementItemView itemInstance = Instantiate(itemPrefab, contentRoot);
            itemInstance.Bind(achievements[i], achievements[i].currentProgress);
        }
    }

    public void Clear()
    {
        if (contentRoot == null)
        {
            return;
        }

        for (int i = contentRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(contentRoot.GetChild(i).gameObject);
        }
    }
}