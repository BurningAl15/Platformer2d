using System;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

/// <summary>Small runtime access layer for the project's Unity Localization tables.</summary>
public static class GameLocalization
{
    public const string TableName = "Game UI";

    /// <summary>
    /// Raised when a locale becomes active, including the first one after
    /// startup. Labels written from code refresh on it.
    /// </summary>
    public static event Action Changed;

    /// <summary>True once the tables can answer lookups.</summary>
    public static bool IsReady =>
        LocalizationSettings.InitializationOperation.IsValid() &&
        LocalizationSettings.InitializationOperation.IsDone &&
        LocalizationSettings.SelectedLocale != null;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void StartInitialization()
    {
        LocalizationSettings.SelectedLocaleChanged -= OnSelectedLocaleChanged;
        LocalizationSettings.SelectedLocaleChanged += OnSelectedLocaleChanged;
        // Starting the operation is asynchronous; completion selects a locale and raises Changed.
        LocalizationSettings.InitializationOperation.Completed += _ => Changed?.Invoke();
    }

    private static void OnSelectedLocaleChanged(Locale locale) => Changed?.Invoke();

    public static string NormalizeLanguageCode(string languageCode)
    {
        return string.Equals(languageCode, "es", StringComparison.OrdinalIgnoreCase)
            ? "es"
            : "en";
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void MigrateSavedLanguagePreference()
    {
        if (PlayerPrefs.HasKey("selected-locale") || !PlayerPrefs.HasKey("SelectedLanguage"))
            return;

        string localeCode = NormalizeLanguageCode(PlayerPrefs.GetString("SelectedLanguage"));
        PlayerPrefs.SetString("selected-locale", localeCode);
        PlayerPrefs.Save();
    }

    public static void SetLanguage(string languageCode)
    {
        string localeCode = NormalizeLanguageCode(languageCode);
        PlayerPrefs.SetString("selected-locale", localeCode);
        PlayerPrefs.Save();

        Locale locale = LocalizationSettings.AvailableLocales?.GetLocale(localeCode);
        if (locale != null)
            LocalizationSettings.SelectedLocale = locale;
    }

    public static string SelectedLanguageCode
    {
        get
        {
            string selectedCode = LocalizationSettings.SelectedLocale?.Identifier.Code;
            return NormalizeLanguageCode(string.IsNullOrEmpty(selectedCode)
                ? PlayerPrefs.GetString("SelectedLanguage", "EN")
                : selectedCode);
        }
    }

    public static string Get(string key, params object[] arguments)
    {
        if (string.IsNullOrEmpty(key))
            return string.Empty;

        if (LocalizationSettings.InitializationOperation.IsValid() &&
            !LocalizationSettings.InitializationOperation.IsDone)
            LocalizationSettings.InitializationOperation.WaitForCompletion();

        // Without an active locale there is nothing to look up; Changed refreshes the label later.
        if (!IsReady)
            return string.Empty;

        string value = LocalizationSettings.StringDatabase.GetLocalizedString(
            TableName,
            key,
            LocalizationSettings.SelectedLocale,
            FallbackBehavior.UseProjectSettings,
            arguments);
        return string.IsNullOrEmpty(value) || string.Equals(value, key, StringComparison.Ordinal)
            ? key
            : value;
    }

    public static string GetLevelName(int levelNumber, string fallback)
    {
        // Any level with a LEVEL_NAME_n entry gets its translated name; others keep the map point's name.
        if (levelNumber < 1) return fallback;
        string key = $"LEVEL_NAME_{levelNumber}";
        string value = Get(key);
        return string.IsNullOrEmpty(value) || value == key ? fallback : value;
    }

    public static string GetWorldName(string sourceName)
    {
        return string.Equals(sourceName, "World 1 - The Forest", StringComparison.OrdinalIgnoreCase)
            ? Get("WORLD_NAME_1")
            : sourceName;
    }
}
