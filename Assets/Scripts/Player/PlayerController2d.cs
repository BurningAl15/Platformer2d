using UnityEngine;

public class PlayerController2d : MonoBehaviour
{
    public static PlayerController2d _instance;
    
    [Header("Components")]
    private PlayerMovement movement;
    private PlayerJump jump;
    private PlayerGroundCheck groundCheck;
    private PlayerKnockback knockback;
    private PlayerEffects effects;
    
    [Header("Level End")]
    [SerializeField] private AnimationCurve levelEndCurve;
    [SerializeField] private float levelEndDuration = 1f;
    [SerializeField] private float levelEndGrowFactor = 1f;
    
    private Rigidbody2D rb;
    private float endTimer;
    
    private void Awake()
    {
        _instance = this;
        
        rb = GetComponent<Rigidbody2D>();
        movement = GetComponent<PlayerMovement>();
        jump = GetComponent<PlayerJump>();
        groundCheck = GetComponent<PlayerGroundCheck>();
        knockback = GetComponent<PlayerKnockback>();
        effects = GetComponent<PlayerEffects>();
        
        endTimer = levelEndDuration;
    }
    
    private void OnEnable()
    {
        SubscribeToInput();
        
        if (jump != null)
        {
            jump.OnJumpPerformed += PlayJumpSound;
            jump.OnDoubleJumpPerformed += PlayJumpSound;
        }
    }
    
    private void OnDisable()
    {
        UnsubscribeFromInput();
        
        if (jump != null)
        {
            jump.OnJumpPerformed -= PlayJumpSound;
            jump.OnDoubleJumpPerformed -= PlayJumpSound;
        }
    }
    
    private void SubscribeToInput()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnJumpPressed += HandleJumpPressed;
            InputManager.Instance.OnJumpReleased += HandleJumpReleased;
        }
    }
    
    private void UnsubscribeFromInput()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnJumpPressed -= HandleJumpPressed;
            InputManager.Instance.OnJumpReleased -= HandleJumpReleased;
        }
    }
    
    private void Update()
    {
        if (!LevelManager._instance.stopGame && !PauseMenu._instance.isPaused)
        {
            GatherInput();
        }
        else if (LevelManager._instance.stopGame)
        {
            ProcessLevelEnd();
        }
    }
    
    private void FixedUpdate()
    {
        if (!LevelManager._instance.stopGame && !PauseMenu._instance.isPaused)
        {
            if (knockback != null && knockback.IsInKnockback)
            {
                knockback.ProcessKnockback();
            }
            else
            {
                if (movement != null)
                    movement.ProcessMovement();
                
                if (jump != null)
                    jump.ProcessJump();
            }
        }
    }
    
    private void GatherInput()
    {
        if (InputManager.Instance != null && movement != null)
        {
            movement.SetInput(InputManager.Instance.MoveInput);
        }
    }
    
    private void HandleJumpPressed()
    {
        if (!LevelManager._instance.stopGame && !PauseMenu._instance.isPaused && jump != null)
        {
            jump.RequestJump();
        }
    }
    
    private void HandleJumpReleased()
    {
        if (jump != null)
        {
            jump.CutJump();
        }
    }
    
    private void PlayJumpSound()
    {
        if (AudioMixerManager._instance != null)
        {
            AudioMixerManager._instance.CallSFX(SFXType.Player_Jump);
        }
    }
    
    private void ProcessLevelEnd()
    {
        if (endTimer > 0)
        {
            float animValue = levelEndCurve.Evaluate(endTimer);
            rb.linearVelocity = new Vector2(rb.linearVelocity.x * animValue, rb.linearVelocity.y);
            endTimer -= Time.deltaTime * levelEndGrowFactor;
        }
    }
    
    public void KnockBack()
    {
        if (knockback != null)
        {
            knockback.ApplyKnockback();
        }
    }
    
    public void Bounce()
    {
        if (knockback != null)
        {
            knockback.ApplyBounce();
        }
    }
    
    public void Bounce(float multiplier)
    {
        if (knockback != null)
        {
            knockback.ApplyBounce(multiplier);
        }
    }
}
