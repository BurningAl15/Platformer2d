using UnityEngine;

public class PlayerGroundCheck : MonoBehaviour
{
    [Header("Ground Detection")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private float checkRadius = 0.2f;
    [SerializeField] private LayerMask groundLayers;
    
    [Header("Coyote Time")]
    [SerializeField] private float coyoteTime = 0.15f;
    
    public bool IsGrounded { get; private set; }
    public float LastGroundedTime { get; private set; }
    
    private bool hasCheckedGround;
    
    public event System.Action OnLanded;
    
    private void FixedUpdate()
    {
        CheckGround();
    }
    
    private void Update()
    {
        UpdateCoyoteTime();
    }
    
    private void CheckGround()
    {
        bool wasGrounded = IsGrounded;
        IsGrounded = Physics2D.OverlapCircle(groundCheckPoint.position, checkRadius, groundLayers);
        
        if (IsGrounded)
        {
            LastGroundedTime = coyoteTime;
            
            if (!wasGrounded && hasCheckedGround)
            {
                OnLanded?.Invoke();
                hasCheckedGround = false;
            }
        }
        else
        {
            hasCheckedGround = true;
        }
    }
    
    private void UpdateCoyoteTime()
    {
        if (LastGroundedTime > 0)
        {
            LastGroundedTime -= Time.deltaTime;
        }
    }
    
    public bool HasCoyoteTime()
    {
        return LastGroundedTime > 0;
    }
    
    private void OnDrawGizmos()
    {
        if (groundCheckPoint == null) return;
        
        Gizmos.color = IsGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(groundCheckPoint.position, checkRadius);
    }
}