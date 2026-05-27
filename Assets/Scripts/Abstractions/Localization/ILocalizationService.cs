using System;

public interface ILocalizationService
{
    event Action<string> LanguageChanged;

    string CurrentLanguageCode { get; }
    bool IsRightToLeft { get; }

    void SetLanguage(string languageCode);
    void ToggleLanguage();
    string GetText(string key, string fallbackText = "");
    string GetTextBySource(string sourceText);
}
