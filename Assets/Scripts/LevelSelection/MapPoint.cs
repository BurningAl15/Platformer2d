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
    public bool hasHeartFragment;

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
        if (!trebolBadge) return;

        trebolBadge.SetActive(true);
        SpriteRenderer badgeRenderer = trebolBadge.GetComponent<SpriteRenderer>();
        if (badgeRenderer != null)
        {
            badgeRenderer.color = hasTrebol ? Color.white : new Color(0.35f, 0.35f, 0.35f, 0.7f);
        }
    }

    public bool AnimateCloverArrival(Vector2 sourceViewportPosition)
    {
        if (!hasTrebol || trebolBadge == null || Camera.main == null)
            return false;

        SpriteRenderer badgeRenderer = trebolBadge.GetComponent<SpriteRenderer>();
        if (badgeRenderer == null || badgeRenderer.sprite == null)
            return false;

        Camera mapCamera = Camera.main;
        Vector3 targetPosition = badgeRenderer.transform.position;
        float cameraDepth = targetPosition.z - mapCamera.transform.position.z;
        Vector3 sourcePosition = mapCamera.ViewportToWorldPoint(
            new Vector3(sourceViewportPosition.x, sourceViewportPosition.y, cameraDepth));
        sourcePosition.z = targetPosition.z;

        GameObject travelObject = new GameObject("CloverArrivalFeedback");
        travelObject.transform.position = sourcePosition;
        SpriteRenderer travelRenderer = travelObject.AddComponent<SpriteRenderer>();
        travelRenderer.sprite = badgeRenderer.sprite;
        travelRenderer.sortingLayerID = badgeRenderer.sortingLayerID;
        travelRenderer.sortingOrder = badgeRenderer.sortingOrder + 1;

        Vector3 targetScale = badgeRenderer.transform.lossyScale;
        badgeRenderer.color = new Color(0.35f, 0.35f, 0.35f, 0.7f);

        DOTween.Sequence()
            .Append(travelObject.transform.DOMove(targetPosition, 0.48f).SetEase(Ease.InCubic))
            .Join(travelObject.transform.DOScale(targetScale, 0.48f).SetEase(Ease.InQuad))
            .Join(travelObject.transform.DORotate(new Vector3(0f, 0f, 360f), 0.48f, RotateMode.FastBeyond360))
            .AppendCallback(() =>
            {
                if (badgeRenderer == null)
                    return;

                badgeRenderer.DOColor(Color.white, 0.18f).SetEase(Ease.OutQuad)
                    .SetLink(trebolBadge, LinkBehaviour.KillOnDestroy);
                badgeRenderer.transform.DOPunchScale(Vector3.one * 0.22f, 0.32f, 8, 0.65f)
                    .SetLink(trebolBadge, LinkBehaviour.KillOnDestroy);
            })
            .OnComplete(() =>
            {
                if (travelObject != null)
                    Destroy(travelObject);
            })
            .SetLink(travelObject, LinkBehaviour.KillOnDestroy);

        return true;
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
