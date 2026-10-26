using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using UnityEngine.UI;

/// <summary>
/// Gestiona la pantalla del menú principal.
/// - Muestra/oculta el botón "Continue" según si hay partida guardada.
/// - "Nueva partida": borra progreso (vía SaveManager) y carga el nivel tutorial.
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

    private bool hasPlayedBefore = false;

    private void Awake()
    {
        // DOTween inicializado en AppBootstrap
    }

    private void Start()
    {
        hasPlayedBefore = SaveManager.Instance != null
            ? SaveManager.Instance.HasSaveData()
            : PlayerPrefs.HasKey("Level_1");

        continueButton.SetActive(hasPlayedBefore);
        SelectButton(hasPlayedBefore ? continueFirstButton : newGameFirstButton);

        panelBG.alpha = 0;
        panelBG.interactable = false;
        panelBG.blocksRaycasts = false;
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
        panelOptions.transform
            .DOLocalMoveY(initPoint, 0.5f)
            .SetEase(initCurve)
            .OnComplete(TurnOffPanel);
    }

    /// <summary>
    /// Inicia el nivel 0 (tutorial) borrando el progreso anterior.
    /// </summary>
    public void ToNextScene() => StartNewGame();

    public void ContinueScene() => SceneUtils.ToSelectionScene();

    public void QuitGame() => Application.Quit();

    // ───────────────────────────────────────────────
    //  Helpers privados
    // ───────────────────────────────────────────────

    private void StartNewGame()
    {
        if (SaveManager.Instance != null)
            SaveManager.Instance.ResetProgress();

        SceneUtils.LoadGameplayScene(0);
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
