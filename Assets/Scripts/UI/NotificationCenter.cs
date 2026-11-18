using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// On-screen notices for achievements and unlocks. One persistent overlay
/// (Resources/UI/NotificationCenter) shows queued notices one at a time:
/// it slides down from the top, holds, then slides back up.
/// </summary>
public class NotificationCenter : MonoBehaviour
{
    public enum IconKind { Star, Clover, Gem, Heart, Fox }

    private const string PrefabPath = "UI/NotificationCenter";
    private static NotificationCenter instance;

    [SerializeField] private RectTransform toast;
    [SerializeField] private CanvasGroup toastGroup;
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI messageText;

    [Header("Icons")]
    [SerializeField] private Sprite starIcon;
    [SerializeField] private Sprite cloverIcon;
    [SerializeField] private Sprite gemIcon;
    [SerializeField] private Sprite heartIcon;
    [SerializeField] private Sprite foxIcon;
    [Tooltip("Original idle frame; unlock notices show the unlocked skin's copy.")]
    [SerializeField] private Sprite skinPreviewSource;

    [Header("Timing")]
    [SerializeField] private float visibleY = -90f;
    [SerializeField] private float hiddenY = 140f;
    [SerializeField, Min(0.5f)] private float holdSeconds = 2.4f;

    private readonly Queue<(string title, string message, Sprite sprite)> pending =
        new Queue<(string, string, Sprite)>();
    private bool showing;

    private static NotificationCenter Instance
    {
        get
        {
            if (instance != null) return instance;
            GameObject prefab = Resources.Load<GameObject>(PrefabPath);
            if (prefab == null) return null;
            instance = Instantiate(prefab).GetComponent<NotificationCenter>();
            DontDestroyOnLoad(instance.gameObject);
            return instance;
        }
    }

    /// <summary>Queues a notice with an explicit icon sprite.</summary>
    public static void Show(string title, string message, Sprite sprite)
    {
        NotificationCenter center = Instance;
        if (center == null) return;
        center.pending.Enqueue((title, message, sprite));
        if (!center.showing) center.ShowNext();
    }

    /// <summary>Queues a notice using one of the shared icons.</summary>
    public static void Show(string title, string message, IconKind kind)
    {
        NotificationCenter center = Instance;
        if (center == null) return;
        Show(title, message, center.IconFor(kind));
    }

    /// <summary>Queues a notice whose icon is the given skin's idle frame.</summary>
    public static void Show(string title, string message, IconKind fallback, int style, int color)
    {
        NotificationCenter center = Instance;
        if (center == null) return;
        Sprite preview = center.skinPreviewSource != null
            ? PlayerSkins.ResolveFor(center.skinPreviewSource, style, color)
            : null;
        Show(title, message, preview != null ? preview : center.IconFor(fallback));
    }

    private Sprite IconFor(IconKind kind)
    {
        switch (kind)
        {
            case IconKind.Clover: return cloverIcon;
            case IconKind.Gem: return gemIcon;
            case IconKind.Heart: return heartIcon;
            case IconKind.Fox: return foxIcon;
            default: return starIcon;
        }
    }

    private void Awake()
    {
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this;
        SetY(hiddenY);
        if (toastGroup != null) toastGroup.alpha = 0f;
    }

    private void ShowNext()
    {
        if (pending.Count == 0) { showing = false; return; }
        showing = true;
        var (title, message, sprite) = pending.Dequeue();

        if (titleText != null) titleText.text = title;
        if (messageText != null) messageText.text = message;
        if (icon != null) { icon.sprite = sprite; icon.enabled = sprite != null; }

        AppBootstrap.TriggerHapticFeedback();
        if (AudioMixerManager._instance != null)
            AudioMixerManager._instance.CallSFX(SFXType.Warp_Jingle);

        toast.DOKill();
        SetY(hiddenY);
        if (toastGroup != null) { toastGroup.DOKill(); toastGroup.alpha = 0f; toastGroup.DOFade(1f, 0.2f).SetUpdate(true); }
        Sequence sequence = DOTween.Sequence()
            .Append(toast.DOAnchorPosY(visibleY, 0.35f).SetEase(Ease.OutBack));
        if (icon != null)
            sequence.Join(icon.transform.DOPunchScale(Vector3.one * 0.25f, 0.45f, 8, 0.6f));
        sequence.AppendInterval(holdSeconds)
            .Append(toast.DOAnchorPosY(hiddenY, 0.3f).SetEase(Ease.InBack))
            .SetUpdate(true)
            .SetLink(gameObject)
            .OnComplete(ShowNext);
    }

    private void SetY(float y)
    {
        if (toast == null) return;
        toast.anchoredPosition = new Vector2(toast.anchoredPosition.x, y);
    }
}
