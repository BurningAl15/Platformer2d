using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float movementSpeed = 7f;
    [SerializeField] private float acceleration = 50f;
    [SerializeField] private float deceleration = 50f;
    [SerializeField] private float airAcceleration = 30f;
    [SerializeField] private float airDeceleration = 30f;
    
    [Header("Flip Settings")]
    [SerializeField] private bool flipUsingScale = false;
    
    public int Direction { get; private set; } = 1;
    public float HorizontalInput { get; private set; }
    public bool IsMoving => Mathf.Abs(HorizontalInput) > 0.01f;
    
    private Rigidbody2D rb;
    private PlayerGroundCheck groundCheck;
    private int prevDirection = 1;
    
    public event System.Action OnFlip;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        groundCheck = GetComponent<PlayerGroundCheck>();
    }
    
    public void SetInput(Vector2 moveInput)
    {
        HorizontalInput = moveInput.x;
    }
    
    public void ProcessMovement()
    {
        ApplyMovement();
        HandleFlip();
    }
    
    private void ApplyMovement()
    {
        float targetSpeed = HorizontalInput * movementSpeed;
        float currentVelocityX = rb.linearVelocity.x;
        
        bool isGrounded = groundCheck != null && groundCheck.IsGrounded;
        
        float accelRate = Mathf.Abs(targetSpeed) > 0.01f
            ? (isGrounded ? acceleration : airAcceleration)
            : (isGrounded ? deceleration : airDeceleration);
        
        float speedDiff = targetSpeed - currentVelocityX;
        float movement = speedDiff * accelRate * Time.fixedDeltaTime;
        
        rb.linearVelocity = new Vector2(currentVelocityX + movement, rb.linearVelocity.y);
    }
    
    private void HandleFlip()
    {
        if (HorizontalInput > 0.01f)
        {
            Direction = 1;
        }
        else if (HorizontalInput < -0.01f)
        {
            Direction = -1;
        }
        
        if (prevDirection != Direction)
        {
            FlipSprite();
            prevDirection = Direction;
        }
    }
    
    private void FlipSprite()
    {
        if (flipUsingScale)
        {
            Vector3 scale = transform.localScale;
            scale.x = Direction;
            transform.localScale = scale;
        }
        else
        {
            transform.eulerAngles = new Vector3(0, Direction == 1 ? 0 : 180, 0);
        }
        
        OnFlip?.Invoke();
    }
    
    public float GetDisplaySpeed()
    {
        return Mathf.Abs(HorizontalInput * movementSpeed);
    }
}
