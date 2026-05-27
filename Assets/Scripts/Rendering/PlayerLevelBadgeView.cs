using TMPro;
using UnityEngine;

public sealed class PlayerLevelBadgeView : MonoBehaviour
{
    [SerializeField] private TMP_Text levelText;

    public void SetLevel(int level)
    {
        if (levelText != null)
            levelText.text = Mathf.Max(1, level).ToString();
    }
}
