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

    [Header("Configuración de Animación")]
    [SerializeField] private float rollDuration = 2.0f;
    [SerializeField] private float cycleInterval = 0.08f;
    [SerializeField] private float hiddenY = -800f;
    [SerializeField] private float visibleY = 0f;

    public event Action<int> OnRollComplete;

    private DiceType currentDiceType = DiceType.Normal;
    private bool isRolling = false;
    private Coroutine rollRoutine;
    private int lastRollResult = 1;

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
    }

    private void Start()
    {
        if (rollButton != null) rollButton.onClick.AddListener(OnRollPressed);
        if (stopButton != null) stopButton.onClick.AddListener(OnStopPressed);
        if (upgradeWithTrebolButton != null) upgradeWithTrebolButton.onClick.AddListener(OnUpgradeWithTrebolPressed);
        if (closeButton != null) closeButton.onClick.AddListener(OnClosePressed);
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
        onGameOverComplete = onComplete;

        RefreshUI();

        if (panelGroup != null)
        {
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
    }

    public void Hide()
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
                     });
        }
        else if (panelGroup != null)
        {
            panelGroup.alpha = 0f;
            panelGroup.interactable = false;
            panelGroup.blocksRaycasts = false;
        }
    }

    public void OnRollPressed()
    {
        if (isRolling) return;
        rollRoutine = StartCoroutine(RollSequence());
    }

    public void OnStopPressed()
    {
        if (!isRolling) return;
        isRolling = false; // Detiene el bucle en RollSequence
    }

    public void OnUpgradeWithTrebolPressed()
    {
        if (isRolling) return;
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
        Hide();
        if (onLevelCompleteContinue != null)
        {
            var callback = onLevelCompleteContinue;
            onLevelCompleteContinue = null;
            callback.Invoke();
        }
        else if (onGameOverComplete != null)
        {
            var cb = onGameOverComplete;
            onGameOverComplete = null;
            cb.Invoke(lastRollResult);
        }
        else
        {
            SceneUtils.ToSelectionScene();
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

        while (isRolling && timer < rollDuration)
        {
            lastValue = GetRandomValueForType(currentDiceType);
            if (diceNumberText != null)
                diceNumberText.text = lastValue.ToString();

            yield return new WaitForSecondsRealtime(cycleInterval);
            timer += cycleInterval;
        }

        isRolling = false;
        int finalResult = GetRandomValueForType(currentDiceType);
        lastRollResult = finalResult;

        if (diceNumberText != null)
        {
            diceNumberText.text = finalResult.ToString();
            diceNumberText.transform.DOPunchScale(Vector3.one * 0.3f, 0.3f).SetUpdate(true);
        }

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
                diceStatusText.text = $"Ganaste +{finalResult} vidas y +1 corazon extra";
        }

        // Notificar cambio de vidas a LivesManager para que dispare OnLivesChanged
        if (LivesManager.Instance != null)
            LivesManager.Instance.NotifyLivesChanged();

        if (diceStatusText != null && currentDiceType != DiceType.Secret)
            diceStatusText.text = $"Ganaste +{finalResult} vidas";

        if (stopButton != null) stopButton.interactable = false;
        if (closeButton != null) closeButton.interactable = true;

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

    private void RefreshUI()
    {
        int trebols = SaveManager.Instance != null ? SaveManager.Instance.LoadTotalTrebols() : 0;

        if (trebolAvailableText != null)
            trebolAvailableText.text = $"Treboles: {trebols}";

        if (upgradeWithTrebolButton != null)
            upgradeWithTrebolButton.interactable = trebols > 0 && currentDiceType == DiceType.Normal;

        if (diceStatusText != null)
        {
            switch (currentDiceType)
            {
                case DiceType.Upgraded_3_5:
                    diceStatusText.text = "Dado Mejorado (3 - 5 Vidas)";
                    break;
                case DiceType.Upgraded_2_6:
                    diceStatusText.text = "Dado Mejorado (2 - 6 Vidas)";
                    break;
                case DiceType.Secret:
                    diceStatusText.text = "Dado Secreto (4 - 6 Vidas)";
                    break;
                default:
                    diceStatusText.text = "Dado Normal (1 - 3 Vidas)";
                    break;
            }
        }
    }
}
