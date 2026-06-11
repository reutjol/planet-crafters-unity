using TMPro;
using UnityEngine;

public class CooldownTextView : MonoBehaviour
{
    [SerializeField] private TMP_Text textLabel;

    public void SetText(string value)
    {
        if (textLabel != null)
        {
            textLabel.text = value;

            textLabel.isRightToLeftText = false;
        }
    }

    public void SetVisible(bool isVisible)
    {
        gameObject.SetActive(isVisible);
    }
}
