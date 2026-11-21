using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Localization.Settings;
using DG.Tweening;

/// <summary>
/// Panel de información del nivel en la pantalla de selección.
/// Muestra nombre, gemas y tiempo récord del nivel activo.
/// Incluye animación de slide in/out con DOTween.
/// </summary>
public class UI_SelectController : MonoBehaviour
{
    public static UI_SelectController _instance;

    [FormerlySerializedAs("levelInfo")]
    [SerializeField] private GameObject levelInfoContainer;

    [FormerlySerializedAs("levelName")]
    [SerializeField] private TextMeshProUGUI levelNameText;

    [Header("Gems")]
    [FormerlySerializedAs("gemsFound")]
    [SerializeField] private TextMeshProUGUI gemsFoundText;
    [FormerlySerializedAs("gemsTarget")]
    [SerializeField] private TextMeshProUGUI gemsTargetText;

    [Header("Time")]
    [FormerlySerializedAs("bestTime")]
    [SerializeField] private TextMeshProUGUI bestTimeText;
    [FormerlySerializedAs("timeTarget")]
    [SerializeField] private TextMeshProUGUI timeTargetText;

    [Header("Clover / Trebol")]
    [Tooltip("Indicador visual en el panel UI de si el trébol del nivel fue recolectado")]
    [SerializeField] private GameObject cloverIndicator;
    [SerializeField] private TextMeshProUGUI cloverText;
    [Tooltip("Amber shards found / placed; hidden until the level has been played and has shards.")]
    [SerializeField] private GameObject amberIndicator;
    [SerializeField] private TextMeshProUGUI amberText;

    [Header("Animación del panel")]
    [Tooltip("Posición Y oculta (fuera de pantalla, abajo)")]
    [SerializeField] private float hiddenY = -250f;
    [Tooltip("Posición Y visible (elevado para que no se corte en pantallas 16:9 como iPhone SE)")]
    [SerializeField] private float visibleY = 180f;
    [Tooltip("Duración de la animación")]
    [SerializeField] private float animDuration = 0.3f;
    [SerializeField] private Ease showEase = Ease.OutBack;
    [SerializeField] private Ease hideEase = Ease.InBack;

    [Header("Play")]
    [SerializeField] private UnityEngine.UI.Button playButton;
    [SerializeField] private TextMeshProUGUI playButtonText;
    [SerializeField] private LSPlayerController player;
    [SerializeField] private CanvasGroup panelGroup;

    private RectTransform panelRect;
    private MapPoint currentLevelInfo;

    // Fixed-size blocks of the panel (stats, Play) with their authored layout. They are
    // placed for a wide screen; narrower aspect ratios (16:9, iPad 4:3) shrink them to fit.
    private RectTransform[] fitBlocks;
    private Vector2[] fitBlockPositions;
    private float fitContentWidth;
    private float fittedPanelWidth = -1f;
    private const float FitMargin = 160f;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;

        if (levelInfoContainer == null) return;
        panelRect = levelInfoContainer.GetComponent<RectTransform>();

        // Asegura que el panel empieza oculto y fuera de pantalla
        if (panelRect != null)
        {
            Vector2 pos = panelRect.anchoredPosition;
            panelRect.anchoredPosition = new Vector2(pos.x, hiddenY);

            // Si el pivot es inferior (Y=0), elevar visibleY para garantizar que no se corte en ninguna resolucion
            if (panelRect.pivot.y == 0f && visibleY < 50f)
            {
                visibleY = 50f;
            }
        }

        // Configurar auto-sizing y no-wrap para que nunca hagan overflow
        ConfigureAutoResize(levelNameText, 16f, 48f);
        ConfigureAutoResize(gemsFoundText, 14f, 40f);
        ConfigureAutoResize(gemsTargetText, 14f, 40f);
        ConfigureAutoResize(bestTimeText, 14f, 40f);
        ConfigureAutoResize(timeTargetText, 14f, 40f);
        ConfigureAutoResize(cloverText, 14f, 40f);

