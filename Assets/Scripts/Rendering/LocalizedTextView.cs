using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class LocalizedTextView : MonoBehaviour
{
    [SerializeField] private string localizationKey;
    [SerializeField] private string fallbackText;
    [SerializeField] private bool updateRightToLeft = true;
    [SerializeField] private TMP_Text tmpText;
    [SerializeField] private Text uiText;

    private ILocalizationService localizationService;

    private void Reset()
    {
        CacheTextTargets();
        fallbackText = GetCurrentText();
    }

    private void OnValidate()
    {
        CacheTextTargets();
    }

    private void OnEnable()
    {
        CacheTextTargets();
        localizationService = UnityLocalizationService.Instance;
        localizationService.LanguageChanged += HandleLanguageChanged;
        ApplyLocalizedText();
    }

    private void OnDisable()
    {
        if (localizationService != null)
            localizationService.LanguageChanged -= HandleLanguageChanged;
    }

    public void Configure(string key, string fallback)
    {
        localizationKey = key;
        fallbackText = fallback;
        CacheTextTargets();
    }

    public void ApplyLocalizedText()
    {
        if (string.IsNullOrWhiteSpace(localizationKey))
            return;

        string value = localizationService != null
            ? localizationService.GetText(localizationKey, fallbackText)
            : fallbackText;

        SetText(value);
    }

    private void HandleLanguageChanged(string languageCode)
    {
        ApplyLocalizedText();
    }

    private void SetText(string value)
    {
        if (tmpText != null)
        {
            tmpText.text = value;

            if (updateRightToLeft && localizationService != null)
                tmpText.isRightToLeftText = localizationService.IsRightToLeft;
        }

        if (uiText != null)
            uiText.text = value;
    }

    private string GetCurrentText()
    {
        if (tmpText != null)
            return tmpText.text;

        if (uiText != null)
            return uiText.text;

        return string.Empty;
    }

    private void CacheTextTargets()
    {
        if (tmpText == null)
            tmpText = GetComponent<TMP_Text>();

        if (uiText == null)
            uiText = GetComponent<Text>();
    }
}
