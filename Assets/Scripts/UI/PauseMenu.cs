using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Gestiona la pausa del juego.
/// - Se suscribe a InputManager.OnPausePressed.
/// - Usa DOTween con SetUpdate(true) para animar aunque Time.timeScale = 0.
/// - DOTween.Init se hace en AppBootstrap, no aquí.
/// </summary>
public class PauseMenu : MonoBehaviour
{
    public static PauseMenu _instance;

    [SerializeField] private CanvasGroup pauseBG;
    [SerializeField] private GameObject pauseScreen;
    public bool isPaused;

    [Tooltip("Posición Y oculta del panel (fuera de pantalla)")]
    [SerializeField] private float initPoint;
    [Tooltip("Posición Y visible del panel")]
    [SerializeField] private float endPoint;
    [SerializeField] private GameObject firstSelectedButton;
    [Header("Stats (icon + amount)")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI gemsText;
    [SerializeField] private Image cloverIcon;
    [SerializeField] private TextMeshProUGUI heartsText;
    [SerializeField] private TextMeshProUGUI fragmentsText;
    [Tooltip("Restarts the level; useful when chasing a no-damage run.")]
    [SerializeField] private Button restartButton;
    [SerializeField] private TextMeshProUGUI restartText;
    [SerializeField] private TextMeshProUGUI livesText;
    [SerializeField] private Color missingTint = new Color(0.35f, 0.38f, 0.55f, 1f);

    [Header("Portrait")]
    [SerializeField] private Image portrait;
    [Tooltip("Original idle frame; the portrait shows the selected skin's copy.")]
    [SerializeField] private Sprite portraitSource;
    private RectTransform pauseRect;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        pauseRect = pauseScreen != null ? pauseScreen.GetComponent<RectTransform>() : null;
    }

    private void OnEnable()
    {
        if (InputManager.Instance != null)
            InputManager.Instance.OnPausePressed += Pause;
    }

    private void OnDisable()
    {
        if (InputManager.Instance != null)
            InputManager.Instance.OnPausePressed -= Pause;
    }

    private void Start()
    {
        if (restartButton != null) restartButton.onClick.AddListener(RestartLevel);
        initPoint = CalculateHiddenPosition();
        SetPausePanelY(initPoint);
        TurnCanvasGroup(false);
    }

    // ───────────────────────────────────────────────
    //  API pública
    // ───────────────────────────────────────────────

    public void Pause()
    {
        // Esc is both Pause and Cancel: if Cancel already closed a menu this frame, do nothing.
        if (MenuBackStack.ClosedThisFrame) return;
        // With a menu open over the pause (e.g. Settings), Start/Esc leaves it to Cancel.
        if (isPaused && MenuBackStack.HasOpen && !MenuBackStack.IsTop(this)) return;
        isPaused = !isPaused;

        if (isPaused)
        {
            // Congelar el juego INMEDIATAMENTE, antes de animar el panel
            Time.timeScale = 0;
            TurnCanvasGroup(true);
            // A fade in progress sits above this panel; never let it eat the menu's taps.
            if (FadeEffect._instance != null)
                FadeEffect._instance.ReleaseInputIfVisible();
            RefreshPauseStats();
            MenuBackStack.Push(this, PauseAuxiliar);
            // The touch controls canvas renders above this panel and would take its clicks.
            PlatformDetector.Instance?.SetGameplayControlsSuppressed(true);
            InputManager.Instance?.EnableUIOnlyInput();
            UISelectionUtility.SelectDefault(firstSelectedButton, pauseScreen != null ? pauseScreen.transform : transform);

            pauseScreen.transform.DOKill();
            SetPausePanelY(endPoint, true)
                .SetEase(Ease.OutBack)
                .SetUpdate(true);
        }
        else
        {
            ResumeGame();
        }
    }

