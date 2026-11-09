using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
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

        if (openSettingsButton != null)
        {
            openSettingsButton.onClick.RemoveListener(OpenSettings);
            openSettingsButton.onClick.AddListener(OpenSettings);
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(CloseSettings);
            closeButton.onClick.AddListener(CloseSettings);
        }

        if (musicToggleButton != null)
        {
            musicToggleButton.onClick.RemoveListener(ToggleMusic);
            musicToggleButton.onClick.AddListener(ToggleMusic);
        }

        if (sfxToggleButton != null)
        {
            sfxToggleButton.onClick.RemoveListener(ToggleSFX);
            sfxToggleButton.onClick.AddListener(ToggleSFX);
        }

        if (hapticsToggleButton != null)
        {
            hapticsToggleButton.onClick.RemoveListener(ToggleHaptics);
            hapticsToggleButton.onClick.AddListener(ToggleHaptics);
        }

        if (languageToggleButton != null)
        {
            languageToggleButton.onClick.RemoveListener(ToggleLanguage);
            languageToggleButton.onClick.AddListener(ToggleLanguage);
        }

        if (resetProgressButton != null)
        {
            resetProgressButton.onClick.RemoveListener(OnResetProgressPressed);
            resetProgressButton.onClick.AddListener(OnResetProgressPressed);
        }

        if (confirmResetButton != null)
        {
            confirmResetButton.onClick.RemoveListener(ConfirmResetProgress);
            confirmResetButton.onClick.AddListener(ConfirmResetProgress);
        }

        if (cancelResetButton != null)
        {
            cancelResetButton.onClick.RemoveListener(CancelResetProgress);
            cancelResetButton.onClick.AddListener(CancelResetProgress);
        }
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
            GameObject hObj = existing != null ? existing.gameObject : Instantiate(musicToggleButton.gameObject, container);
            hObj.name = "HapticsToggleButton";
            RectTransform hrt = hObj.GetComponent<RectTransform>();
            hrt.anchoredPosition = new Vector2(0f, -30f);
            hrt.sizeDelta = new Vector2(420f, 60f);

            hapticsToggleButton = hObj.GetComponent<Button>();
            hapticsStatusText = hObj.GetComponentInChildren<TextMeshProUGUI>();
            hapticsToggleButton.onClick.RemoveAllListeners();
            hapticsToggleButton.onClick.AddListener(ToggleHaptics);
        }

        // 3. Language Button
        if (languageToggleButton == null && musicToggleButton != null)
        {
            Transform existing = container.Find("LanguageToggleButton");
            GameObject lObj = existing != null ? existing.gameObject : Instantiate(musicToggleButton.gameObject, container);
            lObj.name = "LanguageToggleButton";
            RectTransform lrt = lObj.GetComponent<RectTransform>();
            lrt.anchoredPosition = new Vector2(0f, -100f);
            lrt.sizeDelta = new Vector2(420f, 60f);

            languageToggleButton = lObj.GetComponent<Button>();
            languageStatusText = lObj.GetComponentInChildren<TextMeshProUGUI>();
            languageToggleButton.onClick.RemoveAllListeners();
            languageToggleButton.onClick.AddListener(ToggleLanguage);
        }

        // 4. Reset Progress Button
        if (resetProgressButton == null && closeButton != null)
        {
            Transform existing = container.Find("ResetProgressButton");
            GameObject rObj = existing != null ? existing.gameObject : Instantiate(closeButton.gameObject, container);
            rObj.name = "ResetProgressButton";
            RectTransform rrt = rObj.GetComponent<RectTransform>();
            rrt.anchoredPosition = new Vector2(0f, -170f);
            rrt.sizeDelta = new Vector2(420f, 55f);

            resetProgressButton = rObj.GetComponent<Button>();
            resetProgressText = rObj.GetComponentInChildren<TextMeshProUGUI>();
            resetProgressButton.onClick.RemoveAllListeners();
            resetProgressButton.onClick.AddListener(OnResetProgressPressed);
        }

        // Botón Close abajo
        if (closeButton != null)
        {
            RectTransform clrt = closeButton.GetComponent<RectTransform>();
            clrt.anchoredPosition = new Vector2(0f, -240f);
            clrt.sizeDelta = new Vector2(260f, 55f);
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

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
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
        bool isEs = lang == "ES";

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
            sfxStatusText.text = sfxOn ? (isEs ? "SFX: SI" : "SFX: ON") : (isEs ? "SFX: NO" : "SFX: OFF");
            sfxStatusText.color = sfxOn ? enabledColor : disabledColor;
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
    }
}
