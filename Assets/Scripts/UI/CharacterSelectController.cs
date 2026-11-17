using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

/// <summary>
/// Character selector: browse every style and color, preview the animated
/// result, and keep the choice when both options are unlocked. All visuals are
/// serialized in the scene; this component only fills them in.
/// </summary>
public class CharacterSelectController : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private CanvasGroup panelGroup;
    [SerializeField] private RectTransform window;
    [Tooltip("Optional dedicated entry; Settings can also open the panel.")]
    [SerializeField] private Button openButton;
    [Tooltip("Settings panel to return to when the selector closes.")]
    [SerializeField] private SettingsController returnToSettings;
    [SerializeField] private Button closeButton;

    [Header("Preview")]
    [SerializeField] private Image preview;
    [Tooltip("Original idle frames; the preview shows the browsed skin's copy of each.")]
    [SerializeField] private Sprite[] idleFrames;
    [SerializeField, Min(1f)] private float previewFrameRate = 6f;
    [SerializeField] private Color lockedTint = new Color(0.08f, 0.1f, 0.2f, 1f);

    [Header("Rows")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI styleText;
    [SerializeField] private TextMeshProUGUI colorText;
    [Tooltip("Perfect-gem levels (style unlocks), shown as gem icon + amount.")]
    [SerializeField] private TextMeshProUGUI perfectLevelsText;
    [Tooltip("Level clovers (color unlocks), shown as clover icon + amount.")]
    [SerializeField] private TextMeshProUGUI cloversText;
    [SerializeField] private Color amountColor = new Color(0.91f, 0.93f, 1f);
    [SerializeField] private Color requirementColor = new Color(1f, 0.82f, 0.3f);
    [SerializeField] private TextMeshProUGUI styleCaptionText;
    [SerializeField] private TextMeshProUGUI colorCaptionText;
    [SerializeField] private TextMeshProUGUI openButtonText;
    [SerializeField] private TextMeshProUGUI closeButtonText;
    [SerializeField] private Button stylePreviousButton;
    [SerializeField] private Button styleNextButton;
    [SerializeField] private Button colorPreviousButton;
    [SerializeField] private Button colorNextButton;

    private int browsedStyle;
    private int browsedColor;
    private float frameTimer;
    private int frameIndex;
    private bool isOpen;
    private Vector3 windowScale = Vector3.one;

    private void Awake()
    {
        if (window != null)
            windowScale = window.localScale;

        Bind(openButton, Open);
        Bind(closeButton, Close);
        Bind(stylePreviousButton, () => StepStyle(-1));
        Bind(styleNextButton, () => StepStyle(1));
        Bind(colorPreviousButton, () => StepColor(-1));
        Bind(colorNextButton, () => StepColor(1));
        SetVisible(false);
    }

    private void OnEnable()
    {
        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
        Refresh();
    }

    private void OnDisable()
    {
        LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
    }

    private void OnLocaleChanged(UnityEngine.Localization.Locale locale) => Refresh();

    private static void Bind(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button != null)
            button.onClick.AddListener(action);
    }

    public void Open()
    {
        browsedStyle = PlayerSkins.StyleIndex;
        browsedColor = PlayerSkins.ColorIndex;
        isOpen = true;
        SetVisible(true);
        Refresh();

        if (window != null)
        {
            window.DOKill();
            window.localScale = windowScale * 0.85f;
            window.DOScale(windowScale, 0.25f).SetEase(Ease.OutBack).SetUpdate(true).SetLink(window.gameObject);
        }

        UISelectionUtility.SelectDefault(styleNextButton != null ? styleNextButton.gameObject : null, transform);
    }

    public void Close()
    {
        isOpen = false;
        SetVisible(false);
        if (returnToSettings != null)
            returnToSettings.OpenSettings();
        else
            UISelectionUtility.SelectDefault(openButton != null ? openButton.gameObject : null);
    }

    private void SetVisible(bool visible)
    {
        if (panelGroup == null) return;
        panelGroup.alpha = visible ? 1f : 0f;
        panelGroup.interactable = visible;
        panelGroup.blocksRaycasts = visible;
    }

    private void StepStyle(int direction)
    {
        browsedStyle = (browsedStyle + direction + PlayerSkins.Styles.Length) % PlayerSkins.Styles.Length;
        ApplyBrowsed();
    }

    private void StepColor(int direction)
    {
        browsedColor = (browsedColor + direction + PlayerSkins.Colors.Length) % PlayerSkins.Colors.Length;
        ApplyBrowsed();
    }

    private void ApplyBrowsed()
    {
        // Locked options can be previewed, but only an unlocked pair is kept.
        PlayerSkins.TrySelect(browsedStyle, browsedColor);
        Refresh();

        if (preview != null)
        {
            preview.transform.DOKill(true);
            preview.transform.DOPunchScale(Vector3.one * 0.12f, 0.2f, 6, 0.6f).SetUpdate(true)
                .SetLink(preview.gameObject);
        }
    }

    private void Update()
    {
        if (!isOpen || preview == null || idleFrames == null || idleFrames.Length == 0)
            return;

        frameTimer += Time.unscaledDeltaTime;
        float frameDuration = 1f / previewFrameRate;
        if (frameTimer < frameDuration)
            return;

        frameTimer -= frameDuration;
        frameIndex = (frameIndex + 1) % idleFrames.Length;
        ShowFrame();
    }

    private void ShowFrame()
    {
        if (preview == null || idleFrames == null || idleFrames.Length == 0)
            return;

        Sprite original = idleFrames[frameIndex % idleFrames.Length];
        preview.sprite = PlayerSkins.ResolveFor(original, browsedStyle, browsedColor);
    }

    private void Refresh()
    {
        bool styleUnlocked = PlayerSkins.IsStyleUnlocked(browsedStyle);
        bool colorUnlocked = PlayerSkins.IsColorUnlocked(browsedColor);

        SetText(titleText, GameLocalization.Get("UI_CHARACTER"));
        SetText(openButtonText, GameLocalization.Get("UI_CHARACTER"));
        SetText(closeButtonText, GameLocalization.Get("UI_CLOSE_TITLE"));
        SetText(styleCaptionText, GameLocalization.Get("SKIN_ROW_STYLE"));
        SetText(colorCaptionText, GameLocalization.Get("SKIN_ROW_COLOR"));
        SetText(styleText, GameLocalization.Get("SKIN_STYLE_" + PlayerSkins.Styles[browsedStyle].ToUpperInvariant()));
        SetText(colorText, GameLocalization.Get("SKIN_COLOR_" + PlayerSkins.Colors[browsedColor].ToUpperInvariant()));

        // Progress as icon + amount; a locked option shows the amount it needs.
        int perfect = SaveManager.Instance != null ? SaveManager.Instance.CountPerfectGemLevels() : 0;
        int clovers = SaveManager.Instance != null ? SaveManager.Instance.CountLevelClovers() : 0;
        SetAmount(perfectLevelsText, perfect, styleUnlocked ? -1 : PlayerSkins.StyleRequirement(browsedStyle));
        SetAmount(cloversText, clovers, colorUnlocked ? -1 : PlayerSkins.ColorRequirement(browsedColor));

        if (preview != null)
            preview.color = styleUnlocked && colorUnlocked ? Color.white : lockedTint;

        ShowFrame();
    }

    private void SetAmount(TextMeshProUGUI target, int amount, int requirement)
    {
        if (target == null) return;
        target.text = requirement < 0 ? $"x{amount}" : $"{amount}/{requirement}";
        target.color = requirement < 0 ? amountColor : requirementColor;
    }

    private static void SetText(TextMeshProUGUI target, string value)
    {
        if (target != null)
            target.text = value;
    }
}