    private void RefreshPauseStats()
    {
        if (restartText != null) restartText.text = GameLocalization.Get("PAUSE_RESTART");
        SaveManager save = SaveManager.Instance;
        int levelNumber = SceneUtils.Get_CurrentLevelName();
        int currentGems = LevelManager._instance != null ? LevelManager._instance.GetGemsCollected() : 0;
        int totalGems = LevelManager._instance != null ? LevelManager._instance.GetTotalGemsInLevel() : 0;
        int fragments = save != null ? save.LoadHeartFragmentCount() % 2 : 0;
        int permanentHearts = save != null ? save.LoadPermanentHeartCount() : 0;
        int lives = save != null ? save.LoadLives() : 0;
        bool hasClover = save != null && save.HasTrebol(levelNumber);

        if (titleText != null) titleText.text = GameLocalization.Get("UI_PAUSED");
        if (gemsText != null) gemsText.text = $"{currentGems}/{totalGems}";
        if (heartsText != null) heartsText.text = $"x{3 + permanentHearts}";
        if (fragmentsText != null) fragmentsText.text = $"{fragments}/2";
        if (livesText != null) livesText.text = $"x{lives}";
        if (cloverIcon != null) cloverIcon.color = hasClover ? Color.white : missingTint;
        if (portrait != null && portraitSource != null) portrait.sprite = PlayerSkins.Resolve(portraitSource);
    }

    /// <summary>
    /// Reanuda el juego desde un botón externo (e.g. botón de continuar en el panel).
    /// </summary>
    public void PauseAuxiliar()
    {
        isPaused = false;
        ResumeGame();
    }

    /// <summary>Restarts the current level from the start (no life is spent).</summary>
    public void RestartLevel()
    {
        ForceResume();
        SceneUtils.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    public void LevelSelection()
    {
        ForceResume();
        SceneUtils.ToSelectionScene();
    }

    public void MainMenu()
    {
        ForceResume();
        SceneUtils.ToMainScene();
    }

    // ───────────────────────────────────────────────
    //  Helpers privados
    // ───────────────────────────────────────────────

    private void ResumeGame()
    {
        MenuBackStack.Remove(this);
        pauseScreen.transform.DOKill();
        SetPausePanelY(CalculateHiddenPosition(), true)
            .SetEase(Ease.InBack)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                TurnCanvasGroup(false);
                Time.timeScale = 1;
                PlatformDetector.Instance?.SetGameplayControlsSuppressed(false);
                InputManager.Instance?.EnablePlayerInput();

                if (EventSystem.current != null)
                    EventSystem.current.SetSelectedGameObject(null);
            });
    }

    /// <summary>
    /// Resetea el estado de pausa sin animación (para transiciones de escena).
    /// </summary>
    private void ForceResume()
    {
        MenuBackStack.Remove(this);
        isPaused = false;
        Time.timeScale = 1;
        pauseScreen.transform.DOKill();
        SetPausePanelY(CalculateHiddenPosition());
        TurnCanvasGroup(false);
        PlatformDetector.Instance?.SetGameplayControlsSuppressed(false);
        InputManager.Instance?.EnablePlayerInput();
    }

    private void TurnCanvasGroup(bool active)
    {
        if (pauseBG == null) return;
        pauseBG.interactable = active;
        pauseBG.blocksRaycasts = active;
        pauseBG.alpha = active ? 1f : 0f;
    }

    private float CalculateHiddenPosition()
    {
        if (pauseRect == null || !(pauseRect.parent is RectTransform parentRect))
            return initPoint;

        Canvas.ForceUpdateCanvases();
        float panelHalfHeight = pauseRect.rect.height * 0.5f;
        float parentHalfHeight = parentRect.rect.height * 0.5f;
        return -(panelHalfHeight + parentHalfHeight + 24f);
    }

    private Tween SetPausePanelY(float y, bool animate = false)
    {
        if (pauseRect == null) return null;

        if (animate)
            return pauseRect.DOAnchorPosY(y, 0.25f).SetUpdate(true);

        Vector2 anchoredPosition = pauseRect.anchoredPosition;
        anchoredPosition.y = y;
        pauseRect.anchoredPosition = anchoredPosition;
        return null;
    }

}
