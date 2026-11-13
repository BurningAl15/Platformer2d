using System.Collections;
using DG.Tweening;
using UnityEngine;

public class Pickup : MonoBehaviour
{
    public enum PickupType
    {
        Gem,
        Health
    }

    public PickupType _pickupType;

    [SerializeField] private GameObject effect;

    private Coroutine currentCoroutine = null;
    private bool hasBeenCollected;

    [SerializeField] private Transform pickupTransform;
    public Transform PickupTransform => pickupTransform;

    private SpriteRenderer spriteRenderer;
    private Collider2D pickupCollider;

    private void Awake()
    {
        pickupTransform = transform;
        spriteRenderer = GetComponent<SpriteRenderer>();
        pickupCollider = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(StringUtils.tag_Player) || hasBeenCollected)
            return;

        if (_pickupType == PickupType.Gem)
        {
            // A player can have multiple colliders and receive repeated trigger
            // callbacks before the collect animation destroys this object.
            // Claim this pickup before changing the level total.
            hasBeenCollected = true;
            LevelManager levelManager = LevelManager._instance;
            levelManager?.UpdateGemsCollected(false);
            AudioMixerManager._instance?.CallSFX_Gems();
            // Global gems are awarded when the level is completed.
            currentCoroutine = StartCoroutine(CollectGemEffect(levelManager));
            return;
        }

        if (_pickupType == PickupType.Health && PlayerHealth._instance != null &&
            PlayerHealth._instance.isDamaged())
        {
            hasBeenCollected = true;
            PlayerHealth._instance.CureDamage();
            AppBootstrap.TriggerHapticFeedback();
            AudioMixerManager._instance?.CallSFX(SFXType.Player_Pickup_Health);
            currentCoroutine = StartCoroutine(CollectEffect());
        }
    }

    private IEnumerator CollectGemEffect(LevelManager levelManager)
    {
        if (pickupCollider != null)
            pickupCollider.enabled = false;

        int collected = levelManager != null ? levelManager.GetGemsCollected() : 0;
        int total = levelManager != null ? levelManager.GetTotalGemsInLevel() : 0;
        Tween travelTween = UIController._instance != null && spriteRenderer != null
            ? UIController._instance.AnimateGemPickup(transform, collected, total)
            : null;

        if (travelTween != null)
            yield return travelTween.WaitForCompletion();
        else
            UIController._instance?.UpdateGems_UI(collected, total);

        yield return CollectEffect();
    }

    private IEnumerator CollectEffect()
    {
        if (pickupCollider != null)
            pickupCollider.enabled = false;

        if (spriteRenderer != null)
            spriteRenderer.enabled = false;

        if (effect != null)
        {
            effect.SetActive(true);
            TurnOff_OnTime effectTimer = effect.GetComponent<TurnOff_OnTime>();
            if (effectTimer != null)
                yield return new WaitUntil(() => effectTimer == null || effectTimer.finish);
            else
                yield return new WaitForSeconds(0.35f);
        }

        Destroy(gameObject);
    }

    public bool IsGem()
    {
        return _pickupType == PickupType.Gem;
    }
}
