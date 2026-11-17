using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public enum DiceType
{
    Normal,         // 1 a 3 vidas
    Upgraded_3_5,   // 3 a 5 vidas
    Upgraded_2_6,   // 2 a 6 vidas
    Secret          // 4 a 6 vidas (desbloqueado con fragmentos)
}

/// <summary>
/// Controlador de la pantalla del dado de vidas.
/// Se activa al completar un nivel con el 100% de gemas (10/10) o desde Game Over al gastar un trébol.
/// </summary>
public class DiceRollController : MonoBehaviour
{
    public static DiceRollController Instance { get; private set; }

    [Header("Panel")]
    [SerializeField] private CanvasGroup panelGroup;
    [SerializeField] private RectTransform panelRect;

    [Header("UI Elementos")]
    [SerializeField] private TextMeshProUGUI diceNumberText;
    [SerializeField] private TextMeshProUGUI diceStatusText;
    [SerializeField] private TextMeshProUGUI trebolAvailableText;
    [SerializeField] private Button rollButton;
    [SerializeField] private Button stopButton;
    [SerializeField] private Button upgradeWithTrebolButton;
    [SerializeField] private Button closeButton;

    [Header("Visual del Dado (Caras)")]
    [Tooltip("Componente Image opcional para mostrar la cara gráfica del dado")]
    [SerializeField] private Image diceFaceImage;
    [Tooltip("Sprites para las 6 caras del dado (índice 0 = cara 1, ..., índice 5 = cara 6)")]
    [SerializeField] private Sprite[] diceFaceSprites;
    [SerializeField] private Image cloverIcon;

    [Header("Configuración de Animación")]
    [SerializeField] private float rollDuration = 2.0f;
    [SerializeField] private float cycleInterval = 0.08f;
    [SerializeField] private float hiddenY = -800f;
    [SerializeField] private float visibleY = 0f;

    public event Action<int> OnRollComplete;

    private DiceType currentDiceType = DiceType.Normal;
    private bool isRolling = false;
    private bool hasRolledThisPresentation;
    private Coroutine rollRoutine;
    private int lastRollResult = 1;
    private Tween diceRollTween;
    private Vector3 diceRestScale;
    private Vector3 diceRestRotation;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (panelGroup != null)
        {
            panelGroup.alpha = 0f;
            panelGroup.interactable = false;
            panelGroup.blocksRaycasts = false;
        }
        if (panelRect != null)
        {
            panelRect.anchoredPosition = new Vector2(panelRect.anchoredPosition.x, hiddenY);
        }

