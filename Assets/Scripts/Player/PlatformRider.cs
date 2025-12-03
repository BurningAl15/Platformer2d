using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlatformRider : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool inheritVelocityOnJump = true;
    [SerializeField] private LayerMask platformLayers;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugGizmos = true;
    
    private Transform currentPlatform;
    private Vector2 platformVelocity;
    private Rigidbody2D rb;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (IsValidPlatform(collision))
        {
            AttachToPlatform(collision.transform);
        }
    }
    
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (currentPlatform == null && IsValidPlatform(collision))
        {
            AttachToPlatform(collision.transform);
        }
    }
    
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (currentPlatform != null && collision.transform == currentPlatform)
        {
            DetachFromPlatform();
        }
    }
    
    private bool IsValidPlatform(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Platform")) return false;
        
        if (collision.contactCount == 0) return false;
        
        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f)
            {
                return true;
            }
        }
        
        return false;
    }
    
    private void AttachToPlatform(Transform platform)
    {
        if (currentPlatform == platform) return;
        
        currentPlatform = platform;
        transform.SetParent(platform);
        
        MovingPlatform movingPlatform = platform.GetComponent<MovingPlatform>();
        if (movingPlatform != null)
        {
            platformVelocity = movingPlatform.GetVelocity();
        }
        
        Debug.Log($"[RIDER] ✅ Attached to {platform.name}");
    }
    
    private void DetachFromPlatform()
    {
        if (currentPlatform == null) return;
        
        Debug.Log($"[RIDER] ❌ Detached from {currentPlatform.name}");
        
        transform.SetParent(null);
        currentPlatform = null;
        platformVelocity = Vector2.zero;
    }
    
    public void ForceDetach()
    {
        DetachFromPlatform();
    }
    
    public Vector2 GetPlatformVelocity()
    {
        if (currentPlatform != null)
        {
            MovingPlatform movingPlatform = currentPlatform.GetComponent<MovingPlatform>();
            if (movingPlatform != null)
            {
                platformVelocity = movingPlatform.GetVelocity();
            }
        }
        
        return platformVelocity;
    }
    
    public bool IsOnPlatform()
    {
        return currentPlatform != null;
    }
    
    public bool ShouldInheritVelocity()
    {
        return inheritVelocityOnJump && currentPlatform != null;
    }
    
    private void OnDrawGizmos()
    {
        if (!showDebugGizmos || !Application.isPlaying) return;
        
        if (currentPlatform != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, currentPlatform.position);
            
            Gizmos.color = Color.yellow;
            Vector3 velocityEnd = transform.position + (Vector3)platformVelocity * 0.5f;
            Gizmos.DrawLine(transform.position, velocityEnd);
            Gizmos.DrawSphere(velocityEnd, 0.15f);
            
#if UNITY_EDITOR
            UnityEditor.Handles.Label(transform.position + Vector3.up * 2.5f, 
                $"Platform: {currentPlatform.name}\nVelocity: {platformVelocity:F2}");
#endif
        }
    }
}
