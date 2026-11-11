using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
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

    [Header("Animación del panel")]
    [Tooltip("Posición Y oculta (fuera de pantalla, abajo)")]
    [SerializeField] private float hiddenY = -250f;
    [Tooltip("Posición Y visible (elevado para que no se corte en pantallas 16:9 como iPhone SE)")]
    [SerializeField] private float visibleY = 180f;
    [Tooltip("Duración de la animación")]
    [SerializeField] private float animDuration = 0.3f;
    [SerializeField] private Ease showEase = Ease.OutBack;
    [SerializeField] private Ease hideEase = Ease.InBack;

    private RectTransform panelRect;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;

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
        ConfigureAutoResize(levelNameText, 20f, 60f);
        ConfigureAutoResize(gemsFoundText, 14f, 40f);
        ConfigureAutoResize(gemsTargetText, 14f, 40f);
        ConfigureAutoResize(bestTimeText, 14f, 40f);
        ConfigureAutoResize(timeTargetText, 14f, 40f);
        ConfigureAutoResize(cloverText, 14f, 40f);

        // Permitir iniciar el nivel al tocar directamente la tarjeta en pantallas táctiles
        if (levelInfoContainer != null)
        {
            var btn = levelInfoContainer.GetComponent<UnityEngine.UI.Button>();
            if (btn == null)
            {
                btn = levelInfoContainer.AddComponent<UnityEngine.UI.Button>();
                btn.transition = UnityEngine.UI.Selectable.Transition.None;
            }
            btn.onClick.RemoveListener(OnLevelCardClicked);
            btn.onClick.AddListener(OnLevelCardClicked);
        }

        levelInfoContainer.SetActive(false);
    }

    private void OnLevelCardClicked()
    {
        LSPlayerController player = Object.FindFirstObjectByType<LSPlayerController>();
        if (player != null)
        {
            player.HandleLevelSelect();
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

        if (levelNameText != null) levelNameText.text = levelInfo.levelName;
        if (gemsFoundText != null) gemsFoundText.text = "FOUND: " + levelInfo.gemsCollected;
        if (gemsTargetText != null) gemsTargetText.text = "IN LEVEL: " + levelInfo.totalGems;
        if (timeTargetText != null) timeTargetText.text = "TARGET: " + levelInfo.targetTime.ToString("F1") + "s";

        if (bestTimeText != null)
        {
            bestTimeText.text = levelInfo.bestTime >= 99999f
                ? "Best: ----"
                : "Best: " + levelInfo.bestTime.ToString("F1") + "s";
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
            cloverText.text = levelInfo.hasTrebol ? "FOUND" : "--";
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
            levelInfoContainer.SetActive(true);
            panelRect.DOKill();
            panelRect.DOAnchorPosY(visibleY, animDuration)
                     .SetEase(showEase)
                     .SetUpdate(true);
        }
        else
        {
            panelRect.DOKill();
            panelRect.DOAnchorPosY(hiddenY, animDuration)
                     .SetEase(hideEase)
                     .SetUpdate(true)
                     .OnComplete(() => levelInfoContainer.SetActive(false));
        }
    }
}
