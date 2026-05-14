using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

[DefaultExecutionOrder(-1000)]
public class UnityLocalizationService : MonoBehaviour, ILocalizationService
{
    private const string SavedLanguageKey = "localization.language";

    private static UnityLocalizationService instance;

    [Header("Table")]
    [SerializeField] private string tableName = "GameText";
    [SerializeField] private string sourceLanguageCode = "en";

    [Header("Supported Languages")]
    [SerializeField] private string englishLanguageCode = "en";
    [SerializeField] private string hebrewLanguageCode = "he";

    private readonly Dictionary<string, string> sourceTextToKey = new Dictionary<string, string>();
    private bool sourceLookupBuilt;

    public event Action<string> LanguageChanged;

    public static UnityLocalizationService Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<UnityLocalizationService>();

            if (instance == null)
            {
                var serviceObject = new GameObject(nameof(UnityLocalizationService));
                instance = serviceObject.AddComponent<UnityLocalizationService>();
            }

            return instance;
        }
    }

    public string CurrentLanguageCode
    {
        get
        {
            Locale selectedLocale = LocalizationSettings.SelectedLocale;
            return selectedLocale != null ? selectedLocale.Identifier.Code : englishLanguageCode;
        }
    }

    public bool IsRightToLeft => CurrentLanguageCode == hebrewLanguageCode;

    private TableReference TableReference => tableName;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        if (transform.parent == null)
            DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        LocalizationSettings.SelectedLocaleChanged += HandleSelectedLocaleChanged;
        ApplySavedLanguage();
    }

    private void OnDisable()
    {
        LocalizationSettings.SelectedLocaleChanged -= HandleSelectedLocaleChanged;
    }

    public void SetLanguage(string languageCode)
    {
        if (string.IsNullOrWhiteSpace(languageCode))
            return;

        Locale locale = FindLocale(languageCode);

        if (locale == null)
        {
            Debug.LogWarning($"[UnityLocalizationService] Locale '{languageCode}' was not found.");
            return;
        }

        if (LocalizationSettings.SelectedLocale == locale)
        {
            SaveLanguage(locale.Identifier.Code);
            NotifyLanguageChanged(locale.Identifier.Code);
            return;
        }

        LocalizationSettings.SelectedLocale = locale;
    }

    public void ToggleLanguage()
    {
        string nextLanguageCode = CurrentLanguageCode == hebrewLanguageCode
            ? englishLanguageCode
            : hebrewLanguageCode;

        SetLanguage(nextLanguageCode);
    }

    public string GetText(string key, string fallbackText = "")
    {
        if (string.IsNullOrWhiteSpace(key))
            return fallbackText;

        try
        {
            TableEntryReference entryReference = key;
            string localizedText = LocalizationSettings.StringDatabase.GetLocalizedString(TableReference, entryReference);

            if (!string.IsNullOrEmpty(localizedText) && !localizedText.StartsWith("No translation found", StringComparison.Ordinal))
                return localizedText;
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"[UnityLocalizationService] Could not localize key '{key}': {exception.Message}");
        }

        return string.IsNullOrEmpty(fallbackText) ? key : fallbackText;
    }

    public string GetTextBySource(string sourceText)
    {
        if (string.IsNullOrEmpty(sourceText))
            return sourceText;

        if (TryGetKeyBySource(sourceText, out string key))
            return GetText(key, sourceText);

        return sourceText;
    }

    private bool TryGetKeyBySource(string sourceText, out string key)
    {
        BuildSourceLookupIfNeeded();

        if (sourceTextToKey.TryGetValue(sourceText, out key))
            return true;

        return sourceTextToKey.TryGetValue(Normalize(sourceText), out key);
    }

    private void BuildSourceLookupIfNeeded()
    {
        if (sourceLookupBuilt)
            return;

        sourceLookupBuilt = true;
        sourceTextToKey.Clear();

        Locale sourceLocale = FindLocale(sourceLanguageCode);

        if (sourceLocale == null)
            return;

        StringTable sourceTable = LocalizationSettings.StringDatabase.GetTable(TableReference, sourceLocale);

        if (sourceTable == null || sourceTable.SharedData == null)
            return;

        foreach (KeyValuePair<long, StringTableEntry> pair in sourceTable)
        {
            if (pair.Value == null)
                continue;

            SharedTableData.SharedTableEntry sharedEntry = sourceTable.SharedData.GetEntry(pair.Key);

            if (sharedEntry == null)
                continue;

            RegisterSourceText(pair.Value.Value, sharedEntry.Key);
        }
    }

    private void RegisterSourceText(string sourceText, string key)
    {
        if (string.IsNullOrEmpty(sourceText) || string.IsNullOrEmpty(key))
            return;

        if (!sourceTextToKey.ContainsKey(sourceText))
            sourceTextToKey.Add(sourceText, key);

        string normalizedText = Normalize(sourceText);

        if (!sourceTextToKey.ContainsKey(normalizedText))
            sourceTextToKey.Add(normalizedText, key);
    }

    private Locale FindLocale(string languageCode)
    {
        if (LocalizationSettings.AvailableLocales == null)
            return null;

        return LocalizationSettings.AvailableLocales.GetLocale(new LocaleIdentifier(languageCode));
    }

    private void ApplySavedLanguage()
    {
        string savedLanguageCode = PlayerPrefs.GetString(SavedLanguageKey, string.Empty);

        if (string.IsNullOrEmpty(savedLanguageCode))
            return;

        Locale locale = FindLocale(savedLanguageCode);

        if (locale != null && LocalizationSettings.SelectedLocale != locale)
            LocalizationSettings.SelectedLocale = locale;
    }

    private void HandleSelectedLocaleChanged(Locale locale)
    {
        if (locale == null)
            return;

        SaveLanguage(locale.Identifier.Code);
        NotifyLanguageChanged(locale.Identifier.Code);
    }

    private void SaveLanguage(string languageCode)
    {
        PlayerPrefs.SetString(SavedLanguageKey, languageCode);
        PlayerPrefs.Save();
    }

    private void NotifyLanguageChanged(string languageCode)
    {
        LanguageChanged?.Invoke(languageCode);
    }

    private static string Normalize(string value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        string normalized = value.Replace("\r\n", "\n").Replace("\r", "\n").Trim();

        while (normalized.Contains("\n\n"))
            normalized = normalized.Replace("\n\n", "\n");

        while (normalized.Contains("  "))
            normalized = normalized.Replace("  ", " ");

        return normalized;
    }
}
