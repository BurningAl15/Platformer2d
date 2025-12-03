using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerJump : MonoBehaviour
{
    [Header("Jump Settings")]
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float jumpCutMultiplier = 0.5f;
    [SerializeField] private float jumpBufferTime = 0.15f;
    [SerializeField] private bool allowDoubleJump = true;
    
    public bool IsJumping { get; private set; }
    
    private Rigidbody2D rb;
    private PlayerGroundCheck groundCheck;
    private PlatformRider platformRider;
    
    private float lastJumpPressedTime;
    private bool canDoubleJump;
    
    public event System.Action OnJumpPerformed;
    public event System.Action OnDoubleJumpPerformed;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        groundCheck = GetComponent<PlayerGroundCheck>();
        platformRider = GetComponent<PlatformRider>();
    }
    
    private void OnEnable()
    {
        if (groundCheck != null)
        {
            groundCheck.OnLanded += HandleLanded;
        }
    }
    
    private void OnDisable()
    {
        if (groundCheck != null)
        {
            groundCheck.OnLanded -= HandleLanded;
        }
    }
    
    private void Update()
    {
        UpdateJumpBuffer();
    }
    
    private void UpdateJumpBuffer()
    {
        if (lastJumpPressedTime > 0)
        {
            lastJumpPressedTime -= Time.deltaTime;
        }
    }
    
    private void HandleLanded()
    {
        canDoubleJump = allowDoubleJump;
        IsJumping = false;
    }
    
    public void RequestJump()
    {
        lastJumpPressedTime = jumpBufferTime;
    }
    
    public void ProcessJump()
    {
        if (lastJumpPressedTime <= 0) return;
        
        if (groundCheck.HasCoyoteTime())
        {
            PerformJump();
            lastJumpPressedTime = 0;
        }
        else if (canDoubleJump && !groundCheck.IsGrounded)
        {
            PerformDoubleJump();
            lastJumpPressedTime = 0;
        }
    }
    
    private void PerformJump()
    {
        float jumpVelocity = jumpForce;
        Vector2 inheritedVelocity = Vector2.zero;
        
        if (platformRider != null && platformRider.ShouldInheritVelocity())
        {
            inheritedVelocity = platformRider.GetPlatformVelocity();
            
            if (inheritedVelocity.y > 0)
            {
                jumpVelocity += inheritedVelocity.y;
            }
            
            platformRider.ForceDetach();
        }
        
        rb.linearVelocity = new Vector2(rb.linearVelocity.x + inheritedVelocity.x, jumpVelocity);
        IsJumping = true;
        
        OnJumpPerformed?.Invoke();
    }
    
    private void PerformDoubleJump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        IsJumping = true;
        canDoubleJump = false;
        
        OnDoubleJumpPerformed?.Invoke();
    }
    
    public void CutJump()
    {
        if (IsJumping && rb.linearVelocity.y > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);
            IsJumping = false;
        }
    }
}
