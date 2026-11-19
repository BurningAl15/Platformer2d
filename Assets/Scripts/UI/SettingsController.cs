using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Controlador del panel de ajustes (Settings) para Fox's Tale.
/// - Permite alternar Música y SFX de forma persistente a través de AudioMixerManager y SaveManager.
/// - Compatible con controles táctiles móviles, teclado y gamepad.
/// - Cero emojis unicode en strings; texto alfanumérico limpio en inglés.
/// </summary>
public class SettingsController : MonoBehaviour
{
    [Header("Panel & CanvasGroup")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private CanvasGroup settingsCanvasGroup;

    [Header("Botones de Apertura y Cierre")]
    [SerializeField] private Button openSettingsButton;
    [SerializeField] private Button closeButton;

    [Header("Controles de Música")]
    [SerializeField] private Button musicToggleButton;
    [SerializeField] private TextMeshProUGUI musicStatusText;

    [Header("Controles de SFX")]
    [SerializeField] private Button sfxToggleButton;
    [SerializeField] private TextMeshProUGUI sfxStatusText;

    [Header("Controles de Jugabilidad & Idioma")]
    [SerializeField] private Button hapticsToggleButton;
    [SerializeField] private TextMeshProUGUI hapticsStatusText;
    [SerializeField] private Button keepScreenAwakeToggleButton;
    [SerializeField] private TextMeshProUGUI keepScreenAwakeStatusText;
    [SerializeField] private Button languageToggleButton;
    [SerializeField] private TextMeshProUGUI languageStatusText;

    [Header("Character")]
    [SerializeField] private Button characterButton;
    [SerializeField] private TextMeshProUGUI characterButtonText;
    [SerializeField] private CharacterSelectController characterPanel;
    [SerializeField] private Button achievementsButton;
    [SerializeField] private TextMeshProUGUI achievementsButtonText;
    [SerializeField] private AchievementsPanel achievementsPanel;

    [Header("Section captions")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI soundCaptionText;
    [SerializeField] private TextMeshProUGUI gameCaptionText;
    [SerializeField] private TextMeshProUGUI dataCaptionText;

    [Header("Progress (icon + amount)")]
    [SerializeField] private WorldData world;
    [SerializeField] private TextMeshProUGUI levelsText;
    [SerializeField] private TextMeshProUGUI gemsCountText;
    [SerializeField] private TextMeshProUGUI cloversCountText;

    [Header("Versión")]
    [SerializeField] private TextMeshProUGUI versionText;

    [Header("Gestión de Guardado")]
    [SerializeField] private Button resetProgressButton;
    [SerializeField] private TextMeshProUGUI resetProgressText;
    [SerializeField] private GameObject confirmResetDialog;
    [SerializeField] private Button confirmResetButton;
    [SerializeField] private Button cancelResetButton;

    private bool isPanelExplicitlyOpen = false;
    private bool awaitingResetConfirmation = false;
    private float resetConfirmTimer = 0f;

    private void Awake()
    {
        BindSettingsListeners();
        ApplyPlatformLayout();
    }

    /// <summary>
    /// Hides options the platform does not have and re-centres the remaining
    /// buttons. Off mobile the screen button switches fullscreen / window.
    /// </summary>
    private void ApplyPlatformLayout()
    {
        if (PlatformFeatures.IsMobile) return;   // the serialized layout is the mobile one

        bool screenOption = PlatformFeatures.HasDisplayModeOption;
        if (hapticsToggleButton != null) hapticsToggleButton.gameObject.SetActive(false);
        if (keepScreenAwakeToggleButton != null) keepScreenAwakeToggleButton.gameObject.SetActive(screenOption);

        LayoutRow(-26f, 520f, musicToggleButton, sfxToggleButton);
        if (screenOption) LayoutRow(24f, 520f, keepScreenAwakeToggleButton, languageToggleButton);
        else LayoutRow(24f, 520f, languageToggleButton);

        // Explicit links pointed at hidden buttons; let Unity derive them from the new layout.
        foreach (Button button in new[] { musicToggleButton, sfxToggleButton, keepScreenAwakeToggleButton,
                     languageToggleButton, characterButton, achievementsButton, resetProgressButton, closeButton })
        {
            if (button == null) continue;
            Navigation navigation = button.navigation;
            navigation.mode = Navigation.Mode.Automatic;
            button.navigation = navigation;
        }
    }

    private static void LayoutRow(float y, float width, params Button[] buttons)
    {
        const float gap = 24f;
        float total = buttons.Length * width + (buttons.Length - 1) * gap;
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] == null) continue;
            var rect = (RectTransform)buttons[i].transform;
            rect.sizeDelta = new Vector2(width, rect.sizeDelta.y);
            rect.anchoredPosition = new Vector2(-total / 2f + width / 2f + i * (width + gap), y);
        }
    }

    private void OnEnable() => GameLocalization.Changed += RefreshUI;
    private void OnDisable() => GameLocalization.Changed -= RefreshUI;

    private void BindSettingsListeners()
    {
        BindButtonListener(openSettingsButton, OpenSettings);
        BindButtonListener(closeButton, CloseSettings);
        BindButtonListener(musicToggleButton, ToggleMusic);
        BindButtonListener(sfxToggleButton, ToggleSFX);
        BindButtonListener(hapticsToggleButton, ToggleHaptics);
        BindButtonListener(keepScreenAwakeToggleButton, ToggleKeepScreenAwake);
        BindButtonListener(languageToggleButton, ToggleLanguage);
        BindButtonListener(characterButton, OpenCharacter);
        BindButtonListener(achievementsButton, OpenAchievements);
        BindButtonListener(resetProgressButton, OnResetProgressPressed);
        BindButtonListener(confirmResetButton, ConfirmResetProgress);
        BindButtonListener(cancelResetButton, CancelResetProgress);
    }

    private void BindButtonListener(Button button, UnityAction listener)
    {
        if (button == null) return;

        button.onClick.RemoveListener(listener);

        // RemoveListener does not remove Inspector-persistent callbacks. Avoid
        // registering the same action twice when the scene already serializes it.
        for (int i = 0; i < button.onClick.GetPersistentEventCount(); i++)
        {
            if (button.onClick.GetPersistentTarget(i) == this &&
                button.onClick.GetPersistentMethodName(i) == listener.Method.Name)
                return;
        }

        button.onClick.AddListener(listener);
    }

    private void Update()
    {
        if (awaitingResetConfirmation)
        {
            resetConfirmTimer -= Time.unscaledDeltaTime;
            if (resetConfirmTimer <= 0f)
            {
                awaitingResetConfirmation = false;
                RefreshUI();
            }
        }
    }

    private void Start()
    {
        // Solo cerrar si el usuario NO acaba de abrir el panel en este mismo frame antes de Start()
        if (!isPanelExplicitlyOpen)
        {
            CloseSettings();
        }
        RefreshUI();
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Apertura y Cierre
    // ─────────────────────────────────────────────────────────────────────────

    public void OpenSettings()
    {
        isPanelExplicitlyOpen = true;

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
            settingsPanel.transform.SetAsLastSibling();
        }

        BindSettingsListeners();

        if (settingsCanvasGroup != null)
        {
            settingsCanvasGroup.alpha = 1f;
            settingsCanvasGroup.interactable = true;
            settingsCanvasGroup.blocksRaycasts = true;
        }

        RefreshUI();

        // Enfoque para gamepad / navegación
        if (musicToggleButton != null && EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(musicToggleButton.gameObject);
        }
    }

    public void CloseSettings()
    {
        isPanelExplicitlyOpen = false;

        if (settingsCanvasGroup != null)
        {
            settingsCanvasGroup.alpha = 0f;
            settingsCanvasGroup.interactable = false;
            settingsCanvasGroup.blocksRaycasts = false;
        }

        // Keep this GameObject active: it owns the controller and its Awake
        // initialization. CanvasGroup hides the panel without disabling it.
        if (settingsPanel != null && !settingsPanel.activeSelf)
            settingsPanel.SetActive(true);

        if (EventSystem.current != null)
        {
            GameObject selected = EventSystem.current.currentSelectedGameObject;
            if (selected != null && settingsPanel != null &&
                selected.transform.IsChildOf(settingsPanel.transform))
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Toggles de Audio
    // ─────────────────────────────────────────────────────────────────────────

    public void ToggleMusic()
    {
        if (AudioMixerManager._instance != null)
        {
            AudioMixerManager._instance.SetBackgroundMusicVolume();
        }
        else if (SaveManager.Instance != null)
        {
            bool current = SaveManager.Instance.LoadMusicEnabled();
            SaveManager.Instance.SaveMusicEnabled(!current);
        }

        RefreshUI();
    }

    public void ToggleSFX()
    {
        if (AudioMixerManager._instance != null)
        {
            AudioMixerManager._instance.SetSFXVolume();
        }
        else if (SaveManager.Instance != null)
        {
            bool current = SaveManager.Instance.LoadSFXEnabled();
            SaveManager.Instance.SaveSFXEnabled(!current);
        }

        RefreshUI();
    }

    public void ToggleHaptics()
    {
        bool current = PlayerPrefs.GetInt("HapticsEnabled", 1) == 1;
        PlayerPrefs.SetInt("HapticsEnabled", current ? 0 : 1);
        PlayerPrefs.Save();
        RefreshUI();
        if (!current)
            AppBootstrap.TriggerHapticFeedback();
    }

    public void ToggleKeepScreenAwake()
    {
        if (PlatformFeatures.HasDisplayModeOption)
        {
            PlatformFeatures.SetFullscreen(!PlatformFeatures.IsFullscreenPreferred);
            RefreshUI();
            return;
        }

        bool current = PlayerPrefs.GetInt("KeepScreenAwake", 0) == 1;
        PlayerPrefs.SetInt("KeepScreenAwake", current ? 0 : 1);
        PlayerPrefs.Save();
        AppBootstrap.ApplyScreenAwakePreference();
        RefreshUI();
    }

    /// <summary>Opens the character selector on top of the current scene.</summary>
    public void OpenCharacter()
    {
        if (characterPanel == null) return;
        CloseSettings();
        characterPanel.Open();
    }

    /// <summary>Opens the achievements list on top of the current scene.</summary>
    public void OpenAchievements()
    {
        if (achievementsPanel == null) return;
        CloseSettings();
        achievementsPanel.Open();
    }

    public void ToggleLanguage()
    {
        string current = GameLocalization.SelectedLanguageCode;
        string next = current == "en" ? "es" : "en";
        GameLocalization.SetLanguage(next);
        RefreshUI();
    }

    public void OnResetProgressPressed()
    {
        if (confirmResetDialog != null)
        {
            confirmResetDialog.SetActive(true);
            return;
        }

        if (!awaitingResetConfirmation)
        {
            awaitingResetConfirmation = true;
            resetConfirmTimer = 4.5f;
            if (resetProgressText != null)
            {
                resetProgressText.text = GameLocalization.Get("SETTINGS_RESET_CONFIRM");
            }
        }
        else
        {
            awaitingResetConfirmation = false;
            ConfirmResetProgress();
        }
    }

    public void ConfirmResetProgress()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.ResetProgress(keepTutorial: true);
        }

        if (confirmResetDialog != null)
        {
            confirmResetDialog.SetActive(false);
        }

        RefreshUI();

        if (resetProgressText != null)
        {
            resetProgressText.text = GameLocalization.Get("SETTINGS_RESET_DONE");
        }

        // Every screen still shows the old progress, so restart cleanly: with the
        // tutorial kept, go straight to the map at level 1; otherwise to the title.
        CloseSettings();
        bool toMap = SaveManager.Instance != null && SaveManager.Instance.IsTutorialCompleted();
        PauseMenu pause = PauseMenu._instance;
        if (pause != null && pause.isPaused)
        {
            if (toMap) pause.LevelSelection(); else pause.MainMenu();
        }
        else
        {
            Time.timeScale = 1f;
            if (toMap) SceneUtils.ToSelectionScene(); else SceneUtils.ToMainScene();
        }
    }

    public void CancelResetProgress()
    {
        if (confirmResetDialog != null)
        {
            confirmResetDialog.SetActive(false);
        }
        awaitingResetConfirmation = false;
        RefreshUI();
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Actualización de UI
    // ─────────────────────────────────────────────────────────────────────────

    public void RefreshUI()
    {
        bool musicOn = true;
        bool sfxOn = true;

        if (AudioMixerManager._instance != null)
        {
            musicOn = AudioMixerManager._instance.IsMusicOn;
            sfxOn = AudioMixerManager._instance.IsSFXOn;
        }
        else if (SaveManager.Instance != null)
        {
            musicOn = SaveManager.Instance.LoadMusicEnabled();
            sfxOn = SaveManager.Instance.LoadSFXEnabled();
        }

        if (musicStatusText != null)
        {
            musicStatusText.text = GameLocalization.Get(musicOn ? "SETTINGS_MUSIC_ON" : "SETTINGS_MUSIC_OFF");
        }

        if (sfxStatusText != null)
        {
            sfxStatusText.text = GameLocalization.Get(sfxOn ? "SETTINGS_SFX_ON" : "SETTINGS_SFX_OFF");
        }

        if (titleText != null)
            titleText.text = GameLocalization.Get("UI_SETTINGS");
        if (characterButtonText != null)
            characterButtonText.text = GameLocalization.Get("UI_CHARACTER");
        if (achievementsButtonText != null)
            achievementsButtonText.text = $"{GameLocalization.Get("UI_ACHIEVEMENTS")} {Achievements.UnlockedCount()}/{Achievements.All.Length}";
        if (soundCaptionText != null)
            soundCaptionText.text = GameLocalization.Get(PlatformFeatures.HasVibration ? "SETTINGS_GROUP_SOUND" : "SETTINGS_GROUP_SOUND_ONLY");
        if (gameCaptionText != null)
            gameCaptionText.text = GameLocalization.Get("SETTINGS_GROUP_GAME");
        if (dataCaptionText != null)
            dataCaptionText.text = GameLocalization.Get("SETTINGS_GROUP_DATA");

        if (openSettingsButton != null)
        {
            TextMeshProUGUI openText = openSettingsButton.GetComponentInChildren<TextMeshProUGUI>();
            if (openText != null)
                openText.text = GameLocalization.Get("UI_SETTINGS");
        }

        // Haptics / Vibración
        bool hapticsOn = PlayerPrefs.GetInt("HapticsEnabled", 1) == 1;
        if (hapticsStatusText != null)
        {
            hapticsStatusText.text = GameLocalization.Get(hapticsOn ? "SETTINGS_HAPTICS_ON" : "SETTINGS_HAPTICS_OFF");
        }

        bool keepScreenAwake = PlayerPrefs.GetInt("KeepScreenAwake", 0) == 1;
        if (keepScreenAwakeStatusText != null && PlatformFeatures.HasDisplayModeOption)
        {
            keepScreenAwakeStatusText.text = GameLocalization.Get(PlatformFeatures.IsFullscreenPreferred
                ? "SETTINGS_DISPLAY_FULLSCREEN" : "SETTINGS_DISPLAY_WINDOWED");
        }
        else if (keepScreenAwakeStatusText != null)
        {
            keepScreenAwakeStatusText.text = keepScreenAwake
                ? GameLocalization.Get("SETTINGS_AWAKE_ON")
                : GameLocalization.Get("SETTINGS_AWAKE_OFF");
        }

        // Idioma / Language
        if (languageStatusText != null)
        {
            languageStatusText.text = GameLocalization.Get(GameLocalization.SelectedLanguageCode == "es"
                ? "SETTINGS_LANGUAGE_ES"
                : "SETTINGS_LANGUAGE_EN");
        }

        // Reset progress label (si no está esperando confirmación)
        if (!awaitingResetConfirmation && resetProgressText != null)
        {
            resetProgressText.text = GameLocalization.Get("SETTINGS_RESET");
        }

        // Botón Cerrar
        if (closeButton != null)
        {
            TextMeshProUGUI closeTxt = closeButton.GetComponentInChildren<TextMeshProUGUI>();
            if (closeTxt != null)
            {
                closeTxt.text = GameLocalization.Get("UI_CLOSE_TITLE");
            }
        }

        // Progress: levels as a fraction, gems and clovers as icon + amount
        int levelCount = world != null ? world.LevelCount : 8;
        int unlockedCount = 0;
        if (SaveManager.Instance != null)
        {
            for (int i = 1; i <= levelCount; i++)
                if (SaveManager.Instance.IsLevelUnlocked(i))
                    unlockedCount++;
        }
        if (levelsText != null)
            levelsText.text = GameLocalization.Get("SETTINGS_LEVELS", unlockedCount, levelCount);
        if (gemsCountText != null)
            gemsCountText.text = $"x{(SaveManager.Instance != null ? SaveManager.Instance.LoadGlobalGems() : 0)}";
        if (cloversCountText != null)
            cloversCountText.text = $"x{(SaveManager.Instance != null ? SaveManager.Instance.LoadTotalTrebols() : 0)}";

        // Versión del juego
        if (versionText != null)
        {
            versionText.text = AppVersionDisplay.GetFormattedVersion();
        }
    }
}
