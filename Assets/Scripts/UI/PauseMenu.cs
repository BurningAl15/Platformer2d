using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

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

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
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
            InputManager.Instance.EnableUIOnlyInput();
            SelectFirstButton(firstSelectedButton);

            pauseScreen.transform.DOKill();
            pauseScreen.transform.DOLocalMoveY(endPoint, 0.25f)
                .SetEase(Ease.OutBack)
                .SetUpdate(true);
        }
        else
        {
            ResumeGame();
        }
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
        pauseScreen.transform.DOLocalMoveY(initPoint, 0.25f)
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
        TurnCanvasGroup(false);
        InputManager.Instance?.EnablePlayerInput();
    }

    private void TurnCanvasGroup(bool active)
    {
        pauseBG.interactable = active;
        pauseBG.blocksRaycasts = active;
        pauseBG.alpha = active ? 1f : 0f;
    }

    private void SelectFirstButton(GameObject button)
    {
        if (button == null || EventSystem.current == null) return;
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(button);
    }
}
