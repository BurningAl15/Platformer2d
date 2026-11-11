using System.Collections;
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
        if (grantedPermanentHeart)
            player.AddPermanentHeart();

        if (AudioMixerManager._instance != null)
            AudioMixerManager._instance.CallSFX(SFXType.Player_Pickup_Health);

        StartCoroutine(CollectEffect());
    }

    private IEnumerator CollectEffect()
    {
        if (pickupCollider != null) pickupCollider.enabled = false;
        if (spriteRenderer != null) spriteRenderer.enabled = false;

        if (effect != null)
        {
            effect.SetActive(true);
            TurnOff_OnTime turnOff = effect.GetComponent<TurnOff_OnTime>();
            if (turnOff != null)
                yield return new WaitUntil(() => turnOff.finish);
            else
                yield return new WaitForSeconds(fadeDuration);
        }
        else if (fadeDuration > 0f)
        {
            yield return new WaitForSeconds(fadeDuration);
        }

        Destroy(gameObject);
    }
}
