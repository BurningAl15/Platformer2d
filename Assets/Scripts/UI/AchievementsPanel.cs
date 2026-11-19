using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

/// <summary>
/// Lists every achievement as a card: icon, name, what earns it and, for
/// collection goals, the current count. Locked cards are dimmed. All visuals
/// are serialized in the prefab; this component only fills them in.
/// </summary>
public class AchievementsPanel : MonoBehaviour
{
    [System.Serializable]
    private struct Card
    {
        public Image background;
        public Image icon;
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI descriptionText;
        public TextMeshProUGUI progressText;
    }

    [Header("Panel")]
    [SerializeField] private CanvasGroup panelGroup;
    [SerializeField] private RectTransform window;
    [Tooltip("Settings panel to return to when the list closes.")]
    [SerializeField] private SettingsController returnToSettings;
    [SerializeField] private Button closeButton;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI countText;
    [SerializeField] private TextMeshProUGUI closeButtonText;

    [Header("Cards (same order as Achievements.All)")]
    [SerializeField] private Card[] cards;

    [Header("Icons")]
    [SerializeField] private Sprite starIcon;
    [SerializeField] private Sprite cloverIcon;
    [SerializeField] private Sprite gemIcon;
    [SerializeField] private Sprite heartIcon;
    [SerializeField] private Sprite foxIcon;

    [Header("Look")]
    [SerializeField] private Color unlockedName = new Color(1f, 0.91f, 0.66f);
    [SerializeField] private Color lockedName = new Color(0.55f, 0.6f, 0.8f);
    [SerializeField] private Color unlockedText = new Color(0.91f, 0.93f, 1f);
    [SerializeField] private Color lockedText = new Color(0.5f, 0.55f, 0.75f);
    [SerializeField] private Color lockedIconTint = new Color(0.12f, 0.14f, 0.28f, 1f);
    [SerializeField, Range(0f, 1f)] private float lockedCardAlpha = 0.55f;

    private Vector3 windowScale = Vector3.one;

    private void Awake()
    {
        if (window != null) windowScale = window.localScale;
        if (closeButton != null) closeButton.onClick.AddListener(Close);
        SetVisible(false);
    }

    private void OnEnable()
    {
        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
    }

    private void OnDisable()
    {
        LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
    }

    private void OnLocaleChanged(UnityEngine.Localization.Locale locale) => Refresh();

    public void Open()
    {
        Achievements.Evaluate();
        SetVisible(true);
        Refresh();
        if (window != null)
        {
            window.DOKill();
            window.localScale = windowScale * 0.85f;
            window.DOScale(windowScale, 0.25f).SetEase(Ease.OutBack).SetUpdate(true).SetLink(window.gameObject);
        }
        UISelectionUtility.SelectDefault(closeButton != null ? closeButton.gameObject : null, transform);
    }

    public void Close()
    {
        SetVisible(false);
        if (returnToSettings != null) returnToSettings.OpenSettings();
    }

    private void SetVisible(bool visible)
    {
        if (panelGroup == null) return;
        panelGroup.alpha = visible ? 1f : 0f;
        panelGroup.interactable = visible;
        panelGroup.blocksRaycasts = visible;
    }

    private void Refresh()
    {
        SetText(titleText, GameLocalization.Get("UI_ACHIEVEMENTS"));
        SetText(closeButtonText, GameLocalization.Get("UI_CLOSE_TITLE"));
        SetText(countText, $"{Achievements.UnlockedCount()}/{Achievements.All.Length}");

        SaveManager save = SaveManager.Instance;
        for (int i = 0; i < cards.Length; i++)
        {
            Card card = cards[i];
            bool used = i < Achievements.All.Length;
            if (card.background != null) card.background.gameObject.SetActive(used);
            if (!used) continue;
            Achievements.Definition definition = Achievements.All[i];
            bool unlocked = Achievements.IsUnlocked(definition.Id);

            if (card.icon != null)
            {
                card.icon.sprite = IconFor(definition.Icon);
                card.icon.color = unlocked ? Color.white : lockedIconTint;
            }
            if (card.background != null)
            {
                Color c = card.background.color;
                c.a = unlocked ? 1f : lockedCardAlpha;
                card.background.color = c;
            }
            if (card.nameText != null)
            {
                card.nameText.text = GameLocalization.Get(definition.NameKey);
                card.nameText.color = unlocked ? unlockedName : lockedName;
            }
            if (card.descriptionText != null)
            {
                card.descriptionText.text = GameLocalization.Get(definition.DescriptionKey);
                card.descriptionText.color = unlocked ? unlockedText : lockedText;
            }
            if (card.progressText != null)
            {
                bool showProgress = definition.Progress != null && definition.Goal > 0 && save != null;
                card.progressText.gameObject.SetActive(showProgress);
                if (showProgress)
                {
                    int value = Mathf.Min(definition.Progress(save), definition.Goal);
                    card.progressText.text = $"{value}/{definition.Goal}";
                    card.progressText.color = unlocked ? unlockedName : unlockedText;
                }
            }
        }
    }

    private Sprite IconFor(NotificationCenter.IconKind kind)
    {
        switch (kind)
        {
            case NotificationCenter.IconKind.Clover: return cloverIcon;
            case NotificationCenter.IconKind.Gem: return gemIcon;
            case NotificationCenter.IconKind.Heart: return heartIcon;
            case NotificationCenter.IconKind.Fox: return foxIcon;
            default: return starIcon;
        }
    }

    private static void SetText(TextMeshProUGUI target, string value)
    {
        if (target != null) target.text = value;
    }
}
