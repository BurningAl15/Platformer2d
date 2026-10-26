using DG.Tweening;
using UnityEngine;

/// <summary>
/// Nodo del mapa de selección de niveles.
/// Contiene referencias a nodos vecinos, datos del nivel y badges de logros.
/// Expone Select() / Deselect() para feedback visual al navegar.
/// </summary>
public class MapPoint : MonoBehaviour
{
    [Header("Navegación")]
    public MapPoint up, down, right, left;

    [Header("Datos del nivel")]
    public bool isLevel;
    public bool isLocked;
    public int currentLevel;
    public string levelName;

    [Header("Estadísticas")]
    public int gemsCollected, totalGems;
    public float bestTime, targetTime;
    public bool hasTrebol;

    [Header("Badges")]
    [SerializeField] private GameObject timeBadge, gemsBadge, trebolBadge;

    [Header("Feedback visual de selección")]
    [Tooltip("Escala base del nodo")]
    [SerializeField] private float normalScale = 1f;
    [Tooltip("Escala cuando el nodo está seleccionado")]
    [SerializeField] private float selectedScale = 1.2f;
    [Tooltip("Duración del tween de escala")]
    [SerializeField] private float scaleDuration = 0.2f;

    private void Awake()
    {
        if (timeBadge) timeBadge.SetActive(false);
        if (gemsBadge) gemsBadge.SetActive(false);
        if (trebolBadge) trebolBadge.SetActive(false);
    }

    // ───────────────────────────────────────────────
    //  Badges
    // ───────────────────────────────────────────────

    public void CheckGems()
    {
        if (gemsBadge)
            gemsBadge.SetActive(gemsCollected >= totalGems);
    }

    public void CheckTime()
    {
        if (timeBadge)
            timeBadge.SetActive(bestTime <= targetTime && bestTime < 99999f);
    }

    public void CheckTrebol()
    {
        if (trebolBadge)
            trebolBadge.SetActive(hasTrebol);
    }

    // ───────────────────────────────────────────────
    //  Feedback visual
    // ───────────────────────────────────────────────

    /// <summary>
    /// Anima el nodo al estado "seleccionado" (escala aumentada).
    /// </summary>
    public void Select()
    {
        transform.DOKill();
        transform.DOScale(selectedScale, scaleDuration).SetEase(Ease.OutBack);
    }

    /// <summary>
    /// Regresa el nodo a su escala normal.
    /// </summary>
    public void Deselect()
    {
        transform.DOKill();
        transform.DOScale(normalScale, scaleDuration).SetEase(Ease.InOutSine);
    }

    // ───────────────────────────────────────────────
    //  Gizmos de edición
    // ───────────────────────────────────────────────

    private void OnDrawGizmos()
    {
        if (up)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, up.transform.position);
        }
        if (down)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, down.transform.position);
        }
        if (right)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, right.transform.position);
        }
        if (left)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawLine(transform.position, left.transform.position);
        }
    }
}
