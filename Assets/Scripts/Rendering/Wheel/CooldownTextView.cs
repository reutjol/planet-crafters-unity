using TMPro;
using UnityEngine;

public class CooldownTextView : MonoBehaviour
{
    [SerializeField] private TMP_Text textLabel;
    private ILocalizationService localizationService;

    private void Awake()
    {
        localizationService = UnityLocalizationService.Instance;
    }

    public void SetText(string value)
    {
        if (textLabel != null)
        {
            textLabel.text = value;

            if (localizationService != null)
                textLabel.isRightToLeftText = localizationService.IsRightToLeft;
        }
    }

    public void SetVisible(bool isVisible)
    {
        gameObject.SetActive(isVisible);
    }
}
