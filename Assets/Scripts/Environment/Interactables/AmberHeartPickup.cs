using DG.Tweening;
using UnityEngine;

/// <summary>
/// The Amber Heart, the relic at the end of the journey (place it in the final
/// level of a world). Collecting it is saved once, shows a notice and plays a
/// short celebration. The level still ends at its Goal.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class AmberHeartPickup : MonoBehaviour
{
    public const string FoundKey = "AmberHeart_Found";

    [SerializeField] private SpriteRenderer heartRenderer;
    [SerializeField] private SpriteRenderer glowRenderer;
    [SerializeField] private GameObject collectEffect;
    [SerializeField] private Sprite noticeIcon;

    private bool taken;

    public static bool IsFound => PlayerPrefs.GetInt(FoundKey, 0) == 1;

    private void Start()
    {
        transform.DOMoveY(transform.position.y + 0.25f, 1.2f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo).SetLink(gameObject);
        if (glowRenderer != null)
        {
            glowRenderer.transform.DOScale(glowRenderer.transform.localScale * 1.15f, 0.9f).SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo).SetLink(gameObject);
            glowRenderer.DOFade(0.35f, 0.9f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo).SetLink(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (taken || !other.CompareTag(StringUtils.tag_Player)) return;
        taken = true;
        PlayerPrefs.SetInt(FoundKey, 1);
        PlayerPrefs.Save();

        AudioMixerManager._instance?.CallSFX_Gems();
        AppBootstrap.TriggerHapticFeedback();
        if (ScreenShake._instance != null) ScreenShake._instance.StartShake(0.3f, 0.12f);
        if (collectEffect != null) SpawnFactory.Spawn(collectEffect, transform.position, Quaternion.identity);
        NotificationCenter.Show(GameLocalization.Get("AMBER_HEART_TITLE"), GameLocalization.Get("AMBER_HEART_FOUND"), noticeIcon);

        GetComponent<Collider2D>().enabled = false;
        transform.DOKill();
        Sequence celebrate = DOTween.Sequence().SetLink(gameObject);
        celebrate.Append(transform.DOMoveY(transform.position.y + 1.2f, 0.6f).SetEase(Ease.OutQuad));
        celebrate.Join(transform.DOScale(transform.localScale * 1.5f, 0.6f).SetEase(Ease.OutBack));
        if (heartRenderer != null) celebrate.Append(heartRenderer.DOFade(0f, 0.35f));
        if (glowRenderer != null) celebrate.Join(glowRenderer.DOFade(0f, 0.35f));
        celebrate.OnComplete(() => gameObject.SetActive(false));
    }
}
