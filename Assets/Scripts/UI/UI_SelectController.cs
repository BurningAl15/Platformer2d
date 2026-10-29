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

    [Header("Animación del panel")]
    [Tooltip("Posición Y oculta (fuera de pantalla, abajo)")]
    [SerializeField] private float hiddenY = -250f;
    [Tooltip("Posición Y visible (elevado para que no se corte en pantallas 16:9 como iPhone SE)")]
    [SerializeField] private float visibleY = 150f;
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
        }

        levelInfoContainer.SetActive(false);
    }

    public void RenderLevelData(MapPoint levelInfo)
    {
        levelNameText.text = levelInfo.levelName;
        gemsFoundText.text = "FOUND: " + levelInfo.gemsCollected;
        gemsTargetText.text = "IN LEVEL: " + levelInfo.totalGems;
        timeTargetText.text = "TARGET: " + levelInfo.targetTime.ToString("F1") + "s";

        bestTimeText.text = levelInfo.bestTime >= 99999f
            ? "Best: ----"
            : "Best: " + levelInfo.bestTime.ToString("F1") + "s";
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
