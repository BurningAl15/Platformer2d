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

    [Header("Tabs (Audio, Video, Game, Data)")]
    [SerializeField] private Button[] tabButtons;
    [SerializeField] private TextMeshProUGUI[] tabLabels;
    [SerializeField] private GameObject[] tabPages;
    [SerializeField] private Sprite tabNormalSprite;
    [SerializeField] private Sprite tabSelectedSprite;

    [Header("Options")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private TextMeshProUGUI musicLabel;
    [SerializeField] private TextMeshProUGUI musicValueText;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private TextMeshProUGUI sfxLabel;
    [SerializeField] private TextMeshProUGUI sfxValueText;
    [SerializeField] private Button shakeToggleButton;
    [SerializeField] private TextMeshProUGUI shakeStatusText;
    [SerializeField] private Button rumbleToggleButton;
    [SerializeField] private TextMeshProUGUI rumbleStatusText;
    [SerializeField] private Button resolutionButton;
    [SerializeField] private TextMeshProUGUI resolutionText;
    [SerializeField] private Button vsyncToggleButton;
    [SerializeField] private TextMeshProUGUI vsyncStatusText;

    private static readonly string[] TabKeys = { "SETTINGS_TAB_AUDIO", "SETTINGS_TAB_VIDEO", "SETTINGS_TAB_GAME", "SETTINGS_TAB_DATA" };
    private int currentTab;

    private bool isPanelExplicitlyOpen = false;
    private bool awaitingResetConfirmation = false;
    private float resetConfirmTimer = 0f;

    private void Awake()
    {
        BindSettingsListeners();
        ApplyPlatformLayout();
    }

    /// <summary>
    /// Shows only the options this platform has. Pages stack their rows with a
    /// layout group, so hidden rows leave no gap, and every selectable uses
    /// automatic navigation so the gamepad follows what is visible.
    /// </summary>
    private void ApplyPlatformLayout()
    {
        bool mobile = PlatformFeatures.IsMobile;
        bool desktop = PlatformFeatures.Current == PlatformFeatures.Profile.Desktop;
        SetRowActive(musicToggleButton, false);              // replaced by the volume sliders
        SetRowActive(sfxToggleButton, false);
        SetRowActive(hapticsToggleButton, PlatformFeatures.HasVibration);
        SetRowActive(rumbleToggleButton, !mobile || MobileGameplayControls.HasPhysicalGameController());
        SetRowActive(keepScreenAwakeToggleButton, PlatformFeatures.HasKeepScreenAwake || PlatformFeatures.HasDisplayModeOption);
        SetRowActive(resolutionButton, desktop);
        SetRowActive(vsyncToggleButton, desktop);

        foreach (Selectable selectable in GetComponentsInChildren<Selectable>(true))
        {
            Navigation navigation = selectable.navigation;
            navigation.mode = Navigation.Mode.Automatic;
            selectable.navigation = navigation;
        }
    }

    private static void SetRowActive(Selectable control, bool active)
    {
        if (control != null) control.gameObject.SetActive(active);
    }

    /// <summary>Shows one tab's page and selects its first option for gamepad/keyboard.</summary>
    public void ShowTab(int index)
    {
        if (tabPages == null || tabPages.Length == 0) return;
        currentTab = (index + tabPages.Length) % tabPages.Length;
        for (int i = 0; i < tabPages.Length; i++)
        {
            if (tabPages[i] != null) tabPages[i].SetActive(i == currentTab);
            if (tabButtons != null && i < tabButtons.Length && tabButtons[i] != null)
            {
                var image = tabButtons[i].GetComponent<Image>();
                if (image != null) image.sprite = i == currentTab ? tabSelectedSprite : tabNormalSprite;
            }
        }
        Canvas.ForceUpdateCanvases();
        WireNavigation();
        if (tabPages[currentTab] != null)
            UISelectionUtility.SelectDefault(null, tabPages[currentTab].transform);
    }

    /// <summary>
    /// Explicit gamepad/keyboard links for the visible tab: tabs left/right,
    /// rows up/down in order, first row up to its tab, last row down to Close.
    /// Sliders keep left/right for their value.
    /// </summary>
    private void WireNavigation()
    {
        var rows = new System.Collections.Generic.List<Selectable>();
        if (tabPages[currentTab] != null)
            foreach (Selectable selectable in tabPages[currentTab].GetComponentsInChildren<Selectable>(false))
                if (selectable.IsInteractable() && selectable.gameObject.activeInHierarchy) rows.Add(selectable);

        Selectable currentTabButton = tabButtons != null && currentTab < tabButtons.Length ? tabButtons[currentTab] : null;
        Selectable first = rows.Count > 0 ? rows[0] : closeButton;
        for (int i = 0; tabButtons != null && i < tabButtons.Length; i++)
        {
            if (tabButtons[i] == null) continue;
            Link(tabButtons[i], up: closeButton, down: first,
                left: tabButtons[(i - 1 + tabButtons.Length) % tabButtons.Length],
                right: tabButtons[(i + 1) % tabButtons.Length]);
        }
        for (int i = 0; i < rows.Count; i++)
        {
            Selectable up = i == 0 ? currentTabButton : rows[i - 1];
            Selectable down = i == rows.Count - 1 ? closeButton : rows[i + 1];
            if (rows[i] is Slider) Link(rows[i], up, down, null, null);
            else Link(rows[i], up, down, rows[i], rows[i]);
        }
        if (closeButton != null)
            Link(closeButton, rows.Count > 0 ? rows[rows.Count - 1] : currentTabButton, currentTabButton, closeButton, closeButton);
    }

    private static void Link(Selectable target, Selectable up, Selectable down, Selectable left, Selectable right)
    {
        if (target == null) return;
        Navigation navigation = new Navigation
        {
            mode = Navigation.Mode.Explicit,
            selectOnUp = up, selectOnDown = down, selectOnLeft = left, selectOnRight = right
        };
        target.navigation = navigation;
    }

    private void HandleTabShortcuts()
    {
        if (!isPanelExplicitlyOpen || !MenuBackStack.IsTop(this)) return;
        var pad = UnityEngine.InputSystem.Gamepad.current;
        var keys = UnityEngine.InputSystem.Keyboard.current;
        bool next = (pad != null && pad.rightShoulder.wasPressedThisFrame) || (keys != null && keys.eKey.wasPressedThisFrame);
        bool previous = (pad != null && pad.leftShoulder.wasPressedThisFrame) || (keys != null && keys.qKey.wasPressedThisFrame);
        if (next) ShowTab(currentTab + 1);
        else if (previous) ShowTab(currentTab - 1);
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
        BindButtonListener(shakeToggleButton, () => { GameOptions.ScreenShake = !GameOptions.ScreenShake; RefreshUI(); });
        BindButtonListener(rumbleToggleButton, ToggleRumble);
        BindButtonListener(resolutionButton, () => { GameOptions.StepResolution(1); RefreshUI(); });
        BindButtonListener(vsyncToggleButton, () => { GameOptions.VSync = !GameOptions.VSync; RefreshUI(); });
        if (tabButtons != null)
            for (int i = 0; i < tabButtons.Length; i++)
            {
                int tab = i;
                BindButtonListener(tabButtons[i], () => ShowTab(tab));
            }
        BindSlider(musicSlider, value => { GameOptions.MusicVolume = Mathf.RoundToInt(value); RefreshUI(); });
        BindSlider(sfxSlider, value => { GameOptions.SfxVolume = Mathf.RoundToInt(value); RefreshUI(); });
        BindButtonListener(resetProgressButton, OnResetProgressPressed);
        BindButtonListener(confirmResetButton, ConfirmResetProgress);
        BindButtonListener(cancelResetButton, CancelResetProgress);
    }

    private void ToggleRumble()
    {
        GameOptions.GamepadRumble = !GameOptions.GamepadRumble;
        if (AppBootstrap.Instance != null) GameOptions.PulseGamepad(AppBootstrap.Instance);   // feel it when turning it on
        RefreshUI();
    }

    private void BindSlider(Slider slider, UnityAction<float> listener)
    {
        if (slider == null) return;
        slider.minValue = 0; slider.maxValue = GameOptions.MaxVolume; slider.wholeNumbers = true;
        slider.onValueChanged.RemoveListener(listener);
        slider.onValueChanged.AddListener(listener);
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
        HandleTabShortcuts();
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

    public bool IsOpen => isPanelExplicitlyOpen;

    /// <summary>Start / Esc outside a level: open Settings, or close it when it is the menu on top.</summary>
    public void ToggleFromStart()
    {
        if (MenuBackStack.ClosedThisFrame) return;
        if (isPanelExplicitlyOpen) { if (MenuBackStack.IsTop(this)) CloseSettings(); return; }
        if (!MenuBackStack.HasOpen) OpenSettings();
    }

    public void OpenSettings()
    {
        isPanelExplicitlyOpen = true;
        MenuBackStack.Push(this, CloseSettings);

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
        ApplyPlatformLayout();
        if (tabPages != null && tabPages.Length > 0) { ShowTab(currentTab); return; }

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
        MenuBackStack.Remove(this);

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
            MenuBackStack.Push(confirmResetDialog, CancelResetProgress);
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
        if (confirmResetDialog != null) MenuBackStack.Remove(confirmResetDialog);
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
        if (confirmResetDialog != null) MenuBackStack.Remove(confirmResetDialog);
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

        for (int i = 0; tabLabels != null && i < tabLabels.Length && i < TabKeys.Length; i++)
            if (tabLabels[i] != null) tabLabels[i].text = GameLocalization.Get(TabKeys[i]);
        if (musicLabel != null) musicLabel.text = GameLocalization.Get("SETTINGS_MUSIC_VOLUME");
        if (sfxLabel != null) sfxLabel.text = GameLocalization.Get("SETTINGS_SFX_VOLUME");
        if (musicSlider != null) musicSlider.SetValueWithoutNotify(GameOptions.MusicVolume);
        if (sfxSlider != null) sfxSlider.SetValueWithoutNotify(GameOptions.SfxVolume);
        if (musicValueText != null) musicValueText.text = GameOptions.MusicVolume.ToString();
        if (sfxValueText != null) sfxValueText.text = GameOptions.SfxVolume.ToString();
        if (shakeStatusText != null) shakeStatusText.text = GameLocalization.Get(GameOptions.ScreenShake ? "SETTINGS_SHAKE_ON" : "SETTINGS_SHAKE_OFF");
        if (rumbleStatusText != null) rumbleStatusText.text = GameLocalization.Get(GameOptions.GamepadRumble ? "SETTINGS_RUMBLE_ON" : "SETTINGS_RUMBLE_OFF");
        if (vsyncStatusText != null) vsyncStatusText.text = GameLocalization.Get(GameOptions.VSync ? "SETTINGS_VSYNC_ON" : "SETTINGS_VSYNC_OFF");
        if (resolutionText != null)
        {
            Vector2Int size = GameOptions.CurrentResolution;
            resolutionText.text = GameLocalization.Get("SETTINGS_RESOLUTION", $"{size.x}x{size.y}");
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
            gemsCountText.text = $"x{(SaveManager.Instance != null ? SaveManager.Instance.CountCollectedGems() : 0)}";
        if (cloversCountText != null)
            cloversCountText.text = $"x{(SaveManager.Instance != null ? SaveManager.Instance.LoadTotalTrebols() : 0)}";

        // Versión del juego
        if (versionText != null)
        {
            versionText.text = AppVersionDisplay.GetFormattedVersion();
        }
    }
}
