using System.Collections;
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

        if (UIController._instance != null)
        {
            UIController._instance.RefreshTrebolUI();
        }

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
        if (spriteRenderer != null) spriteRenderer.enabled = false;

        if (effect != null)
        {
            effect.SetActive(true);
            var turnOff = effect.GetComponent<TurnOff_OnTime>();
            if (turnOff != null)
                yield return new WaitUntil(() => turnOff.finish);
            else
                yield return new WaitForSeconds(fadeDuration);
        }

        Destroy(gameObject);
    }
}
