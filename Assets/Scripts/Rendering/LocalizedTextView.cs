using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class LocalizedTextView : MonoBehaviour
{
    [SerializeField] private string localizationKey;
    [SerializeField] private string fallbackText;
    [SerializeField] private bool updateRightToLeft = true;
    [SerializeField] private TMP_FontAsset hebrewFont;
    [SerializeField] private TMP_Text tmpText;
    [SerializeField] private Text uiText;

    private ILocalizationService localizationService;
    private TMP_FontAsset defaultFont;
    private HorizontalAlignmentOptions defaultHAlignment;

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
        if (tmpText != null)
        {
            if (defaultFont == null) defaultFont = tmpText.font;
            defaultHAlignment = tmpText.horizontalAlignment;
        }
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

            if (localizationService != null)
            {
                bool rtl = localizationService.IsRightToLeft;

                if (updateRightToLeft)
                    tmpText.isRightToLeftText = rtl;

                if (hebrewFont != null && defaultFont != null)
                    tmpText.font = rtl ? hebrewFont : defaultFont;

                tmpText.horizontalAlignment = FlipAlignment(defaultHAlignment, rtl);
            }
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

    public static HorizontalAlignmentOptions FlipAlignment(HorizontalAlignmentOptions original, bool rtl)
    {
        if (!rtl) return original;
        return original switch
        {
            HorizontalAlignmentOptions.Left  => HorizontalAlignmentOptions.Right,
            HorizontalAlignmentOptions.Right => HorizontalAlignmentOptions.Left,
            _                                => original
        };
    }

    private void CacheTextTargets()
    {
        if (tmpText == null)
            tmpText = GetComponent<TMP_Text>();

        if (uiText == null)
            uiText = GetComponent<Text>();
    }
}
