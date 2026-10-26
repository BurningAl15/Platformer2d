using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using UnityEngine.EventSystems;

/// <summary>
/// Pantalla de Game Over. Se activa cuando el jugador pierde todas sus vidas.
///
/// OPCIONES AL JUGADOR:
/// 1. Ver un Rewarded Ad → ganar vidas y continuar.
/// 2. Gastar gemas globales → comprar 1 vida y continuar.
/// 3. Volver al mapa de selección.
///
/// SETUP EN UNITY EDITOR:
/// - Agregar este script a un CanvasGroup en la escena de gameplay.
/// - Asignar los botones y textos en el Inspector.
/// - El CanvasGroup debe empezar con alpha = 0, interactable = false.
/// - Se suscribe a LivesManager.OnGameOver automáticamente.
/// </summary>
public class GameOverController : MonoBehaviour
{
    public static GameOverController _instance;

    [Header("Panel")]
    [SerializeField] private CanvasGroup panelGroup;
    [SerializeField] private RectTransform panelRect;

    [Header("Botones")]
    [SerializeField] private Button watchAdButton;
    [SerializeField] private Button spendGemsButton;
    [SerializeField] private Button useTrebolButton;
    [SerializeField] private Button returnToMapButton;
    [SerializeField] private GameObject firstSelectedButton;

    [Header("Textos")]
    [SerializeField] private TextMeshProUGUI gemCostText;
    [SerializeField] private TextMeshProUGUI currentGemsText;
    [SerializeField] private TextMeshProUGUI trebolAvailableText;

    [Header("Animación")]
    [SerializeField] private float animDuration = 0.4f;
    [SerializeField] private float hiddenY = -600f;
    [SerializeField] private float visibleY = 0f;

