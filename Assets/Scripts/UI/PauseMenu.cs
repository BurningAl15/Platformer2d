using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using TMPro;

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
    [SerializeField] private TextMeshProUGUI pauseStatsText;
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
        if (pauseStatsText == null && pauseScreen != null)
        {
            Transform title = pauseScreen.transform.Find("Text (TMP)");
            pauseStatsText = title != null
                ? title.GetComponent<TextMeshProUGUI>()
                : pauseScreen.GetComponentInChildren<TextMeshProUGUI>(true);
        }
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
        initPoint = CalculateHiddenPosition();
        SetPausePanelY(initPoint);
        TurnCanvasGroup(false);
    }

    // ───────────────────────────────────────────────
    //  API pública
    // ───────────────────────────────────────────────

    public void Pause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            // Congelar el juego INMEDIATAMENTE, antes de animar el panel
            Time.timeScale = 0;
            TurnCanvasGroup(true);
            RefreshPauseStats();
            InputManager.Instance.EnableUIOnlyInput();
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
        if (pauseStatsText == null) return;

        SaveManager save = SaveManager.Instance;
        int levelNumber = SceneUtils.Get_CurrentLevelName();
        int currentGems = LevelManager._instance != null ? LevelManager._instance.GetGemsCollected() : 0;
        int totalGems = LevelManager._instance != null ? LevelManager._instance.GetTotalGemsInLevel() : 0;
        int fragmentCount = save != null ? save.LoadHeartFragmentCount() : 0;
        int permanentHearts = save != null ? save.LoadPermanentHeartCount() : 0;
        int lives = save != null ? save.LoadLives() : 0;
        bool hasClover = save != null && save.HasTrebol(levelNumber);
        bool spanish = PlayerPrefs.GetString("SelectedLanguage", "EN") == "ES";

        pauseStatsText.enableAutoSizing = true;
        pauseStatsText.fontSizeMin = 22f;
        pauseStatsText.fontSizeMax = 48f;
        pauseStatsText.textWrappingMode = TextWrappingModes.Normal;
        pauseStatsText.text = spanish
            ? $"PAUSA\nGEMAS: {currentGems}/{totalGems}  |  TRÉBOL: {(hasClover ? "SÍ" : "NO")}\nFRAGMENTOS: {fragmentCount % 2}/2  |  CORAZONES: {3 + permanentHearts}  |  VIDAS: {lives}"
            : $"PAUSED\nGEMS: {currentGems}/{totalGems}  |  CLOVER: {(hasClover ? "YES" : "NO")}\nFRAGMENTS: {fragmentCount % 2}/2  |  HEARTS: {3 + permanentHearts}  |  LIVES: {lives}";
    }

    /// <summary>
    /// Reanuda el juego desde un botón externo (e.g. botón de continuar en el panel).
    /// </summary>
    public void PauseAuxiliar()
    {
        isPaused = false;
        ResumeGame();
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
        pauseScreen.transform.DOKill();
        SetPausePanelY(CalculateHiddenPosition(), true)
            .SetEase(Ease.InBack)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                TurnCanvasGroup(false);
                Time.timeScale = 1;
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
        isPaused = false;
        Time.timeScale = 1;
        pauseScreen.transform.DOKill();
        SetPausePanelY(CalculateHiddenPosition());
        TurnCanvasGroup(false);
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
