using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WheelSliceView : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text titleText;

    public void SetCounterRotation(float wheelZRotation)
    {
        RectTransform rectTransform = transform as RectTransform;
        Quaternion counterRotation = Quaternion.Euler(0f, 0f, -wheelZRotation);

        if (rectTransform != null)
            rectTransform.localRotation = counterRotation;
        else
            transform.localRotation = counterRotation;
    }

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
