using System.Collections;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// Coleccionable Trébol 🍀.
/// Exactamente 1 por nivel en zonas que requieren exploración o habilidad extra.
/// Persiste permanentemente en SaveManager una vez obtenido.
/// </summary>
public class TrebolPickup : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("Nivel al que pertenece este trébol. Si es 0, toma el número del nombre de escena.")]
    [SerializeField] private int levelNumber = 0;

    [Header("Efectos")]
    [SerializeField] private GameObject effect;
    [SerializeField] private float fadeDuration = 0.5f;

    private SpriteRenderer spriteRenderer;
    private Collider2D col;
    private bool isCollected = false;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
    }

    private void Start()
    {
        if (levelNumber <= 0)
            levelNumber = SceneUtils.Get_CurrentLevelName();

        // Si ya fue recolectado en partidas anteriores, ocultar por completo de la escena.
        // El estado recolectado se refleja en la UI (HUD y mapa de niveles), no en la escena.
        if (SaveManager.Instance != null && SaveManager.Instance.HasTrebol(levelNumber))
        {
            gameObject.SetActive(false);
            return;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isCollected) return;

        if (other.CompareTag(StringUtils.tag_Player))
        {
            isCollected = true;
            Collect();
        }
    }

    private void Collect()
    {
        if (levelNumber <= 0)
            levelNumber = SceneUtils.Get_CurrentLevelName();

        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SetTrebolCollected(levelNumber);
        }
        AppBootstrap.TriggerHapticFeedback();

        Vector3 viewportPosition = Camera.main != null
            ? Camera.main.WorldToViewportPoint(transform.position)
            : new Vector3(0.5f, 0.5f, 0f);
        MapPointsHolder.QueueCloverArrival(
            levelNumber,
            new Vector2(Mathf.Clamp01(viewportPosition.x), Mathf.Clamp01(viewportPosition.y)));

        // SFX feedback
        if (AudioMixerManager._instance != null)
        {
            AudioMixerManager._instance.CallSFX(SFXType.Player_Pickup_Health);
        }

        StartCoroutine(CollectEffect());
    }

    private IEnumerator CollectEffect()
    {
        if (col != null) col.enabled = false;
        Pickup.PlayCollectBurstAt(effect, transform.position);

        Tween travelTween = UIController._instance != null
            ? UIController._instance.AnimateCloverPickup(spriteRenderer)
            : null;
        if (travelTween != null)
        {
            yield return travelTween.WaitForCompletion();
        }
        else
        {
            UIController._instance?.RefreshTrebolUI();

            if (spriteRenderer != null)
            {
                float duration = Mathf.Max(0.2f, fadeDuration);
                Sequence collectSequence = DOTween.Sequence()
                    .Join(transform.DOScale(transform.localScale * 1.3f, duration).SetEase(Ease.OutBack))
                    .Join(transform.DOMoveY(transform.position.y + 0.3f, duration).SetEase(Ease.OutQuad))
                    .Join(transform.DORotate(transform.eulerAngles + Vector3.forward * 180f, duration, RotateMode.FastBeyond360))
                    .Join(spriteRenderer.DOFade(0f, duration))
                    .SetLink(gameObject, LinkBehaviour.KillOnDestroy);
                yield return collectSequence.WaitForCompletion();
            }
        }

        if (spriteRenderer != null) spriteRenderer.enabled = false;
        Destroy(gameObject);
    }
}
