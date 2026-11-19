using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[DefaultExecutionOrder(100)]
public class PlatformRider : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool inheritVelocityOnJump = true;
    [SerializeField] private float attachThreshold = 0.5f;
    [SerializeField] private float maxAttachDistance = 2f;
    [Tooltip("Upward speed relative to the platform above which a touching player is leaving it (a jump) and must not re-attach.")]
    [SerializeField] private float jumpReleaseSpeed = 1.5f;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugGizmos = true;
    [SerializeField] private bool showDebugLogs = true;
    
    private MovingPlatform currentPlatform;
    private Vector2 platformVelocity;
    private Rigidbody2D rb;
    private bool isAttached;
    private bool wasGroundedLastFrame;
    
    public bool IsOnPlatform => currentPlatform != null && isAttached;

    /// <summary>Vertical speed of the platform being ridden, or zero.</summary>
    public float PlatformVerticalSpeed => IsOnPlatform ? currentPlatform.GetVelocity().y : 0f;
    
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

        // Right after a jump the feet can still touch the platform for a step;
        // re-attaching then would flatten the jump to the platform's speed.
        if (rb.linearVelocity.y - platform.GetVelocity().y > jumpReleaseSpeed) return;
        
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
        
        float platformVerticalSpeed = currentPlatform.GetVelocity().y;
        float relativeVerticalSpeed = rb.linearVelocity.y - platformVerticalSpeed;
        if (!wasGroundedLastFrame && relativeVerticalSpeed < -1f)
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
        Vector2 platformStepVelocity = platformDelta / Time.fixedDeltaTime;

        // Compose platform carry with the player's velocity. MovePosition on a
        // dynamic player body replaces its motion for this physics step, which
        // prevented PlayerMovement input from moving the player while riding.
        Vector2 velocity = rb.linearVelocity;
        velocity.x += platformStepVelocity.x;

        // Vertically, the rider moves exactly with the platform. Adding only the
        // downward carry left the momentum gained while rising, so when the
        // platform reversed the player flew off it for a moment and the fall
        // animation played. Jumps detach first (PlayerJump.PerformJump), so they
        // are never flattened here.
        velocity.y = platformStepVelocity.y;
        rb.linearVelocity = velocity;

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
