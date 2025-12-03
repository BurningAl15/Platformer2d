using UnityEngine;

public class PlayerEffects : MonoBehaviour
{
    [Header("Particle Systems")]
    [SerializeField] private ParticleSystem dustParticles;
    [SerializeField] private ParticleSystem jumpDustParticles;
    [SerializeField] private ParticleSystem fallDustParticles;
    
    private PlayerMovement movement;
    private PlayerGroundCheck groundCheck;
    private PlayerJump jump;
    
    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        groundCheck = GetComponent<PlayerGroundCheck>();
        jump = GetComponent<PlayerJump>();
    }
    
    private void OnEnable()
    {
        if (movement != null)
            movement.OnFlip += PlayDustEffect;
        
        if (groundCheck != null)
            groundCheck.OnLanded += PlayFallDust;
        
        if (jump != null)
            jump.OnDoubleJumpPerformed += PlayJumpDust;
    }
    
    private void OnDisable()
    {
        if (movement != null)
            movement.OnFlip -= PlayDustEffect;
        
        if (groundCheck != null)
            groundCheck.OnLanded -= PlayFallDust;
        
        if (jump != null)
            jump.OnDoubleJumpPerformed -= PlayJumpDust;
    }
    
    private void PlayDustEffect()
    {
        if (groundCheck != null && groundCheck.IsGrounded && dustParticles != null)
        {
            dustParticles.Play();
        }
    }
    
    private void PlayJumpDust()
    {
        if (jumpDustParticles != null)
        {
            jumpDustParticles.Play();
        }
    }
    
    private void PlayFallDust()
    {
        if (fallDustParticles != null)
        {
            fallDustParticles.Play();
        }
    }
}