        ConfigureAllTextsAutoResize();
    }

    private void ConfigureAllTextsAutoResize()
    {
        ConfigureAutoResize(diceNumberText, 32f, 120f);
        ConfigureAutoResize(diceStatusText, 18f, 40f);
        ConfigureAutoResize(trebolAvailableText, 18f, 44f);
        ConfigureDisabledStateVisual(rollButton);
        ConfigureDisabledStateVisual(stopButton);
        ConfigureDisabledStateVisual(upgradeWithTrebolButton);
        ConfigureDisabledStateVisual(closeButton);
    }

    private static void ConfigureDisabledStateVisual(Button button)
    {
        if (button == null) return;

        ColorBlock colors = button.colors;
        colors.disabledColor = new Color(0.55f, 0.55f, 0.55f, 0.45f);
        colors.colorMultiplier = 1f;
        button.colors = colors;
        button.transition = Selectable.Transition.ColorTint;
    }

    private static void ConfigureAutoResize(TextMeshProUGUI tmp, float minSize, float maxSize)
    {
        if (tmp == null) return;
        tmp.enableAutoSizing = true;
        tmp.fontSizeMin = minSize;
        tmp.fontSizeMax = maxSize;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;
        tmp.overflowMode = TextOverflowModes.Ellipsis;
    }

    private void Start()
    {
        if (rollButton != null) rollButton.onClick.AddListener(OnRollPressed);
        if (stopButton != null) stopButton.onClick.AddListener(OnStopPressed);
        if (upgradeWithTrebolButton != null) upgradeWithTrebolButton.onClick.AddListener(OnUpgradeWithTrebolPressed);
        if (closeButton != null) closeButton.onClick.AddListener(OnClosePressed);
    }

    private void OnDestroy()
    {
        diceRollTween?.Kill();
    }

    private System.Action<int> onGameOverComplete;
    private System.Action onLevelCompleteContinue;

    public void Show(DiceType startingType = DiceType.Normal)
    {
        onLevelCompleteContinue = null;
        ShowInternal(startingType, null);
    }

    /// <summary>Muestra el dado al completar nivel y ejecuta el siguiente flujo al cerrarlo.</summary>
    public void ShowFromLevelComplete(DiceType startingType, System.Action onContinue)
    {
        onLevelCompleteContinue = onContinue;
        ShowInternal(startingType, null);
    }

    /// <summary>
    /// Muestra el dado desde el Game Over. Al cerrarse, llama onComplete con las vidas ganadas
    /// y respawnea al jugador en lugar de ir al mapa.
    /// </summary>
    public void ShowFromGameOver(DiceType startingType, System.Action<int> onComplete)
    {
        onLevelCompleteContinue = null;
        ShowInternal(startingType, onComplete);
    }

    private void ShowInternal(DiceType startingType, System.Action<int> onComplete)
    {
        currentDiceType   = startingType;
        isRolling         = false;
        hasRolledThisPresentation = false;
        onGameOverComplete = onComplete;

        RefreshUI();

        PlatformDetector.Instance?.SetGameplayControlsSuppressed(true);

        if (panelGroup != null)
        {
            panelGroup.gameObject.SetActive(true);
            panelGroup.alpha = 1f;
            panelGroup.interactable = true;
            panelGroup.blocksRaycasts = true;
        }

        if (panelRect != null)
        {
            panelRect.DOKill();
            panelRect.DOAnchorPosY(visibleY, 0.4f).SetEase(Ease.OutBack).SetUpdate(true);
        }

        if (rollButton != null) rollButton.interactable = true;
        if (stopButton != null) stopButton.interactable = false;
        if (upgradeWithTrebolButton != null)
            upgradeWithTrebolButton.interactable = SaveManager.Instance != null &&
                SaveManager.Instance.LoadTotalTrebols() > 0 && startingType == DiceType.Normal;
        UISelectionUtility.SelectDefault(rollButton != null ? rollButton.gameObject : null, panelGroup != null ? panelGroup.transform : transform);
    }

    public void Hide(Action onHidden = null)
    {
        if (panelRect != null)
        {
            panelRect.DOKill();
            panelRect.DOAnchorPosY(hiddenY, 0.3f).SetEase(Ease.InBack).SetUpdate(true)
                     .OnComplete(() =>
                     {
                         if (panelGroup != null)
                         {
                             panelGroup.alpha = 0f;
                             panelGroup.interactable = false;
                             panelGroup.blocksRaycasts = false;
                         }
                         CompleteHide(onHidden);
                     });
        }
        else if (panelGroup != null)
        {
            panelGroup.alpha = 0f;
            panelGroup.interactable = false;
            panelGroup.blocksRaycasts = false;
            CompleteHide(onHidden);
        }
        else
        {
            CompleteHide(onHidden);
        }
    }

    private void CompleteHide(Action onHidden)
    {
        PlatformDetector.Instance?.SetGameplayControlsSuppressed(false);
        onHidden?.Invoke();
    }

    public void OnRollPressed()
    {
        if (isRolling || hasRolledThisPresentation) return;
        hasRolledThisPresentation = true;
        rollRoutine = StartCoroutine(RollSequence());
    }

    public void OnStopPressed()
    {
        if (!isRolling) return;
        isRolling = false; // Detiene el bucle en RollSequence
    }

    public void OnUpgradeWithTrebolPressed()
    {
        if (isRolling || hasRolledThisPresentation) return;
        if (SaveManager.Instance == null) return;

        if (SaveManager.Instance.SpendTrebol())
        {
            currentDiceType = UnityEngine.Random.value > 0.5f ? DiceType.Upgraded_3_5 : DiceType.Upgraded_2_6;
            if (upgradeWithTrebolButton != null) upgradeWithTrebolButton.interactable = false;
            StartCoroutine(PlayUpgradeSequence());
        }
        else
        {
            if (upgradeWithTrebolButton != null)
                upgradeWithTrebolButton.transform.DOShakePosition(0.3f, 8f, 20).SetUpdate(true);
        }
    }

    /// <summary>
    /// Full visual sequence for spending a Clover to upgrade the dice:
    /// 1. Clover icon flies from the HUD counter to the dice face.
    /// 2. Dice flashes white and punches scale on arrival.
    /// 3. Panel title and status text update to show the new range.
    /// 4. Brief screen shake (subtle) caps the effect.
    /// </summary>
    private IEnumerator PlayUpgradeSequence()
    {
        AudioMixerManager._instance?.CallSFX(SFXType.Player_Pickup_Health);

        // ── 1. Fly a clover token from the counter to the dice ──────────────
        Transform diceVisual = GetDiceVisualTransform();
        RectTransform cloverSource = cloverIcon != null ? cloverIcon.rectTransform : null;

        if (cloverSource != null && diceVisual != null && panelGroup != null)
        {
            Canvas canvas = panelGroup.GetComponentInParent<Canvas>();
            RectTransform canvasRect = canvas != null ? canvas.transform as RectTransform : null;

            if (canvasRect != null)
            {
                // Determine canvas-space positions
                Camera uiCam = canvas.renderMode == RenderMode.ScreenSpaceOverlay
                    ? null
                    : (canvas.worldCamera != null ? canvas.worldCamera : Camera.main);

                Vector2 startPos = GetCanvasLocalPos(cloverSource, canvasRect, uiCam);
                Vector2 endPos   = GetCanvasLocalPos(diceVisual as RectTransform, canvasRect, uiCam);

                // Build the token
                GameObject token = new GameObject("CloverFlight",
                    typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                token.transform.SetParent(canvasRect, false);
                Image tokenImg = token.GetComponent<Image>();
                tokenImg.sprite = cloverIcon.sprite;
                tokenImg.color  = Color.white;
                tokenImg.preserveAspect = true;
                tokenImg.raycastTarget  = false;

                RectTransform tokenRect = token.GetComponent<RectTransform>();
                tokenRect.sizeDelta = new Vector2(48f, 48f);
                tokenRect.anchoredPosition = startPos;

                // Animate: arc toward the dice over 0.55 s with overshoot
                bool arrived = false;
                float elapsed = 0f;
                const float duration = 0.55f;

                while (elapsed < duration)
                {
                    elapsed += Time.unscaledDeltaTime;
                    float t = Mathf.Clamp01(elapsed / duration);
                    // Ease out cubic
                    float eased = 1f - Mathf.Pow(1f - t, 3f);

                    // Recalculate end each frame in case layout shifted
                    Vector2 currentEnd = GetCanvasLocalPos(diceVisual as RectTransform, canvasRect, uiCam);

                    // Arc: lift the token upward at mid-flight
                    float arc = Mathf.Sin(t * Mathf.PI) * 60f;
                    Vector2 pos = Vector2.LerpUnclamped(startPos, currentEnd, eased);
                    pos.y += arc;
                    tokenRect.anchoredPosition = pos;

                    // Scale: starts at 1, shrinks to 0.5 on arrival
                    float scale = Mathf.Lerp(1f, 0.5f, eased);
                    tokenRect.localScale = Vector3.one * scale;

                    if (!arrived && t >= 0.95f) arrived = true;
                    yield return null;
                }

                UnityEngine.Object.Destroy(token);
            }
        }

        // ── 2. Impact: dice flashes and punches scale ────────────────────────
        if (diceVisual != null)
        {
            // White flash via DOColor if it's an Image, otherwise just punch
            Image diceImg = diceVisual.GetComponent<Image>();
            if (diceImg != null)
            {
                Color original = diceImg.color;
                DOTween.Sequence()
                    .Append(diceImg.DOColor(Color.white, 0.07f).SetUpdate(true))
                    .Append(diceImg.DOColor(original, 0.18f).SetUpdate(true));
            }

            diceVisual.DOKill();
            diceVisual.DOPunchScale(Vector3.one * 0.45f, 0.4f, 10, 0.6f).SetUpdate(true);
        }

        // Also punch the whole panel to feel the impact
        if (panelRect != null)
        {
            panelRect.DOKill();
            panelRect.DOPunchScale(Vector3.one * 0.04f, 0.3f, 6, 0.5f).SetUpdate(true);
        }

        // ── 3. Brief pause so the dice punch is visible ──────────────────────
        yield return new WaitForSecondsRealtime(0.15f);

        // ── 4. Update status text with a pop ────────────────────────────────
        RefreshUI();
        if (diceStatusText != null)
        {
            diceStatusText.transform.DOKill();
            diceStatusText.transform.DOPunchScale(Vector3.one * 0.25f, 0.3f, 7, 0.6f).SetUpdate(true);
        }

        // ── 5. Update the clover counter with a shake to show it was spent ──
        if (cloverIcon != null)
        {
            cloverIcon.transform.DOKill();
            cloverIcon.transform.DOShakeScale(0.4f, 0.35f, 10, 90f).SetUpdate(true);
        }
        if (trebolAvailableText != null)
        {
            trebolAvailableText.transform.DOKill();
            trebolAvailableText.transform.DOPunchScale(Vector3.one * 0.2f, 0.25f, 6, 0.5f).SetUpdate(true);
        }

        // ── 6. Screen shake for extra punch ─────────────────────────────────
        ScreenShake._instance?.StartShake(0.2f, 0.04f);
    }

    private static Vector2 GetCanvasLocalPos(RectTransform target, RectTransform canvasRect, Camera uiCam)
    {
        if (target == null || canvasRect == null) return Vector2.zero;
        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(uiCam, target.position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect, screenPos, uiCam, out Vector2 local);
        return local;
    }

    public void OnClosePressed()
    {
        Action continueAfterHide = null;
        if (onLevelCompleteContinue != null)
        {
            var callback = onLevelCompleteContinue;
            onLevelCompleteContinue = null;
            continueAfterHide = callback;
        }
        else if (onGameOverComplete != null)
        {
            var cb = onGameOverComplete;
            onGameOverComplete = null;
            continueAfterHide = () => cb.Invoke(lastRollResult);
        }
        else
        {
            continueAfterHide = SceneUtils.ToSelectionScene;
        }
        Hide(continueAfterHide);
    }

    private void UpdateDiceVisual(int value)
    {
        bool hasSprite = diceFaceSprites != null && value >= 1 && value <= diceFaceSprites.Length && diceFaceSprites[value - 1] != null && diceFaceImage != null;
        if (hasSprite)
        {
            diceFaceImage.gameObject.SetActive(true);
            diceFaceImage.sprite = diceFaceSprites[value - 1];
            if (diceNumberText != null) diceNumberText.gameObject.SetActive(false);
        }
        else
        {
            if (diceFaceImage != null) diceFaceImage.gameObject.SetActive(false);
            if (diceNumberText != null)
            {
                diceNumberText.gameObject.SetActive(true);
                diceNumberText.text = value.ToString();
            }
        }
    }

    private IEnumerator RollSequence()
    {
        isRolling = true;
        if (rollButton != null) rollButton.interactable = false;
        if (stopButton != null) stopButton.interactable = true;
        if (upgradeWithTrebolButton != null) upgradeWithTrebolButton.interactable = false;

        float timer = 0f;
        int lastValue = GetRandomValueForType(currentDiceType);
        UpdateDiceVisual(lastValue);
        StartDiceRollFeedback();

        while (isRolling && timer < rollDuration)
        {
            lastValue = GetRandomValueForType(currentDiceType);
            UpdateDiceVisual(lastValue);

            yield return new WaitForSecondsRealtime(cycleInterval);
            timer += cycleInterval;
        }

        isRolling = false;
        int finalResult = GetRandomValueForType(currentDiceType);
        lastRollResult = finalResult;

        UpdateDiceVisual(finalResult);
        StopDiceRollFeedback();
        PlayDiceResultFeedback();

        // Otorgar vidas ganadas
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.AddLives(finalResult);
        }

        // Dado Secreto: además de vidas, otorga 1 corazón extra temporal (GDD sección 7)
        if (currentDiceType == DiceType.Secret && PlayerHealth._instance != null)
        {
            PlayerHealth._instance.AddBonusHeart();
            if (diceStatusText != null)
                diceStatusText.text = GameLocalization.Get("DICE_RESULT_SECRET", finalResult);
        }

        // Notificar cambio de vidas a LivesManager para que dispare OnLivesChanged
        if (LivesManager.Instance != null)
            LivesManager.Instance.NotifyLivesChanged();
        UIController._instance?.AnimateLifeRewardFeedback(finalResult, GetDiceVisualTransform());

        if (diceStatusText != null && currentDiceType != DiceType.Secret)
            diceStatusText.text = GameLocalization.Get("DICE_RESULT_LIVES", finalResult);

        if (stopButton != null) stopButton.interactable = false;
        if (closeButton != null) closeButton.interactable = true;
        if (rollButton != null) rollButton.interactable = false;
        if (upgradeWithTrebolButton != null) upgradeWithTrebolButton.interactable = false;
        UISelectionUtility.SelectDefault(closeButton != null ? closeButton.gameObject : null, panelGroup != null ? panelGroup.transform : transform);

        OnRollComplete?.Invoke(finalResult);
    }

    private int GetRandomValueForType(DiceType type)
    {
        switch (type)
        {
            case DiceType.Upgraded_3_5:
                return UnityEngine.Random.Range(3, 6); // 3, 4, 5
            case DiceType.Upgraded_2_6:
                return UnityEngine.Random.Range(2, 7); // 2, 3, 4, 5, 6
            case DiceType.Secret:
                return UnityEngine.Random.Range(4, 7); // 4, 5, 6
            case DiceType.Normal:
            default:
                return UnityEngine.Random.Range(1, 4); // 1, 2, 3
        }
    }

    private Transform GetDiceVisualTransform()
    {
        if (diceFaceImage != null && diceFaceImage.gameObject.activeSelf)
            return diceFaceImage.transform;
        if (diceNumberText != null && diceNumberText.gameObject.activeSelf)
            return diceNumberText.transform;
        return null;
    }

    private void StartDiceRollFeedback()
    {
        Transform visual = GetDiceVisualTransform();
        if (visual == null)
            return;

        diceRollTween?.Kill();
        diceRestScale = visual.localScale;
        diceRestRotation = visual.localEulerAngles;

        Sequence sequence = DOTween.Sequence()
            .Append(visual.DOScale(diceRestScale * 1.12f, 0.14f).SetEase(Ease.OutQuad))
            .Join(visual.DOLocalRotate(diceRestRotation + new Vector3(0f, 0f, 18f), 0.14f))
            .Append(visual.DOScale(diceRestScale * 0.92f, 0.14f).SetEase(Ease.InOutSine))
            .Join(visual.DOLocalRotate(diceRestRotation - new Vector3(0f, 0f, 18f), 0.14f))
            .Append(visual.DOScale(diceRestScale, 0.12f).SetEase(Ease.OutBack))
            .Join(visual.DOLocalRotate(diceRestRotation, 0.12f))
            .SetLoops(-1, LoopType.Restart)
            .SetUpdate(true)
            .SetLink(visual.gameObject, LinkBehaviour.KillOnDestroy);

        diceRollTween = sequence;
    }

    private void StopDiceRollFeedback()
    {
        diceRollTween?.Kill();
        diceRollTween = null;

        Transform visual = GetDiceVisualTransform();
        if (visual == null)
            return;

        visual.localScale = diceRestScale;
        visual.localEulerAngles = diceRestRotation;
    }

    private void PlayDiceResultFeedback()
    {
        Transform visual = GetDiceVisualTransform();
        if (visual == null)
            return;

        visual.DOKill();
        DOTween.Sequence()
            .Append(visual.DOScale(diceRestScale * 1.28f, 0.12f).SetEase(Ease.OutBack))
            .Append(visual.DOScale(diceRestScale, 0.24f).SetEase(Ease.OutBounce))
            .Join(visual.DOPunchRotation(new Vector3(0f, 0f, 24f), 0.36f, 8, 0.7f))
            .SetUpdate(true)
            .SetLink(visual.gameObject, LinkBehaviour.KillOnDestroy);
    }

    private void RefreshUI()
    {
        int trebols = SaveManager.Instance != null ? SaveManager.Instance.LoadTotalTrebols() : 0;

        if (trebolAvailableText != null)
            trebolAvailableText.text = $"x{trebols}";

        if (cloverIcon != null)
            cloverIcon.color = trebols > 0 ? Color.white : new Color(0.45f, 0.45f, 0.45f, 0.6f);

        if (upgradeWithTrebolButton != null)
            upgradeWithTrebolButton.interactable = trebols > 0 && currentDiceType == DiceType.Normal;

        if (diceStatusText != null)
        {
            switch (currentDiceType)
            {
                case DiceType.Upgraded_3_5:
                    diceStatusText.text = GameLocalization.Get("DICE_RANGE_3_5");
                    break;
                case DiceType.Upgraded_2_6:
                    diceStatusText.text = GameLocalization.Get("DICE_RANGE_2_6");
                    break;
                case DiceType.Secret:
                    diceStatusText.text = GameLocalization.Get("DICE_RANGE_SECRET");
                    break;
                default:
                    diceStatusText.text = GameLocalization.Get("DICE_RANGE_NORMAL");
                    break;
            }
        }
    }
}
