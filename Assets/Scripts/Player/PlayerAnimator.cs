using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimator : MonoBehaviour
{
    private static readonly int MoveSpeedHash = Animator.StringToHash("moveSpeed");
    private static readonly int IsGroundedHash = Animator.StringToHash("isGrounded");
    private static readonly int HurtHash = Animator.StringToHash("Hurt");
    
    private Animator animator;
    private PlayerMovement movement;
    private PlayerGroundCheck groundCheck;
    
    private void Awake()
    {
        animator = GetComponent<Animator>();
        movement = GetComponent<PlayerMovement>();
        groundCheck = GetComponent<PlayerGroundCheck>();
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
    }
    
    public void TriggerHurt()
    {
        animator.SetTrigger(HurtHash);
    }
}