using System.Collections;
using UnityEngine;

/// <summary>
/// Coleccionable Fragmento de Dado Secreto 🎲.
/// Ubicado en zonas secretas (paredes falsas o rutas ocultas).
/// Al reunir los fragmentos necesarios del mundo, se desbloquea el Dado Secreto.
/// </summary>
public class DiceFragmentPickup : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("Número del mundo actual (1 por defecto).")]
    [SerializeField] private int worldNumber = 1;
    [Tooltip("Número del nivel. Si es 0, toma el número del nombre de escena.")]
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

        // Si ya fue recolectado en este nivel, ocultar o marcar translúcido
        if (SaveManager.Instance != null && SaveManager.Instance.HasDiceFragment(worldNumber, levelNumber))
        {
            if (spriteRenderer != null)
            {
                Color c = spriteRenderer.color;
                c.a = 0.35f;
                spriteRenderer.color = c;
            }
            if (col != null) col.enabled = false;
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
            SaveManager.Instance.SetDiceFragmentCollected(worldNumber, levelNumber);
        }

        if (AudioMixerManager._instance != null)
        {
            AudioMixerManager._instance.CallSFX_Gems();
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
