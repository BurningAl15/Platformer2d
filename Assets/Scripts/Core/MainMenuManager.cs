using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using UnityEngine.UI;

/// <summary>
/// Gestiona la pantalla del menú principal.
/// - Muestra/oculta el botón "Continue" según si hay partida guardada.
/// - "Nueva partida": borra progreso (vía SaveManager), muestra la historia y luego el tutorial.
/// - "Continuar": va a la pantalla de selección de niveles.
/// </summary>
public class MainMenuManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject continueButton;
    [SerializeField] private Image textImg;
    [SerializeField] private CanvasGroup panelBG;
    [SerializeField] private GameObject panelOptions;
    [SerializeField] private float initPoint, endPoint;
    [SerializeField] private AnimationCurve initCurve;

    [Header("Botones de selección")]
    [Tooltip("Primer botón cuando el jugador YA jugó (Continue visible)")]
    [SerializeField] private GameObject continueFirstButton;
    [Tooltip("Primer botón cuando el jugador NUNCA jugó (Continue oculto)")]
    [SerializeField] private GameObject newGameFirstButton;
    [Tooltip("Primer botón del panel de confirmación de nueva partida")]
    [SerializeField] private GameObject panelFirstButton;
    [Tooltip("Botón de tienda; su navegación izquierda apunta dinámicamente a Continue o New Game.")]
    [SerializeField] private Button storeButton;

    [Header("Versión")]
    [SerializeField] private TMPro.TextMeshProUGUI versionText;

    private const string StorySceneName = "Story";

    private bool hasPlayedBefore = false;

    private void Awake()
    {
        // DOTween inicializado en AppBootstrap
    }

    private void OnDestroy()
    {
        if (InputManager.Instance != null) InputManager.Instance.OnPausePressed -= OpenSettingsFromStart;
    }

    /// <summary>Start / Esc on the title screen opens Settings (or closes it).</summary>
    private void OpenSettingsFromStart()
    {
        SettingsController settings = FindFirstObjectByType<SettingsController>();
        if (settings != null && settings.gameObject.scene == gameObject.scene) settings.ToggleFromStart();
    }

    private void Start()
    {
        // InputManager lives in this scene too, so subscribe once every Awake has run.
        if (InputManager.Instance != null) InputManager.Instance.OnPausePressed += OpenSettingsFromStart;
        hasPlayedBefore = HasContinueProgress();

        continueButton.SetActive(hasPlayedBefore);
        ConfigureStoreNavigation();
        SelectButton(hasPlayedBefore ? continueFirstButton : newGameFirstButton);

        panelBG.alpha = 0;
        panelBG.interactable = false;
        panelBG.blocksRaycasts = false;

        EnsureVersionLabel();

        if (!PlatformFeatures.HasQuitButton)
        {
            Transform quit = transform.root.Find("Canvas/Quit-Button");
            if (quit == null) quit = FindQuitButton();
            if (quit != null) quit.gameObject.SetActive(false);
        }
    }

    private Transform FindQuitButton()
    {
        foreach (Button button in FindObjectsByType<Button>(FindObjectsInactive.Include))
            if (button.name == "Quit-Button" && button.gameObject.scene == gameObject.scene) return button.transform;
        return null;
    }

    private void EnsureVersionLabel()
    {
        if (versionText == null)
        {
            Transform existing = transform.Find("AppVersionText");
            if (existing != null)
            {
                versionText = existing.GetComponent<TMPro.TextMeshProUGUI>();
            }
            else
            {
                GameObject vObj = new GameObject("AppVersionText");
                vObj.transform.SetParent(transform, false);
                RectTransform vrt = vObj.AddComponent<RectTransform>();
                vrt.anchorMin = new Vector2(0f, 0f);
                vrt.anchorMax = new Vector2(0f, 0f);
                vrt.pivot = new Vector2(0f, 0f);
                vrt.anchoredPosition = new Vector2(30f, 20f);
                vrt.sizeDelta = new Vector2(350f, 35f);

                versionText = vObj.AddComponent<TMPro.TextMeshProUGUI>();
                TMPro.TextMeshProUGUI sampleText = GetComponentInChildren<TMPro.TextMeshProUGUI>();
                if (sampleText != null)
                {
                    versionText.font = sampleText.font;
                    versionText.fontSharedMaterial = sampleText.fontSharedMaterial;
                }
                versionText.alignment = TMPro.TextAlignmentOptions.BottomLeft;
                versionText.fontSize = 24f;
                versionText.color = new Color(1f, 1f, 1f, 0.65f);
                vObj.AddComponent<AppVersionDisplay>();
            }
        }

        if (versionText != null)
        {
            versionText.text = AppVersionDisplay.GetFormattedVersion();
        }
    }

    // ───────────────────────────────────────────────
    //  Acciones de botones
    // ───────────────────────────────────────────────

    /// <summary>
    /// Botón "Nueva partida". Si hay partida guardada, muestra confirmación.
    /// Si no hay partida, inicia directamente.
    /// </summary>
    public void ActivatePanel()
    {
        if (hasPlayedBefore)
        {
            panelBG.alpha = 1;
            panelBG.interactable = true;
            panelBG.blocksRaycasts = true;
            MenuBackStack.Push(this, DeactivatePanel);
            panelOptions.transform
                .DOLocalMoveY(endPoint, 0.5f)
                .SetEase(initCurve)
                .OnComplete(() => SelectButton(panelFirstButton));
        }
        else
        {
            StartNewGame();
        }
    }

    /// <summary>
    /// Confirma y cancela el panel de nueva partida.
    /// </summary>
    public void DeactivatePanel()
    {
        MenuBackStack.Remove(this);
        panelOptions.transform
            .DOLocalMoveY(initPoint, 0.5f)
            .SetEase(initCurve)
            .OnComplete(TurnOffPanel);
    }

    /// <summary>
    /// Inicia el nivel 0 (tutorial) borrando el progreso anterior.
    /// </summary>
    public void ToNextScene() => StartNewGame();

    public void ContinueScene()
    {
        bool tutorialIncomplete = SaveManager.Instance != null
            ? SaveManager.Instance.HasIncompleteTutorial()
            : IsTutorialIncompleteInPrefs();

        if (tutorialIncomplete)
        {
            SceneUtils.LoadGameplayScene(0);
            return;
        }

        SceneUtils.ToSelectionScene();
    }

    public void QuitGame() => Application.Quit();

    // ───────────────────────────────────────────────
    //  Helpers privados
    // ───────────────────────────────────────────────

    private void StartNewGame()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.ResetProgress();
            SaveManager.Instance.MarkTutorialStarted();
        }
        else
        {
            PlayerPrefs.SetInt("TutorialStarted", 1);
            PlayerPrefs.SetInt("TutorialCompleted", 0);
            PlayerPrefs.Save();
        }

        // The intro story plays first and then loads the tutorial (Level_0). If a
        // build is missing the story scene, never strand the player on the menu.
        if (Application.CanStreamedLevelBeLoaded(StorySceneName))
            SceneUtils.LoadScene(StorySceneName);
        else
            SceneUtils.LoadGameplayScene(0);
    }

    private bool HasContinueProgress()
    {
        if (SaveManager.Instance != null)
            return SaveManager.Instance.HasSaveData();

        if (IsTutorialIncompleteInPrefs())
            return true;

        for (int level = 1; level <= 10; level++)
        {
            if (PlayerPrefs.GetInt($"Level_{level}", 0) == 1)
                return true;
        }

        return false;
    }

    private static bool IsTutorialIncompleteInPrefs()
    {
        bool tutorialStarted = PlayerPrefs.GetInt("TutorialStarted", 0) == 1;
        bool tutorialCompleted = PlayerPrefs.GetInt("TutorialCompleted", 0) == 1 ||
                                 PlayerPrefs.GetInt("Level_1", 0) == 1;
        return tutorialStarted && !tutorialCompleted;
    }

    private void ConfigureStoreNavigation()
    {
        if (storeButton == null)
        {
            Transform storeTransform = transform.root.Find("Canvas/Store-Button");
            if (storeTransform != null)
                storeButton = storeTransform.GetComponent<Button>();
        }

        GameObject firstButton = hasPlayedBefore ? continueFirstButton : newGameFirstButton;
        Selectable firstSelectable = firstButton != null
            ? firstButton.GetComponent<Selectable>()
            : null;

        if (storeButton == null || firstSelectable == null)
            return;

        Navigation navigation = storeButton.navigation;
        navigation.mode = Navigation.Mode.Explicit;
        navigation.selectOnLeft = firstSelectable;
        storeButton.navigation = navigation;
    }

    private void TurnOffPanel()
    {
        panelBG.alpha = 0;
        panelBG.interactable = false;
        panelBG.blocksRaycasts = false;
        SelectButton(hasPlayedBefore ? continueFirstButton : newGameFirstButton);
    }

    private void SelectButton(GameObject button)
    {
        if (button == null || EventSystem.current == null) return;
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(button);
    }
}