        ClearLevelInfoPlaceholders();

        if (gemsTargetText != null)
            gemsTargetText.gameObject.SetActive(false);
        if (cloverText != null)
            cloverText.gameObject.SetActive(false);

        CacheFitBlocks();
        levelInfoContainer.SetActive(false);

        if (playButton != null && player != null)
            playButton.onClick.AddListener(player.HandleLevelSelect);
    }

    private void OnEnable()
    {
        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
    }

    private void OnDisable()
    {
        LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
    }

    private void OnLocaleChanged(UnityEngine.Localization.Locale locale)
    {
        if (currentLevelInfo != null)
            RenderLevelData(currentLevelInfo);
        if (playButtonText != null)
            playButtonText.text = GameLocalization.Get("UI_PLAY");
    }

    private void ClearLevelInfoPlaceholders()
    {
        if (levelNameText != null) levelNameText.text = string.Empty;
        if (gemsFoundText != null) gemsFoundText.text = string.Empty;
        if (gemsTargetText != null) gemsTargetText.text = string.Empty;
        if (bestTimeText != null) bestTimeText.text = string.Empty;
        if (timeTargetText != null) timeTargetText.text = string.Empty;
        if (cloverText != null) cloverText.text = string.Empty;
    }

    private void CacheFitBlocks()
    {
        if (panelRect == null) return;
        var blocks = new System.Collections.Generic.List<RectTransform>();
        float halfWidth = 0f;
        foreach (Transform child in panelRect)
        {
            // Stretched children (the title) already follow the panel width.
            if (child is not RectTransform rect || rect.anchorMin != rect.anchorMax) continue;
            blocks.Add(rect);
            halfWidth = Mathf.Max(halfWidth, Mathf.Abs(rect.anchoredPosition.x) + ContentHalfWidth(rect));
        }
        fitBlocks = blocks.ToArray();
        fitBlockPositions = new Vector2[fitBlocks.Length];
        for (int i = 0; i < fitBlocks.Length; i++) fitBlockPositions[i] = fitBlocks[i].anchoredPosition;
        fitContentWidth = halfWidth * 2f + FitMargin;
    }

    /// <summary>Half the width actually covered by a block, including children placed outside its rect.</summary>
    private static float ContentHalfWidth(RectTransform rect)
    {
        // Empty layout containers can be much wider than what they show; only drawn rects count.
        float half = rect.GetComponent<UnityEngine.UI.Graphic>() != null ? rect.rect.width * 0.5f : 0f;
        foreach (Transform child in rect)
        {
            if (child is not RectTransform c || !c.gameObject.activeSelf) continue;
            if (c.anchorMin != c.anchorMax) continue;
            Vector2 anchorOffset = (c.anchorMin - new Vector2(0.5f, 0.5f)) * rect.rect.size;
            half = Mathf.Max(half, Mathf.Abs(anchorOffset.x + c.anchoredPosition.x) + ContentHalfWidth(c));
        }
        return half;
    }

    private void LateUpdate()
    {
        // Refit when the panel width changes (resolution, window size) while it is shown.
        if (panelRect != null && levelInfoContainer.activeSelf &&
            !Mathf.Approximately(panelRect.rect.width, fittedPanelWidth))
            FitContentToPanel();
    }

    private void FitContentToPanel()
    {
        if (fitBlocks == null || fitContentWidth <= 0f) return;
        Canvas.ForceUpdateCanvases();
        fittedPanelWidth = panelRect.rect.width;
        float scale = Mathf.Min(1f, panelRect.rect.width / fitContentWidth);
        for (int i = 0; i < fitBlocks.Length; i++)
        {
            if (fitBlocks[i] == null) continue;
            fitBlocks[i].localScale = new Vector3(scale, scale, 1f);
            fitBlocks[i].anchoredPosition = new Vector2(fitBlockPositions[i].x * scale, fitBlockPositions[i].y);
        }
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

    public void RenderLevelData(MapPoint levelInfo)
    {
        if (levelInfo == null) return;
        currentLevelInfo = levelInfo;

        if (levelNameText != null)
            levelNameText.text = GameLocalization.GetLevelName(levelInfo.currentLevel, levelInfo.levelName);

        if (gemsFoundText != null)
            gemsFoundText.text = $"{levelInfo.gemsCollected} / {levelInfo.totalGems}";
        if (gemsTargetText != null)
        {
            gemsTargetText.text = string.Empty;
            gemsTargetText.gameObject.SetActive(false);
        }
        if (timeTargetText != null)
            timeTargetText.text = GameLocalization.Get("SELECT_TARGET",
                GameLocalization.Get("SELECT_TIME_SECONDS", levelInfo.targetTime.ToString("F1")));

        if (bestTimeText != null)
        {
            string best = levelInfo.bestTime >= 99999f
                ? GameLocalization.Get("SELECT_BEST_NONE")
                : GameLocalization.Get("SELECT_TIME_SECONDS", levelInfo.bestTime.ToString("F1"));
            bestTimeText.text = GameLocalization.Get("SELECT_BEST", best);
        }

        if (cloverIndicator != null)
        {
            cloverIndicator.SetActive(true);
            UnityEngine.UI.Image cloverImage = cloverIndicator.GetComponent<UnityEngine.UI.Image>();
            if (cloverImage != null)
                cloverImage.color = levelInfo.hasTrebol
                    ? Color.white
                    : new Color(0.48f, 0.48f, 0.48f, 0.9f);
        }
        if (cloverText != null)
            cloverText.gameObject.SetActive(false);

        int amberTotal = SaveManager.Instance != null ? SaveManager.Instance.LoadAmberTotal(levelInfo.currentLevel) : 0;
        if (amberIndicator != null) amberIndicator.SetActive(amberTotal > 0);
        if (amberText != null) amberText.gameObject.SetActive(amberTotal > 0);
        if (amberText != null && amberTotal > 0)
            amberText.text = $"{SaveManager.Instance.CountAmberShards(levelInfo.currentLevel)} / {amberTotal}";
    }

    /// <summary>
    /// Muestra u oculta el panel con animación de slide.
    /// </summary>
    public void Turn_On_Off(bool show)
    {
        if (panelRect == null)
        {
            levelInfoContainer.SetActive(show);
            return;
        }

        if (show)
        {
            // Rises from below while fading from transparent to solid.
            bool wasHidden = !levelInfoContainer.activeSelf;
            levelInfoContainer.SetActive(true);
            FitContentToPanel();
            panelRect.DOKill();
            if (wasHidden)
                panelRect.anchoredPosition = new Vector2(panelRect.anchoredPosition.x, hiddenY);
            panelRect.DOAnchorPosY(visibleY, animDuration)
                     .SetEase(showEase)
                     .SetUpdate(true);
            if (panelGroup != null)
            {
                panelGroup.DOKill();
                if (wasHidden) panelGroup.alpha = 0f;
                panelGroup.DOFade(1f, animDuration).SetUpdate(true);
            }
            if (playButtonText != null)
                playButtonText.text = GameLocalization.Get("UI_PLAY");
            if (playButton != null)
            {
                // Draws the eye to the button that confirms the selected level.
                playButton.transform.DOKill(true);
                playButton.transform.DOPunchScale(Vector3.one * 0.12f, 0.35f, 6, 0.6f)
                    .SetDelay(animDuration).SetUpdate(true).SetLink(playButton.gameObject);
            }
        }
        else
        {
            panelRect.DOKill();
            panelRect.DOAnchorPosY(hiddenY, animDuration)
                     .SetEase(hideEase)
                     .SetUpdate(true)
                     .OnComplete(() => levelInfoContainer.SetActive(false));
            if (panelGroup != null)
            {
                panelGroup.DOKill();
                panelGroup.DOFade(0f, animDuration).SetUpdate(true);
            }
        }
    }
}