    // ─────────────────────────────────────────────────────────────────────────
    //  Ciclo de vida
    // ─────────────────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (_instance != null && _instance != this) { Destroy(gameObject); return; }
        _instance = this;
        HideInstant();
    }

    private void OnEnable()
    {
        if (LivesManager.Instance != null)
            LivesManager.Instance.OnGameOver += Show;
    }

    private void OnDisable()
    {
        if (LivesManager.Instance != null)
            LivesManager.Instance.OnGameOver -= Show;
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Mostrar / ocultar
    // ─────────────────────────────────────────────────────────────────────────

    public void Show()
    {
        // Pausar el juego
        Time.timeScale = 0f;
        InputManager.Instance?.EnableUIOnlyInput();

        // Actualizar textos de economía
        RefreshEconomyUI();

        // Mostrar panel con animación
        panelGroup.interactable  = true;
        panelGroup.blocksRaycasts = true;
        panelGroup.alpha = 1f;

        if (panelRect != null)
        {
            panelRect.anchoredPosition = new Vector2(panelRect.anchoredPosition.x, hiddenY);
            panelRect.DOKill();
            panelRect.DOAnchorPosY(visibleY, animDuration)
                     .SetEase(Ease.OutBack)
                     .SetUpdate(true)
                     .OnComplete(SelectFirstButton);
        }
        else
        {
            SelectFirstButton();
        }
    }

    private void Hide()
    {
        if (panelRect != null)
        {
            panelRect.DOKill();
            panelRect.DOAnchorPosY(hiddenY, animDuration)
                     .SetEase(Ease.InBack)
                     .SetUpdate(true)
                     .OnComplete(HideInstant);
        }
        else
        {
            HideInstant();
        }
    }

    private void HideInstant()
    {
        panelGroup.interactable   = false;
        panelGroup.blocksRaycasts = false;
        panelGroup.alpha = 0f;
        if (panelRect != null)
            panelRect.anchoredPosition = new Vector2(panelRect.anchoredPosition.x, hiddenY);
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Acciones de botones
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Botón "Ver Ad" — otorga vidas y continúa el juego.
    /// La integración real del ad se hace en Fase 4.
    /// Por ahora simula el rewarded directamente.
    /// </summary>
    public void OnWatchAdPressed()
    {
        if (LivesManager.Instance == null) return;

        // TODO (Fase 4): Reemplazar con llamada real al SDK de ads.
        // Por ahora simulamos la recompensa directamente.
        LivesManager.Instance.AddLivesFromAd();
        ContinueGame();
    }

    /// <summary>
    /// Botón "Usar gemas" — gasta gemas del wallet y continúa si hay suficientes.
    /// </summary>
    public void OnSpendGemsPressed()
    {
        if (LivesManager.Instance == null) return;

        bool success = LivesManager.Instance.SpendGemsForLife();
        if (success)
        {
            ContinueGame();
        }
        else
        {
            // No hay gemas suficientes: shake del botón como feedback
            if (spendGemsButton != null)
                spendGemsButton.transform.DOShakePosition(0.3f, 8f, 20).SetUpdate(true);
            RefreshEconomyUI();
        }
    }

    /// <summary>
    /// Botón "Gastar Trébol" — gasta 1 trébol, lanza dado mejorado (2-6 o 3-5) y continúa.
    /// </summary>
    public void OnUseTrebolPressed()
    {
        if (SaveManager.Instance == null) return;

        if (SaveManager.Instance.SpendTrebol())
        {
            // Tirada de dado mejorado (3-5 ó 2-6) según el GDD
            int livesWon = UnityEngine.Random.value > 0.5f
                ? UnityEngine.Random.Range(3, 6)
                : UnityEngine.Random.Range(2, 7);

            SaveManager.Instance.AddLives(livesWon);

            if (AudioMixerManager._instance != null)
                AudioMixerManager._instance.CallSFX(SFXType.Player_Pickup_Health);

            ContinueGame();
        }
        else
        {
            if (useTrebolButton != null)
                useTrebolButton.transform.DOShakePosition(0.3f, 8f, 20).SetUpdate(true);
            RefreshEconomyUI();
        }
    }

    /// <summary>
    /// Botón "Volver al mapa" — resetea vidas para siguiente sesión y carga selección.
    /// </summary>
    public void OnReturnToMapPressed()
    {
        // Restaurar las vidas al valor por defecto para que el jugador pueda volver a jugar
        if (SaveManager.Instance != null)
            SaveManager.Instance.SaveLives(SaveManager.Instance.GetDefaultLives());

        Time.timeScale = 1f;
        InputManager.Instance?.EnablePlayerInput();
        HideInstant();
        SceneUtils.ToSelectionScene();
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Helpers privados
    // ─────────────────────────────────────────────────────────────────────────

    private void ContinueGame()
    {
        Hide();
        Time.timeScale = 1f;
        InputManager.Instance?.EnablePlayerInput();

        // Respawnear al jugador con las vidas actualizadas
        if (LevelManager._instance != null)
            LevelManager._instance.RespawnPlayer();
    }

    private void RefreshEconomyUI()
    {
        if (SaveManager.Instance == null) return;

        int cost     = LivesManager.Instance?.GetGemCostPerLife() ?? 10;
        int gems     = SaveManager.Instance.LoadGlobalGems();
        int trebols  = SaveManager.Instance.LoadTotalTrebols();
        bool canGems = LivesManager.Instance?.CanAffordLife() ?? false;

        if (gemCostText != null)
            gemCostText.text = $"{cost} 💎";

        if (currentGemsText != null)
            currentGemsText.text = $"Tienes: {gems} 💎";

        if (trebolAvailableText != null)
            trebolAvailableText.text = $"Tréboles: {trebols} 🍀";

        // Deshabilitar botón de gemas si no alcanza
        if (spendGemsButton != null)
            spendGemsButton.interactable = canGems;

        // Deshabilitar botón de trébol si no tiene
        if (useTrebolButton != null)
            useTrebolButton.interactable = trebols > 0;
    }

    private void SelectFirstButton()
    {
        if (firstSelectedButton == null || EventSystem.current == null) return;
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(firstSelectedButton);
    }
}
