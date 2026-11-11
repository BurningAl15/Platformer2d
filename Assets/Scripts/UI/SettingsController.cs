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
    [SerializeField] private Button languageToggleButton;
    [SerializeField] private TextMeshProUGUI languageStatusText;

    [Header("Progreso del Jugador")]
    [SerializeField] private TextMeshProUGUI progressSummaryText;

    [Header("Versión")]
    [SerializeField] private TextMeshProUGUI versionText;

    [Header("Gestión de Guardado")]
    [SerializeField] private Button resetProgressButton;
    [SerializeField] private TextMeshProUGUI resetProgressText;
    [SerializeField] private GameObject confirmResetDialog;
    [SerializeField] private Button confirmResetButton;
    [SerializeField] private Button cancelResetButton;

    [Header("Colores de Estado")]
    [SerializeField] private Color enabledColor = new Color(0.35f, 0.95f, 0.45f, 1f); // Verde menta
    [SerializeField] private Color disabledColor = new Color(0.85f, 0.35f, 0.35f, 1f); // Rojo suave

    private bool isPanelExplicitlyOpen = false;
    private bool awaitingResetConfirmation = false;
    private float resetConfirmTimer = 0f;

    private void Awake()
    {
        EnsureUIElementsExist();
        BindSettingsListeners();
    }

    private void BindSettingsListeners()
    {
        BindButtonListener(openSettingsButton, OpenSettings);
        BindButtonListener(closeButton, CloseSettings);
        BindButtonListener(musicToggleButton, ToggleMusic);
        BindButtonListener(sfxToggleButton, ToggleSFX);
        BindButtonListener(hapticsToggleButton, ToggleHaptics);
        BindButtonListener(languageToggleButton, ToggleLanguage);
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

    private Button CloneButtonWithoutSerializedActions(Button source, Transform parent, string objectName)
    {
        GameObject clone = Instantiate(source.gameObject, parent, false);
        clone.name = objectName;

        Button clonedButton = clone.GetComponent<Button>();
        if (clonedButton == null)
            clonedButton = clone.AddComponent<Button>();

        // Button.onClick has a public setter. Reset the cloned UnityEvent in
        // place so copied Inspector-persistent actions cannot toggle Music or
        // close Settings. Destroy()+AddComponent() is deferred until end of
        // frame and can leave the cloned control with the original event (or
        // cause the rest of panel initialization to abort).
        clonedButton.onClick = new Button.ButtonClickedEvent();
        return clonedButton;
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

    private void EnsureUIElementsExist()
    {
        if (musicToggleButton != null)
            musicStatusText = musicToggleButton.GetComponentInChildren<TextMeshProUGUI>();
        if (sfxToggleButton != null)
            sfxStatusText = sfxToggleButton.GetComponentInChildren<TextMeshProUGUI>();

        Transform container = null;
        if (musicToggleButton != null)
            container = musicToggleButton.transform.parent;
        else if (settingsPanel != null)
            container = settingsPanel.transform.Find("Container") ?? settingsPanel.transform;

        if (container == null) return;

        // Ampliar contenedor para que quepan holgadamente todas las opciones
        RectTransform crt = container.GetComponent<RectTransform>();
        if (crt != null)
        {
            crt.sizeDelta = new Vector2(Mathf.Max(crt.sizeDelta.x, 620f), 600f);
        }

        // Título arriba
        Transform title = container.Find("SettingsTitle") ?? container.Find("Title");
        if (title != null)
        {
            RectTransform trt = title.GetComponent<RectTransform>();
            trt.anchoredPosition = new Vector2(0f, 235f);
            trt.sizeDelta = new Vector2(500f, 50f);
        }

        // 1. Resumen de progreso
        if (progressSummaryText == null && musicStatusText != null)
        {
            Transform existing = container.Find("ProgressSummaryText");
            if (existing != null)
            {
                progressSummaryText = existing.GetComponent<TextMeshProUGUI>();
            }
            else
            {
                GameObject pObj = new GameObject("ProgressSummaryText");
                pObj.transform.SetParent(container, false);
                RectTransform prt = pObj.AddComponent<RectTransform>();
                prt.anchorMin = new Vector2(0.5f, 0.5f);
                prt.anchorMax = new Vector2(0.5f, 0.5f);
                prt.pivot = new Vector2(0.5f, 0.5f);
                prt.anchoredPosition = new Vector2(0f, 175f);
                prt.sizeDelta = new Vector2(580f, 38f);

                progressSummaryText = pObj.AddComponent<TextMeshProUGUI>();
                progressSummaryText.font = musicStatusText.font;
                progressSummaryText.alignment = TextAlignmentOptions.Center;
                progressSummaryText.enableAutoSizing = true;
                progressSummaryText.fontSizeMin = 14f;
                progressSummaryText.fontSizeMax = 22f;
                progressSummaryText.color = new Color(1f, 0.9f, 0.3f, 1f);
            }
        }

        // Reposicionar Music y SFX
        if (musicToggleButton != null)
        {
            RectTransform mrt = musicToggleButton.GetComponent<RectTransform>();
            mrt.anchoredPosition = new Vector2(0f, 110f);
            mrt.sizeDelta = new Vector2(420f, 60f);
        }

        if (sfxToggleButton != null)
        {
            RectTransform srt = sfxToggleButton.GetComponent<RectTransform>();
            srt.anchoredPosition = new Vector2(0f, 40f);
            srt.sizeDelta = new Vector2(420f, 60f);
        }

        // 2. Haptics Button
        if (hapticsToggleButton == null && musicToggleButton != null)
        {
            Transform existing = container.Find("HapticsToggleButton");
            Button clonedButton = existing != null
                ? existing.GetComponent<Button>()
                : CloneButtonWithoutSerializedActions(musicToggleButton, container, "HapticsToggleButton");
            GameObject hObj = clonedButton.gameObject;
            hObj.name = "HapticsToggleButton";
            RectTransform hrt = hObj.GetComponent<RectTransform>();
            hrt.anchoredPosition = new Vector2(0f, -30f);
            hrt.sizeDelta = new Vector2(420f, 60f);

            hapticsToggleButton = clonedButton;
            hapticsStatusText = hObj.GetComponentInChildren<TextMeshProUGUI>();
        }

        // 3. Language Button
        if (languageToggleButton == null && musicToggleButton != null)
        {
            Transform existing = container.Find("LanguageToggleButton");
            Button clonedButton = existing != null
                ? existing.GetComponent<Button>()
                : CloneButtonWithoutSerializedActions(musicToggleButton, container, "LanguageToggleButton");
            GameObject lObj = clonedButton.gameObject;
            lObj.name = "LanguageToggleButton";
            RectTransform lrt = lObj.GetComponent<RectTransform>();
            lrt.anchoredPosition = new Vector2(0f, -100f);
            lrt.sizeDelta = new Vector2(420f, 60f);

            languageToggleButton = clonedButton;
            languageStatusText = lObj.GetComponentInChildren<TextMeshProUGUI>();
        }

        // 4. Reset Progress Button
        if (resetProgressButton == null && closeButton != null)
        {
            Transform existing = container.Find("ResetProgressButton");
            Button clonedButton = existing != null
                ? existing.GetComponent<Button>()
                : CloneButtonWithoutSerializedActions(closeButton, container, "ResetProgressButton");
            GameObject rObj = clonedButton.gameObject;
            rObj.name = "ResetProgressButton";
            RectTransform rrt = rObj.GetComponent<RectTransform>();
            rrt.anchoredPosition = new Vector2(0f, -170f);
            rrt.sizeDelta = new Vector2(420f, 55f);

            resetProgressButton = clonedButton;
            resetProgressText = rObj.GetComponentInChildren<TextMeshProUGUI>();
        }

        // Botón Close abajo
        if (closeButton != null)
        {
            RectTransform clrt = closeButton.GetComponent<RectTransform>();
            clrt.anchoredPosition = new Vector2(0f, -240f);
            clrt.sizeDelta = new Vector2(260f, 55f);
        }

        // 5. Etiqueta de versión abajo
        if (versionText == null && musicStatusText != null)
        {
            Transform existing = container.Find("VersionText");
            if (existing != null)
            {
                versionText = existing.GetComponent<TextMeshProUGUI>();
            }
            else
            {
                GameObject vObj = new GameObject("VersionText");
                vObj.transform.SetParent(container, false);
                RectTransform vrt = vObj.AddComponent<RectTransform>();
                vrt.anchorMin = new Vector2(0.5f, 0.5f);
                vrt.anchorMax = new Vector2(0.5f, 0.5f);
                vrt.pivot = new Vector2(0.5f, 0.5f);
                vrt.anchoredPosition = new Vector2(0f, -282f);
                vrt.sizeDelta = new Vector2(300f, 24f);

                versionText = vObj.AddComponent<TextMeshProUGUI>();
                versionText.font = musicStatusText.font;
                versionText.alignment = TextAlignmentOptions.Center;
                versionText.fontSize = 16f;
                versionText.color = new Color(0.7f, 0.7f, 0.7f, 0.8f);
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

        // The settings panel starts inactive in both scenes, so construct its
        // optional controls on the explicit open path as well as in Awake.
        EnsureUIElementsExist();
        BindSettingsListeners();
        ConfigureSettingsNavigation();

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

    private void ConfigureSettingsNavigation()
    {
        Button[] buttons =
        {
            musicToggleButton,
            sfxToggleButton,
            hapticsToggleButton,
            languageToggleButton,
            resetProgressButton,
            closeButton
        };

        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] == null) continue;
            Navigation navigation = new Navigation { mode = Navigation.Mode.Explicit };
            navigation.selectOnUp = FindPreviousInteractable(buttons, i);
            navigation.selectOnDown = FindNextInteractable(buttons, i);
            buttons[i].navigation = navigation;
        }
    }

    private static Selectable FindPreviousInteractable(Button[] buttons, int index)
    {
        for (int step = 1; step < buttons.Length; step++)
        {
            int candidate = (index - step + buttons.Length) % buttons.Length;
            if (buttons[candidate] != null && buttons[candidate].interactable)
                return buttons[candidate];
        }
        return buttons[index];
    }

    private static Selectable FindNextInteractable(Button[] buttons, int index)
    {
        for (int step = 1; step < buttons.Length; step++)
        {
            int candidate = (index + step) % buttons.Length;
            if (buttons[candidate] != null && buttons[candidate].interactable)
                return buttons[candidate];
        }
        return buttons[index];
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
    }

    public void ToggleLanguage()
    {
        string current = PlayerPrefs.GetString("SelectedLanguage", "EN");
        string next = current == "EN" ? "ES" : "EN";
        PlayerPrefs.SetString("SelectedLanguage", next);
        PlayerPrefs.Save();
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
            string lang = PlayerPrefs.GetString("SelectedLanguage", "EN");
            if (resetProgressText != null)
            {
                resetProgressText.text = lang == "ES" ? "CONFIRMAR BORRADO" : "TAP TO CONFIRM RESET";
                resetProgressText.color = new Color(1f, 0.9f, 0.2f);
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
            SaveManager.Instance.ResetProgress();
        }

        if (confirmResetDialog != null)
        {
            confirmResetDialog.SetActive(false);
        }

        RefreshUI();

        string lang = PlayerPrefs.GetString("SelectedLanguage", "EN");
        if (resetProgressText != null)
        {
            resetProgressText.text = lang == "ES" ? "PROGRESO REINICIADO!" : "PROGRESS RESET!";
            resetProgressText.color = enabledColor;
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
        string lang = PlayerPrefs.GetString("SelectedLanguage", "EN");
        bool isEs = string.Equals(lang, "ES", System.StringComparison.OrdinalIgnoreCase);

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
            musicStatusText.text = musicOn ? (isEs ? "MUSICA: SI" : "MUSIC: ON") : (isEs ? "MUSICA: NO" : "MUSIC: OFF");
            musicStatusText.color = musicOn ? enabledColor : disabledColor;
        }

        if (sfxStatusText != null)
        {
            sfxStatusText.text = sfxOn ? (isEs ? "EFECTOS: SI" : "SFX: ON") : (isEs ? "EFECTOS: NO" : "SFX: OFF");
            sfxStatusText.color = sfxOn ? enabledColor : disabledColor;
        }

        Transform container = musicToggleButton != null ? musicToggleButton.transform.parent : null;
        Transform title = container != null ? (container.Find("SettingsTitle") ?? container.Find("Title")) : null;
        TextMeshProUGUI titleText = title != null ? title.GetComponent<TextMeshProUGUI>() : null;
        if (titleText != null)
            titleText.text = isEs ? "AJUSTES" : "SETTINGS";

        if (openSettingsButton != null)
        {
            TextMeshProUGUI openText = openSettingsButton.GetComponentInChildren<TextMeshProUGUI>();
            if (openText != null)
                openText.text = isEs ? "AJUSTES" : "SETTINGS";
        }

        // Haptics / Vibración
        bool hapticsOn = PlayerPrefs.GetInt("HapticsEnabled", 1) == 1;
        if (hapticsStatusText != null)
        {
            hapticsStatusText.text = hapticsOn ? (isEs ? "VIBRACION: SI" : "HAPTICS: ON") : (isEs ? "VIBRACION: NO" : "HAPTICS: OFF");
            hapticsStatusText.color = hapticsOn ? enabledColor : disabledColor;
        }

        // Idioma / Language
        if (languageStatusText != null)
        {
            languageStatusText.text = isEs ? "IDIOMA: ESPANOL" : "LANGUAGE: ENGLISH";
        }

        // Reset progress label (si no está esperando confirmación)
        if (!awaitingResetConfirmation && resetProgressText != null)
        {
            resetProgressText.text = isEs ? "REINICIAR PROGRESO" : "RESET PROGRESS";
            resetProgressText.color = disabledColor;
        }

        // Botón Cerrar
        if (closeButton != null)
        {
            TextMeshProUGUI closeTxt = closeButton.GetComponentInChildren<TextMeshProUGUI>();
            if (closeTxt != null)
            {
                closeTxt.text = isEs ? "CERRAR" : "CLOSE";
            }
        }

        // Resumen de progreso
        if (progressSummaryText != null)
        {
            int unlockedCount = 0;
            int totalGems = 0;
            int totalTrebols = 0;

            if (SaveManager.Instance != null)
            {
                for (int i = 1; i <= 10; i++)
                {
                    if (SaveManager.Instance.IsLevelUnlocked(i))
                        unlockedCount++;
                }
                totalGems = SaveManager.Instance.LoadGlobalGems();
                totalTrebols = SaveManager.Instance.LoadTotalTrebols();
            }

            if (isEs)
                progressSummaryText.text = $"NIVELES: {unlockedCount}/10  |  GEMAS: {totalGems}  |  TREBOLES: {totalTrebols}";
            else
                progressSummaryText.text = $"LEVELS: {unlockedCount}/10  |  GEMS: {totalGems}  |  CLOVERS: {totalTrebols}";
        }

        // Versión del juego
        if (versionText != null)
        {
            versionText.text = AppVersionDisplay.GetFormattedVersion();
        }
    }
}
