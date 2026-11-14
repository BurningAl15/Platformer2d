using System.Collections;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// Persistent heart fragment. Each level can award one fragment; every pair
/// permanently increases the player's maximum health by one full heart.
/// </summary>
public sealed class HeartFragmentPickup : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField, Min(1)] private int levelNumber;

    [Header("Collection effect")]
    [SerializeField] private GameObject effect;
    [SerializeField, Min(0f)] private float fadeDuration = 0.5f;

    private SpriteRenderer spriteRenderer;
    private Collider2D pickupCollider;
    private bool isCollected;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        pickupCollider = GetComponent<Collider2D>();
    }

    private void Start()
    {
        if (levelNumber <= 0)
            levelNumber = SceneUtils.Get_CurrentLevelName();

        if (SaveManager.Instance != null && SaveManager.Instance.HasHeartFragment(levelNumber))
            gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isCollected) return;

        PlayerHealth player = other.GetComponentInParent<PlayerHealth>();
        SaveManager save = SaveManager.Instance;
        if (player == null || save == null) return;

        if (!save.TryCollectHeartFragment(levelNumber, out bool grantedPermanentHeart))
        {
            gameObject.SetActive(false);
            return;
        }

        isCollected = true;
        AppBootstrap.TriggerHapticFeedback();

        if (AudioMixerManager._instance != null)
            AudioMixerManager._instance.CallSFX(SFXType.Player_Pickup_Health);

        StartCoroutine(CollectEffect(player, grantedPermanentHeart));
    }

    private IEnumerator CollectEffect(PlayerHealth player, bool grantsPermanentHeart)
    {
        if (pickupCollider != null) pickupCollider.enabled = false;
        Pickup.PlayCollectBurstAt(effect, transform.position);

        if (grantsPermanentHeart)
        {
            Tween travelTween = UIController._instance != null
                ? UIController._instance.AnimateHeartFragmentPickup(spriteRenderer, player.AddPermanentHeart)
                : null;

            if (travelTween != null)
            {
                yield return travelTween.WaitForCompletion();
            }
            else
            {
                player.AddPermanentHeart();
                UIController._instance?.AnimatePermanentHeartGain();
            }

            if (spriteRenderer != null) spriteRenderer.enabled = false;
            Destroy(gameObject);
            yield break;
        }

        if (spriteRenderer != null)
        {
            float duration = Mathf.Max(0.2f, fadeDuration);
            Vector3 startScale = transform.localScale;
            Sequence collectSequence = DOTween.Sequence()
                .Join(transform.DOPunchScale(Vector3.one * 0.35f, duration, 8, 0.7f))
                .Join(transform.DOMoveY(transform.position.y + 0.25f, duration).SetEase(Ease.OutQuad))
                .Join(transform.DORotate(transform.eulerAngles + Vector3.forward * 360f, duration, RotateMode.FastBeyond360))
                .Join(spriteRenderer.DOFade(0f, duration))
                .SetLink(gameObject, LinkBehaviour.KillOnDestroy);
            yield return collectSequence.WaitForCompletion();
            if (transform != null) transform.localScale = startScale;
        }

        if (spriteRenderer != null) spriteRenderer.enabled = false;
        Destroy(gameObject);
    }
}
