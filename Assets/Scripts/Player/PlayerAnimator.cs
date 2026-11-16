using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimator : MonoBehaviour
{
    private static readonly int MoveSpeedHash = Animator.StringToHash("moveSpeed");
    private static readonly int IsGroundedHash = Animator.StringToHash("isGrounded");
    private static readonly int VerticalVelocityHash = Animator.StringToHash("verticalVelocity");
    private static readonly int HurtHash = Animator.StringToHash("Hurt");
    private static readonly int DoubleJumpHash = Animator.StringToHash("DoubleJump");
    
    private Animator animator;
    private PlayerMovement movement;
    private PlayerGroundCheck groundCheck;
    private PlayerJump jump;
    private Rigidbody2D body;
    
    private void Awake()
    {
        animator = GetComponent<Animator>();
        movement = GetComponent<PlayerMovement>();
        groundCheck = GetComponent<PlayerGroundCheck>();
        jump = GetComponent<PlayerJump>();
        body = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        if (jump != null)
            jump.OnDoubleJumpPerformed += PlayDoubleJumpAnimation;
    }

    private void OnDisable()
    {
        if (jump != null)
            jump.OnDoubleJumpPerformed -= PlayDoubleJumpAnimation;
    }
    
    private void Update()
    {
        UpdateAnimations();
    }
    
    private void UpdateAnimations()
    {
        if (movement != null)
        {
            animator.SetFloat(MoveSpeedHash, movement.GetDisplaySpeed());
        }
        
        if (groundCheck != null)
        {
            animator.SetBool(IsGroundedHash, groundCheck.IsGrounded);
        }

        if (body != null)
            animator.SetFloat(VerticalVelocityHash, body.linearVelocity.y);
    }

    private void PlayDoubleJumpAnimation()
    {
        animator.ResetTrigger(DoubleJumpHash);
        animator.SetTrigger(DoubleJumpHash);
    }
    
    public void TriggerHurt()
    {
        animator.SetTrigger(HurtHash);
    }
}
