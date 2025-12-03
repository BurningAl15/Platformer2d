using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerKnockback : MonoBehaviour
{
    [Header("Knockback Settings")]
    [SerializeField] private float knockbackDuration = 0.5f;
    [SerializeField] private Vector2 knockbackForce = new Vector2(5f, 5f);
    
    [Header("Bounce Settings")]
    [SerializeField] private float bounceForce = 10f;
    
    public bool IsInKnockback { get; private set; }
    
    private Rigidbody2D rb;
    private PlayerMovement movement;
    private PlayerAnimator playerAnimator;
    private float knockbackCounter;
    
    public event System.Action OnKnockbackStart;
    public event System.Action OnKnockbackEnd;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        movement = GetComponent<PlayerMovement>();
        playerAnimator = GetComponent<PlayerAnimator>();
    }
    
    private void Update()
    {
        UpdateKnockback();
    }
    
    private void UpdateKnockback()
    {
        if (knockbackCounter > 0)
        {
            knockbackCounter -= Time.deltaTime;
            
            if (knockbackCounter <= 0)
            {
                IsInKnockback = false;
                OnKnockbackEnd?.Invoke();
            }
        }
    }
    
    public void ApplyKnockback()
    {
        IsInKnockback = true;
        knockbackCounter = knockbackDuration;
        
        int direction = movement != null ? movement.Direction : 1;
        rb.linearVelocity = new Vector2(knockbackForce.x * -direction, knockbackForce.y);
        
        if (playerAnimator != null)
        {
            playerAnimator.TriggerHurt();
        }
        
        OnKnockbackStart?.Invoke();
    }
    
    public void ProcessKnockback()
    {
        if (!IsInKnockback) return;
        
        int direction = movement != null ? movement.Direction : 1;
        rb.linearVelocity = new Vector2(knockbackForce.x * -direction, rb.linearVelocity.y);
    }
    
    public void ApplyBounce(float multiplier = 1f)
    {
        float bounceY = bounceForce * multiplier;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, bounceY);
    }
}
