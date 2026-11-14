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
        ConfigureAutoResize(diceNumberText, 24f, 80f);
        ConfigureAutoResize(diceStatusText, 14f, 26f);
        ConfigureAutoResize(trebolAvailableText, 14f, 24f);
        ConfigureButtonAutoResize(rollButton, 14f, 28f);
        ConfigureButtonAutoResize(stopButton, 14f, 28f);
        ConfigureButtonAutoResize(upgradeWithTrebolButton, 14f, 28f);
        ConfigureButtonAutoResize(closeButton, 14f, 28f);
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

    private static void ConfigureButtonAutoResize(Button btn, float minSize, float maxSize)
    {
        if (btn == null) return;
        var tmp = btn.GetComponentInChildren<TextMeshProUGUI>();
        ConfigureAutoResize(tmp, minSize, maxSize);
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
            // El tipo de dado mejorado recibido es aleatorio (3-5 ó 2-6) según el GDD
            currentDiceType = UnityEngine.Random.value > 0.5f ? DiceType.Upgraded_3_5 : DiceType.Upgraded_2_6;

            if (AudioMixerManager._instance != null)
                AudioMixerManager._instance.CallSFX(SFXType.Player_Pickup_Health);

            RefreshUI();
        }
        else
        {
            if (upgradeWithTrebolButton != null)
                upgradeWithTrebolButton.transform.DOShakePosition(0.3f, 8f, 20).SetUpdate(true);
        }
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
                diceStatusText.text = $"+{finalResult} LIVES  +1 HEART";
        }

        // Notificar cambio de vidas a LivesManager para que dispare OnLivesChanged
        if (LivesManager.Instance != null)
            LivesManager.Instance.NotifyLivesChanged();
        UIController._instance?.AnimateLifeRewardFeedback(finalResult, GetDiceVisualTransform());

        if (diceStatusText != null && currentDiceType != DiceType.Secret)
            diceStatusText.text = $"+{finalResult} LIVES";

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
                    diceStatusText.text = "3–5 LIVES";
                    break;
                case DiceType.Upgraded_2_6:
                    diceStatusText.text = "2–6 LIVES";
                    break;
                case DiceType.Secret:
                    diceStatusText.text = "4–6 LIVES  + HEART";
                    break;
                default:
                    diceStatusText.text = "1–3 LIVES";
                    break;
            }
        }
    }
}
