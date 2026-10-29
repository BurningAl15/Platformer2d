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
    [Tooltip("Botón opcional para usar el Dado Secreto si está desbloqueado")]
    [SerializeField] private Button useSecretDiceButton;
    [SerializeField] private Button returnToMapButton;
    [SerializeField] private GameObject firstSelectedButton;

    [Header("Textos")]
    [SerializeField] private TextMeshProUGUI gemCostText;
    [SerializeField] private TextMeshProUGUI currentGemsText;
    [SerializeField] private TextMeshProUGUI trebolAvailableText;
    [Tooltip("Feedback visible si el rewarded no está disponible.")]
    [SerializeField] private TextMeshProUGUI adFeedbackText;

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

    private void Start()
    {
        if (watchAdButton != null)
            watchAdButton.onClick.AddListener(OnWatchAdPressed);
        if (useTrebolButton != null)
            useTrebolButton.onClick.AddListener(OnUseTrebolPressed);
        if (useSecretDiceButton != null)
            useSecretDiceButton.onClick.AddListener(OnUseSecretDicePressed);
        if (spendGemsButton != null)
            spendGemsButton.onClick.AddListener(OnSpendGemsPressed);
        if (returnToMapButton != null)
            returnToMapButton.onClick.AddListener(OnReturnToMapPressed);
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
        if (adFeedbackText != null)
            adFeedbackText.text = string.Empty;
        if (watchAdButton != null)
            watchAdButton.gameObject.SetActive(AdsManager.Instance == null || !AdsManager.Instance.IsNoAdsActive());

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
    /// Botón "Ver Ad" — muestra rewarded ad a través de AdsManager.
    /// Solo concede la recompensa cuando el SDK confirma que el rewarded se completó.
    /// </summary>
    public void OnWatchAdPressed()
    {
        if (LivesManager.Instance == null) return;

        // Deshabilitar botón para evitar doble tap
        if (watchAdButton != null) watchAdButton.interactable = false;

        if (AdsManager.Instance != null && AdsManager.Instance.IsRewardedReady())
        {
            AdsManager.Instance.ShowRewarded(
                AdsManager.AdContext.GameOver,
                onSuccess: () =>
                {
                    // Éxito: el jugador vio el ad completo
                    LivesManager.Instance.AddLivesFromAd();
                    ContinueGame();
                },
                onFail: () =>
                {
                    // Fallo o cierre anticipado: restaurar botón
                    if (watchAdButton != null) watchAdButton.interactable = true;
                    ShowAdUnavailableFeedback();
                }
            );
        }
        else
        {
            // No conceder la recompensa si el SDK no pudo presentar y completar el anuncio.
            if (watchAdButton != null) watchAdButton.interactable = true;
            ShowAdUnavailableFeedback();
        }
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
    /// Botón "Gastar Trébol" — gasta 1 trébol, abre la pantalla del dado mejorado y continúa.
    /// </summary>
    public void OnUseTrebolPressed()
    {
        if (SaveManager.Instance == null) return;

        if (SaveManager.Instance.SpendTrebol())
        {
            // Ocultar el Game Over y pasar al DiceRollController con dado mejorado
            // El tipo es aleatorio entre Upgraded_3_5 y Upgraded_2_6 (GDD)
            DiceType upgradedType = UnityEngine.Random.value > 0.5f
                ? DiceType.Upgraded_3_5
                : DiceType.Upgraded_2_6;

            HideInstant();
            Time.timeScale = 1f;
            InputManager.Instance?.EnablePlayerInput();

            if (DiceRollController.Instance != null)
            {
                // El DiceRollController en modo Game Over otorga vidas y luego respawnea
                DiceRollController.Instance.ShowFromGameOver(upgradedType, OnDiceGameOverComplete);
            }
            else
            {
                // Fallback si no hay DiceRollController: dar vidas directamente
                int livesWon = UnityEngine.Random.value > 0.5f
                    ? UnityEngine.Random.Range(3, 6)
                    : UnityEngine.Random.Range(2, 7);
                SaveManager.Instance.AddLives(livesWon);
                ContinueGame();
            }
        }
        else
        {
            if (useTrebolButton != null)
                useTrebolButton.transform.DOShakePosition(0.3f, 8f, 20).SetUpdate(true);
            RefreshEconomyUI();
        }
    }

    /// <summary>
    /// Botón opcional "Dado Secreto" — tira el dado secreto (4-6 vidas) y continúa.
    /// </summary>
    public void OnUseSecretDicePressed()
    {
        if (SaveManager.Instance == null) return;
        if (!SaveManager.Instance.IsSecretDiceUnlocked(1)) return;

        HideInstant();
        Time.timeScale = 1f;
        InputManager.Instance?.EnablePlayerInput();

        if (DiceRollController.Instance != null)
        {
            DiceRollController.Instance.ShowFromGameOver(DiceType.Secret, OnDiceGameOverComplete);
        }
        else
        {
            SaveManager.Instance.AddLives(4);
            ContinueGame();
        }
    }

    private void OnDiceGameOverComplete(int livesWon)
    {
        ContinueGame();
    }

    /// <summary>
    /// Botón "Volver al mapa" — resetea vidas y carga selección.
    /// Muestra un interstitial si las condiciones se cumplen (no en primeros 2 min).
    /// </summary>
    public void OnReturnToMapPressed()
    {
        if (SaveManager.Instance != null)
            SaveManager.Instance.SaveLives(SaveManager.Instance.GetDefaultLives());

        // Resetear corazones bonus al salir al mapa (son temporales por sesión)
        if (PlayerHealth._instance != null)
            PlayerHealth._instance.ResetBonusHearts();

        Time.timeScale = 1f;
        InputManager.Instance?.EnablePlayerInput();
        HideInstant();

        // Interstitial al volver al mapa (controlado por AdsManager — no muestra si es muy pronto)
        AdsManager.Instance?.ShowInterstitial(AdsManager.AdContext.ReturnToMap);

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

        // Mostrar u ocultar botón de dado secreto según si está desbloqueado
        if (useSecretDiceButton != null)
        {
            bool secretUnlocked = SaveManager.Instance.IsSecretDiceUnlocked(1);
            useSecretDiceButton.gameObject.SetActive(secretUnlocked);
        }
    }

    private void ShowAdUnavailableFeedback()
    {
        const string message = "Anuncio no disponible. Inténtalo de nuevo más tarde.";
        if (adFeedbackText != null)
            adFeedbackText.text = message;
        else
            Debug.LogWarning($"[GameOverController] {message}");
    }

    private void SelectFirstButton()
    {
        if (firstSelectedButton == null || EventSystem.current == null) return;
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(firstSelectedButton);
    }
}
