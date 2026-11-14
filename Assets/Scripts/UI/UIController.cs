using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

// ─────────────────────────────────────────────────────────────────────────────
//  Clase auxiliar para los corazones de salud
// ─────────────────────────────────────────────────────────────────────────────
[Serializable]
public class HeartUI
{
    public Image heartImage;
    private Sprite heartEmpty;
    private Sprite heartHalf;
    private Sprite heartFull;

    public void Init(Sprite empty, Sprite half, Sprite full)
    {
        heartEmpty = empty;
        heartHalf  = half;
        heartFull  = full;

        if (heartImage != null)
            heartImage.sprite = heartFull;
    }

    public void UpdateHeartSprite_Damage(int currentHealth)
    {
        heartImage.sprite = currentHealth % 2 == 0 ? heartEmpty : heartHalf;
    }

    public void UpdateHeartSprite_Cure(int currentHealth)
    {
        heartImage.sprite = currentHealth % 2 == 1 ? heartFull : heartHalf;
    }

    public void SetHealth(int health, int slotIndex)
    {
        if (heartImage == null) return;

        int healthForHeart = health - (slotIndex * 2);
        heartImage.sprite = healthForHeart >= 2 ? heartFull : healthForHeart == 1 ? heartHalf : heartEmpty;
    }
}

// ─────────────────────────────────────────────────────────────────────────────
//  UIController — HUD de gameplay
// ─────────────────────────────────────────────────────────────────────────────
/// <summary>
/// Controla todos los elementos del HUD durante el gameplay:
/// - Corazones de salud.
/// - Contador de gemas del nivel y wallet global.
/// - Animaciones de fin de nivel.
/// - Pantalla de estrellas al completar el nivel.
/// </summary>
public class UIController : MonoBehaviour
{
    public static UIController _instance;

    // ─────────────────────────────
    //  Salud
    // ─────────────────────────────
    [Header("Corazones")]
    [SerializeField] private List<HeartUI> hearts = new List<HeartUI>();
    [SerializeField] private Sprite heartEmpty;
    [SerializeField] private Sprite heartHalf;
    [SerializeField] private Sprite heartFull;

    [Header("Corazon extra - Dado Secreto")]
    [Tooltip("Slot visual del corazón extra. Se muestra cuando la salud máxima supera los slots base.")]
    [SerializeField] private Image bonusHeartIcon;

    // ─────────────────────────────
    //  Gemas
    // ─────────────────────────────
    [Header("Gemas")]
    [SerializeField] private TextMeshProUGUI gemsLevelText;
    [Tooltip("(Opcional) Texto del wallet global de gemas en el HUD")]
    [SerializeField] private TextMeshProUGUI gemsGlobalText;

    // ─────────────────────────────
    //  Tréboles 🍀
    // ─────────────────────────────
    [Header("Tréboles")]
    [Tooltip("(Opcional) Texto o contador de tréboles acumulados en el HUD")]
    // [SerializeField] private TextMeshProUGUI trebolCountText;
    [SerializeField] private Image cloverImg;
    [SerializeField] private Color disabledColor, gotColor;

    // ─────────────────────────────
    //  Vidas
    // ─────────────────────────────
    [Header("Vidas")]
    [Tooltip("(Opcional) Texto o imagen que muestra las vidas restantes en el HUD")]
    [SerializeField] private TextMeshProUGUI livesText;

    // ─────────────────────────────
    //  Fin de nivel
    // ─────────────────────────────
    [Header("Fin de nivel")]
    [SerializeField] private Animator endLevel_animator;
    [SerializeField] private Animator amazingJob_animator;

    [Header("Estrellas de calificación")]
    [Tooltip("Imágenes de las 3 estrellas en la pantalla de fin de nivel")]
    [SerializeField] private List<Image> starImages;
    [SerializeField] private Sprite starFull;
    [SerializeField] private Sprite starEmpty;

    private Coroutine currentCoroutine = null;
    private Coroutine heartGainAnimation;
    private Image livesIcon;

    // ─────────────────────────────────────────────────────────────────────────
    //  Ciclo de vida
    // ─────────────────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (_instance != null && _instance != this) { Destroy(gameObject); return; }
        _instance = this;

        if (livesText != null && livesText.transform.parent != null)
        {
            Transform iconTransform = livesText.transform.parent.Find("Lives_Img");
            if (iconTransform != null)
                livesIcon = iconTransform.GetComponent<Image>();
        }

        foreach (var heart in hearts)
            heart.Init(heartEmpty, heartHalf, heartFull);

