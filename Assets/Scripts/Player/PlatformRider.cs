using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[DefaultExecutionOrder(100)]
public class PlatformRider : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool inheritVelocityOnJump = true;
    [SerializeField] private float attachThreshold = 0.5f;
    [SerializeField] private float maxAttachDistance = 2f;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugGizmos = true;
    [SerializeField] private bool showDebugLogs = true;
    
    private MovingPlatform currentPlatform;
    private Vector2 platformVelocity;
    private Rigidbody2D rb;
    private bool isAttached;
    private bool wasGroundedLastFrame;
    
    public bool IsOnPlatform => currentPlatform != null && isAttached;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    
    private void LateFixedUpdate()
    {
        if (isAttached && currentPlatform != null)
        {
            CheckAttachmentValidity();
            
            if (isAttached)
            {
                ApplyPlatformMovement();
            }
        }
    }
    
    private void FixedUpdate()
    {
        LateFixedUpdate();
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryAttach(collision);
    }
    
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!isAttached)
        {
            TryAttach(collision);
        }
        else if (IsCollisionWithCurrentPlatform(collision))
        {
            wasGroundedLastFrame = true;
        }
    }
    
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (IsCollisionWithCurrentPlatform(collision))
        {
            Detach();
        }
    }

    private bool IsCollisionWithCurrentPlatform(Collision2D collision)
    {
        return currentPlatform != null &&
               collision.gameObject.GetComponentInParent<MovingPlatform>() == currentPlatform;
    }
    
    private void TryAttach(Collision2D collision)
    {
        if (isAttached) return;
        
        if (!collision.gameObject.CompareTag("Platform")) return;
        
        if (!IsStandingOn(collision)) return;
        
        MovingPlatform platform = collision.gameObject.GetComponentInParent<MovingPlatform>();
        if (platform == null) return;
        
        Attach(platform);
    }
    
    private bool IsStandingOn(Collision2D collision)
    {
        if (collision.contactCount == 0) return false;
        
        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y >= attachThreshold)
            {
                return true;
            }
        }
        
        return false;
    }
    
    private void CheckAttachmentValidity()
    {
        if (currentPlatform == null)
        {
            Detach();
            return;
        }
        
        float distance = Vector2.Distance(rb.position, currentPlatform.GetPhysicsPosition());
        
        if (distance > maxAttachDistance)
        {
            if (showDebugLogs)
            {
                Debug.Log($"[RIDER] ⚠️ Too far from platform ({distance:F2}m), detaching");
            }
            Detach();
            return;
        }
        
        if (!wasGroundedLastFrame && rb.linearVelocity.y < -1f)
        {
            if (showDebugLogs)
            {
                Debug.Log($"[RIDER] ⚠️ Falling off platform, detaching");
            }
            Detach();
            return;
        }
        
        wasGroundedLastFrame = false;
    }
    
    private void Attach(MovingPlatform platform)
    {
        currentPlatform = platform;
        isAttached = true;
        wasGroundedLastFrame = true;
        
        platformVelocity = currentPlatform.GetVelocity();
        
        if (showDebugLogs)
        {
            Debug.Log($"[RIDER] ✅ Attached to {platform.name}");
        }
    }
    
    private void Detach()
    {
        if (!isAttached) return;
        
        if (showDebugLogs && currentPlatform != null)
        {
            Debug.Log($"[RIDER] ❌ Detached from {currentPlatform.name}");
        }
        
        isAttached = false;
        currentPlatform = null;
        platformVelocity = Vector2.zero;
        wasGroundedLastFrame = false;
    }
    
    private void ApplyPlatformMovement()
    {
        if (currentPlatform == null)
        {
            Detach();
            return;
        }
        
        Vector2 platformDelta = currentPlatform.GetFixedStepDelta();

        // The solver keeps contact with vertical platforms, but this project’s
        // horizontal one-way platforms do not carry the player laterally. Apply
        // X explicitly; keep upward Y to the solver to avoid the prior jitter.
        // Descending platforms also need explicit Y because they cannot pull a
        // dynamic passenger down with them.
        Vector2 carryDelta = new Vector2(platformDelta.x, Mathf.Min(0f, platformDelta.y));
        if (carryDelta.sqrMagnitude > 0.000001f)
        {
            rb.MovePosition(rb.position + carryDelta);
        }
        
        platformVelocity = currentPlatform.GetVelocity();
    }
    
    public void ForceDetach()
    {
        Detach();
    }
    
    public Vector2 GetPlatformVelocity()
    {
        return platformVelocity;
    }
    
    public bool ShouldInheritVelocity()
    {
        return inheritVelocityOnJump && isAttached;
    }
    
    private void OnDrawGizmos()
    {
        if (!showDebugGizmos || !Application.isPlaying) return;
        
        if (isAttached && currentPlatform != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, currentPlatform.transform.position);
            Gizmos.DrawWireSphere(transform.position, 0.3f);
            
            Gizmos.color = Color.yellow;
            Vector3 velocityEnd = transform.position + (Vector3)platformVelocity * 0.5f;
            Gizmos.DrawLine(transform.position, velocityEnd);
            Gizmos.DrawSphere(velocityEnd, 0.15f);
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, maxAttachDistance);
            
#if UNITY_EDITOR
            UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, 
                $"🟢 {currentPlatform.name}\nVel: {platformVelocity:F2}\nGrounded: {wasGroundedLastFrame}");
#endif
        }
    }
}
