using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WheelSliceView : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text titleText;

    public void SetIcon(Sprite sprite)
    {
        if (iconImage != null)
            iconImage.sprite = sprite;
    }

    public void SetTitle(string value)
    {
        if (titleText != null)
            titleText.text = value;
    }
}