        if (bonusHeartIcon != null)
        {
            bonusHeartIcon.sprite = heartEmpty;
            bonusHeartIcon.gameObject.SetActive(false);
        }

        if (endLevel_animator != null) endLevel_animator.gameObject.SetActive(false);
        if (amazingJob_animator != null) amazingJob_animator.gameObject.SetActive(false);
    }

    private void Start()
    {
        FadeEffect._instance.Initialize();

        if (endLevel_animator != null) endLevel_animator.gameObject.SetActive(false);
        if (amazingJob_animator != null) amazingJob_animator.gameObject.SetActive(false);

        // Configurar auto-sizing y no-wrap en todos los textos del HUD
        ConfigureAutoResize(gemsLevelText, 24f, 90f);
        ConfigureAutoResize(gemsGlobalText, 16f, 48f);
        // ConfigureAutoResize(trebolCountText, 16f, 36f);
        ConfigureAutoResize(livesText, 16f, 36f);

        // Mostrar vidas iniciales
        RefreshLivesUI();
        RefreshHealthDisplay();

        // Mostrar gemas globales si hay texto asignado
        RefreshGlobalGemsUI();

        // Mostrar tréboles acumulados
        RefreshTrebolUI();

        // Suscribirse a cambios de vidas
        if (LivesManager.Instance != null)
            LivesManager.Instance.OnLivesChanged += OnLivesChanged;
    }

    private void OnDestroy()
    {
        if (LivesManager.Instance != null)
            LivesManager.Instance.OnLivesChanged -= OnLivesChanged;
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Salud
    // ─────────────────────────────────────────────────────────────────────────

    public void UpdateHealthDisplay_Damage(int index)
    {
        int heartIndex = index / 2;
        if (heartIndex >= 0 && heartIndex < hearts.Count)
            hearts[heartIndex].UpdateHeartSprite_Damage(index);
    }

    public void UpdateHealthDisplay_Cure(int index)
    {
        int heartIndex = index / 2;
        if (heartIndex >= 0 && heartIndex < hearts.Count)
            hearts[heartIndex].UpdateHeartSprite_Cure(index);
    }

    public void ResetHearts()
    {
        RefreshHealthDisplay();
    }

    /// <summary>Actualiza los slots base y el slot extra opcional según PlayerHealth.</summary>
    public void RefreshHealthDisplay()
    {
        int health = PlayerHealth._instance != null ? PlayerHealth._instance.CurrentHealth : hearts.Count * 2;
        int maxHealth = PlayerHealth._instance != null ? PlayerHealth._instance.MaxHealth : hearts.Count * 2;

        for (int i = 0; i < hearts.Count; i++)
        {
            var heart = hearts[i];
            if (heart?.heartImage == null) continue;

            bool slotAvailable = i * 2 < maxHealth;
            heart.heartImage.gameObject.SetActive(slotAvailable);
            if (slotAvailable)
                heart.SetHealth(health, i);
        }

        if (bonusHeartIcon != null)
        {
            int baseCapacity = hearts.Count * 2;
            bool hasBonusHeart = maxHealth > baseCapacity;
            bonusHeartIcon.gameObject.SetActive(hasBonusHeart);
            if (hasBonusHeart)
            {
                int bonusHealth = Mathf.Clamp(health - baseCapacity, 0, 2);
                bonusHeartIcon.sprite = bonusHealth >= 2 ? heartFull : bonusHealth == 1 ? heartHalf : heartEmpty;
            }
        }
    }

    public void AnimatePermanentHeartGain()
    {
        if (hearts == null || hearts.Count == 0 || PlayerHealth._instance == null) return;

        int heartIndex = Mathf.Clamp(PlayerHealth._instance.MaxHealth / 2 - 1, 0, hearts.Count - 1);
        Image image = hearts[heartIndex]?.heartImage;
        if (image == null || !image.gameObject.activeInHierarchy) return;

        if (heartGainAnimation != null)
            StopCoroutine(heartGainAnimation);
        heartGainAnimation = StartCoroutine(AnimateHeartScale(image.transform));
    }

    private IEnumerator AnimateHeartScale(Transform target)
    {
        Vector3 baseScale = target.localScale;
        target.DOKill();
        target.DOPunchScale(Vector3.one * 0.35f, 0.45f, 8, 0.65f);
        yield return new WaitForSecondsRealtime(0.5f);
        if (target != null) target.localScale = baseScale;
        heartGainAnimation = null;
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Gemas
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Actualiza el contador de gemas del nivel actual (formato X o X / Total).</summary>
    public void UpdateGems_UI(int currentGems, int totalGems = 0)
    {
        if (gemsLevelText != null)
        {
            gemsLevelText.enableAutoSizing = true;
            gemsLevelText.fontSizeMin = 24f;
            gemsLevelText.fontSizeMax = 90f;
            gemsLevelText.textWrappingMode = TextWrappingModes.NoWrap;
            gemsLevelText.overflowMode = TextOverflowModes.Ellipsis;
            gemsLevelText.text = totalGems > 0
                ? $"{currentGems} / {totalGems}"
                : currentGems.ToString();
        }
    }

    /// <summary>Moves a collected gem to the existing HUD counter before updating its displayed value.</summary>
    public Tween AnimateGemPickup(SpriteRenderer gemRenderer, int currentGems, int totalGems)
    {
        if (gemRenderer == null || gemsLevelText == null)
            return null;

        return AnimatePickupToImage(gemRenderer, gemsLevelText, 0.38f, () =>
        {
            UpdateGems_UI(currentGems, totalGems);
            if (gemsLevelText != null)
            {
                gemsLevelText.transform.DOKill();
                gemsLevelText.transform.DOPunchScale(Vector3.one * 0.12f, 0.22f, 5, 0.5f);
            }
        });
    }

    /// <summary>Moves a collected Clover to its existing gameplay HUD icon.</summary>
    public Tween AnimateCloverPickup(SpriteRenderer cloverRenderer)
    {
        if (cloverRenderer == null || cloverImg == null)
            return null;

        return AnimatePickupToImage(cloverRenderer, cloverImg, 0.48f, () =>
        {
            RefreshTrebolUI();
            cloverImg.transform.DOKill();
            cloverImg.transform.DOPunchScale(Vector3.one * 0.22f, 0.32f, 7, 0.6f);
        });
    }

    /// <summary>Moves a completing heart fragment to the next heart slot before granting it.</summary>
    public Tween AnimateHeartFragmentPickup(SpriteRenderer fragmentRenderer, Action onArrival)
    {
        if (fragmentRenderer == null || PlayerHealth._instance == null)
            return null;

        int nextHeartIndex = PlayerHealth._instance.MaxHealth / 2;
        if (hearts == null || nextHeartIndex < 0 || nextHeartIndex >= hearts.Count ||
            hearts[nextHeartIndex]?.heartImage == null)
            return null;

        return AnimatePickupToImage(fragmentRenderer, hearts[nextHeartIndex].heartImage, 0.62f, () =>
        {
            onArrival?.Invoke();
            AnimatePermanentHeartGain();
        });
    }

    private Tween AnimatePickupToImage(SpriteRenderer source, Graphic target, float duration, Action onArrival)
    {
        if (source == null || source.sprite == null || target == null || Camera.main == null)
            return null;

        Canvas canvas = target.canvas != null ? target.canvas.rootCanvas : null;
        RectTransform canvasRect = canvas != null ? canvas.transform as RectTransform : null;
        if (canvasRect == null)
            return null;

        Camera uiCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : (canvas.worldCamera != null ? canvas.worldCamera : Camera.main);
        Vector3 sourceScreenPosition = Camera.main.WorldToScreenPoint(source.bounds.center);
        if (sourceScreenPosition.z <= 0f)
            return null;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect, sourceScreenPosition, uiCamera, out Vector2 startLocalPosition))
            return null;

        Vector3 boundsMin = Camera.main.WorldToScreenPoint(source.bounds.min);
        Vector3 boundsMax = Camera.main.WorldToScreenPoint(source.bounds.max);
        float scaleFactor = Mathf.Max(canvas.scaleFactor, 0.01f);
        Vector2 flightSize = new Vector2(
            Mathf.Abs(boundsMax.x - boundsMin.x),
            Mathf.Abs(boundsMax.y - boundsMin.y)) / scaleFactor;

        GameObject flightObject = new GameObject("PickupFlight", typeof(RectTransform),
            typeof(CanvasRenderer), typeof(Image));
        RectTransform flightRect = flightObject.GetComponent<RectTransform>();
        flightRect.SetParent(canvasRect, false);
        flightRect.anchorMin = flightRect.anchorMax = new Vector2(0.5f, 0.5f);
        flightRect.pivot = new Vector2(0.5f, 0.5f);
        flightRect.anchoredPosition = startLocalPosition;
        flightRect.sizeDelta = flightSize;

        Image flightImage = flightObject.GetComponent<Image>();
        flightImage.sprite = source.sprite;
        flightImage.color = source.color;
        flightImage.preserveAspect = true;
        flightImage.raycastTarget = false;

        Sequence sequence = DOTween.Sequence()
            .Append(DOTween.To(() => 0f, progress =>
            {
                if (flightRect == null || target == null)
                    return;

                Vector2 targetLocalPosition = GetTargetCanvasPosition(target.rectTransform, canvasRect, uiCamera);
                float easedProgress = 1f - Mathf.Pow(1f - progress, 3f);
                flightRect.anchoredPosition = Vector2.LerpUnclamped(
                    startLocalPosition, targetLocalPosition, easedProgress);
            }, 1f, duration).SetEase(Ease.Linear))
            .SetUpdate(UpdateType.Late)
            .AppendCallback(() =>
            {
                if (flightRect != null && target != null)
                    flightRect.anchoredPosition = GetTargetCanvasPosition(target.rectTransform, canvasRect, uiCamera);
                onArrival?.Invoke();
                if (flightObject != null)
                    Destroy(flightObject);
            })
            .SetLink(flightObject, LinkBehaviour.KillOnDestroy);
        source.enabled = false;
        return sequence;
    }

    private static Vector2 GetTargetCanvasPosition(RectTransform targetRect, RectTransform canvasRect, Camera uiCamera)
    {
        Vector2 targetScreenPosition = RectTransformUtility.WorldToScreenPoint(
            uiCamera,
            targetRect.position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect, targetScreenPosition, uiCamera, out Vector2 localPosition);
        return localPosition;
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

    /// <summary>Refresca el texto de gemas globales del wallet.</summary>
    public void RefreshGlobalGemsUI()
    {
        if (gemsGlobalText == null || SaveManager.Instance == null) return;
        gemsGlobalText.text = SaveManager.Instance.LoadGlobalGems().ToString();
    }

    /// <summary>Refreshes the current level's Clover HUD state.</summary>
    public void RefreshTrebolUI()
    {
        if (cloverImg == null) return;

        cloverImg.gameObject.SetActive(true);
        int levelNumber = SceneUtils.Get_CurrentLevelName();
        bool hasCollectedClover = levelNumber > 0 && SaveManager.Instance != null &&
                                  SaveManager.Instance.HasTrebol(levelNumber);
        cloverImg.color = hasCollectedClover ? gotColor : disabledColor;
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Vidas
    // ─────────────────────────────────────────────────────────────────────────

    private void OnLivesChanged(int lives) => RefreshLivesUI();

    public void RefreshLivesUI()
    {
        if (livesText == null || SaveManager.Instance == null) return;
        livesText.text = $"x{SaveManager.Instance.LoadLives()}";
    }

    /// <summary>Sends fox-life tokens from a reward source to the existing lives HUD.</summary>
    public void AnimateLifeRewardFeedback(int awardedLives, Transform source)
    {
        if (livesText == null || awardedLives <= 0 || livesIcon == null || livesIcon.sprite == null ||
            source == null || livesText.canvas == null)
        {
            RefreshLivesUI();
            PunchLivesCounter();
            return;
        }

        Canvas canvas = livesText.canvas;
        RectTransform canvasRect = canvas.transform as RectTransform;
        if (canvasRect == null)
        {
            RefreshLivesUI();
            PunchLivesCounter();
            return;
        }

        Camera uiCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : (canvas.worldCamera != null ? canvas.worldCamera : Camera.main);
        int finalLives = SaveManager.Instance != null ? SaveManager.Instance.LoadLives() : 0;
        int startingLives = Mathf.Max(0, finalLives - awardedLives);
        livesText.text = $"x{startingLives}";

        for (int i = 0; i < awardedLives; i++)
        {
            int lifeIndex = i;
            Image token = CreateLifeRewardToken(canvasRect, uiCamera, source, livesIcon);
            if (token == null) continue;

            RectTransform tokenRect = token.rectTransform;
            Vector2 startPosition = tokenRect.anchoredPosition;
            Sequence flight = DOTween.Sequence();
            flight.SetDelay(lifeIndex * 0.09f);
            flight.OnStart(() =>
            {
                if (token == null || source == null) return;
                tokenRect.anchoredPosition = GetCanvasLocalPosition(
                    canvasRect, source as RectTransform, uiCamera, source.position);
                token.color = Color.white;
            });
            flight.Append(DOTween.To(() => 0f, progress =>
                {
                    if (tokenRect == null || livesIcon == null) return;
                    Vector2 targetPosition = GetCanvasLocalPosition(canvasRect, livesIcon.rectTransform, uiCamera);
                    float easedProgress = progress * progress * progress;
                    tokenRect.anchoredPosition = Vector2.LerpUnclamped(startPosition, targetPosition, easedProgress);
                    tokenRect.localScale = Vector3.one * Mathf.Lerp(1f, 0.55f, progress);
                }, 1f, 0.42f).SetEase(Ease.Linear));
            flight.OnComplete(() =>
            {
                if (token != null) Destroy(token.gameObject);
                if (livesText != null) livesText.text = $"x{startingLives + lifeIndex + 1}";
                PunchLivesCounter();
            });
            flight.OnKill(() =>
            {
                if (token != null) Destroy(token.gameObject);
            });
            flight.SetUpdate(true);
            flight.SetLink(token.gameObject, LinkBehaviour.KillOnDestroy);
        }

        DOVirtual.DelayedCall((awardedLives - 1) * 0.09f + 0.5f, RefreshLivesUI, true)
            .SetLink(gameObject, LinkBehaviour.KillOnDestroy);
    }

    private static Image CreateLifeRewardToken(RectTransform canvasRect, Camera uiCamera, Transform source, Image livesIcon)
    {
        Vector2 sourceScreenPosition = RectTransformUtility.WorldToScreenPoint(uiCamera, source.position);
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect, sourceScreenPosition, uiCamera, out Vector2 startPosition))
            return null;

        GameObject tokenObject = new GameObject("LifeRewardToken", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        tokenObject.transform.SetParent(canvasRect, false);
        Image token = tokenObject.GetComponent<Image>();
        token.sprite = livesIcon.sprite;
        token.preserveAspect = true;
        token.raycastTarget = false;
        token.color = new Color(1f, 1f, 1f, 0f);
        token.rectTransform.sizeDelta = livesIcon.rectTransform.rect.size;
        token.rectTransform.anchoredPosition = startPosition;
        return token;
    }

    private static Vector2 GetCanvasLocalPosition(RectTransform canvasRect, RectTransform target, Camera uiCamera)
    {
        return GetCanvasLocalPosition(canvasRect, target, uiCamera, target.position);
    }

    private static Vector2 GetCanvasLocalPosition(RectTransform canvasRect, RectTransform target, Camera uiCamera, Vector3 worldPosition)
    {
        Vector2 targetScreenPosition = RectTransformUtility.WorldToScreenPoint(uiCamera, worldPosition);
        return RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect, targetScreenPosition, uiCamera, out Vector2 targetLocalPosition)
            ? targetLocalPosition
            : target != null ? target.anchoredPosition : Vector2.zero;
    }

    private void PunchLivesCounter()
    {
        Transform livesContainer = livesText != null && livesText.transform.parent != null
            ? livesText.transform.parent
            : livesText != null ? livesText.transform : null;
        if (livesContainer == null) return;

        livesContainer.DOKill();
        livesContainer.DOPunchScale(Vector3.one * 0.22f, 0.38f, 8, 0.65f);
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Fin de nivel
    // ─────────────────────────────────────────────────────────────────────────

    public void Run_EndLevelAnimation()
    {
        if (endLevel_animator != null)
            endLevel_animator.gameObject.SetActive(true);
    }

    public void Run_AmazingJobAnimation()
    {
        if (amazingJob_animator == null) return;
        amazingJob_animator.gameObject.SetActive(true);
        if (currentCoroutine == null)
            currentCoroutine = StartCoroutine(TurnOffAmazingJob(2f));
    }

    /// <summary>
    /// Muestra las estrellas de calificación en la pantalla de fin de nivel.
    /// Llamado por LevelManager al completar el nivel.
    /// </summary>
    public void ShowLevelRating(LevelRating rating)
    {
        if (starImages == null || starImages.Count == 0) return;

        bool[] states = LevelRatingSystem.GetStarStates(rating, starImages.Count);
        for (int i = 0; i < starImages.Count; i++)
        {
            if (starImages[i] == null) continue;
            starImages[i].sprite = states[i] ? starFull : starEmpty;
            starImages[i].gameObject.SetActive(true);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Helpers privados
    // ─────────────────────────────────────────────────────────────────────────

    private IEnumerator TurnOffAmazingJob(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        if (amazingJob_animator != null)
            amazingJob_animator.gameObject.SetActive(false);
        currentCoroutine = null;
    }
}
