using System.Collections;
using UnityEngine;

public class PlayerController2d : MonoBehaviour
{
    public static PlayerController2d _instance;
    private static readonly int MoveSpeed = Animator.StringToHash("moveSpeed");
    private static readonly int IsGrounded = Animator.StringToHash("isGrounded");
    private static readonly int Hurt = Animator.StringToHash("Hurt");

    [Header("Components")]
    [SerializeField] private Rigidbody2D _rgb;
    [SerializeField] private Animator _anim;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private PlatformRider platformRider;
    
    private int direction = 1;
    private int prevDirection = 1;

    [Header("Movement Variables")]
    [SerializeField] private float movementSpeed = 7f;
    [SerializeField] private float acceleration = 50f;
    [SerializeField] private float deceleration = 50f;
    [SerializeField] private float airAcceleration = 30f;
    [SerializeField] private float airDeceleration = 30f;

    [Header("Jump Variables")]
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float jumpCutMultiplier = 0.5f;
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private float radius = 0.2f;
    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private float coyoteTime = 0.15f;
    [SerializeField] private float jumpBufferTime = 0.15f;
    
    private bool isGrounded = false;
    private bool canDoubleJump = true;
    private float lastGroundedTime = 0f;
    private float lastJumpPressedTime = 0f;
    private bool isJumping = false;

    [Header("Knockback Effect Variables")]
    public float knockBackLength = 0.5f;
    public Vector2 knockBackForce = new Vector2(5f, 5f);
    public float knockBackCounter = 0f;

    [Header("Enemy Effect Variables")]
    [SerializeField] private float bounceForce = 10f;
    [SerializeField] private AnimationCurve animationCurve;
    [SerializeField] private float endTimer = 1f;
    [SerializeField] private float growFactor = 1f;

    [Header("Particles")]
    [SerializeField] private ParticleSystem dust;
    [SerializeField] private ParticleSystem jumpDust;
    [SerializeField] private ParticleSystem fallDust;
    
    private bool hasCheckedGround = false;
    private Vector2 moveInput;
    private bool jumpInputPressed = false;
    private bool jumpInputReleased = false;
    
    private void Awake()
    {
        _rgb = GetComponent<Rigidbody2D>();
        _anim = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        platformRider = GetComponent<PlatformRider>();
        _instance = this;
    }

    private void OnEnable()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnJumpPressed += HandleJumpPressed;
            InputManager.Instance.OnJumpReleased += HandleJumpReleased;
        }
    }

    private void OnDisable()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnJumpPressed -= HandleJumpPressed;
            InputManager.Instance.OnJumpReleased -= HandleJumpReleased;
        }
    }

    void HandleJumpReleased()
    {
        if (isJumping && _rgb.linearVelocity.y > 0)
        {
            _rgb.linearVelocity = new Vector2(_rgb.linearVelocity.x, _rgb.linearVelocity.y * jumpCutMultiplier);
            isJumping = false;
        }
    }

    void Update()
    {
        if (!LevelManager._instance.stopGame && !PauseMenu._instance.isPaused)
        {
            GatherInput();
            UpdateTimers();
        }
        else if (LevelManager._instance.stopGame)
        {
            EndingLevel();
        }
    
        UpdateAnimations();
    }

    void FixedUpdate()
    {
        GroundCheck();

        if (!LevelManager._instance.stopGame && !PauseMenu._instance.isPaused)
        {
            if (knockBackCounter <= 0)
            {
                HandleMovement();
                HandleJump();
            }
            else
            {
                ApplyKnockback();
            }
        }
    }

    #region Input

    void GatherInput()
    {
        if (InputManager.Instance != null)
        {
            moveInput = InputManager.Instance.MoveInput;
        }
        else
        {
            moveInput = Vector2.zero;
        }
    }

    void HandleJumpPressed()
    {
        if (!LevelManager._instance.stopGame && !PauseMenu._instance.isPaused)
        {
            lastJumpPressedTime = jumpBufferTime;
        }
    }

    #endregion

    #region Timers

    void UpdateTimers()
    {
        lastGroundedTime -= Time.deltaTime;
        lastJumpPressedTime -= Time.deltaTime;
        knockBackCounter -= Time.deltaTime;
    }

    #endregion

    #region Ground Check

    void GroundCheck()
    {
        bool wasGrounded = isGrounded;
        isGrounded = Physics2D.OverlapCircle(groundCheckPoint.position, radius, whatIsGround);

        if (isGrounded)
        {
            lastGroundedTime = coyoteTime;
            canDoubleJump = true;
            isJumping = false;

            if (!wasGrounded && hasCheckedGround)
            {
                fallDust.Play();
                hasCheckedGround = false;
            }
        }
        else
        {
            hasCheckedGround = true;
        }
    }

    #endregion

    #region Movement

    void HandleMovement()
    {
        float horizontalInput = moveInput.x;
        float targetSpeed = horizontalInput * movementSpeed;
    
        float currentVelocityX = _rgb.linearVelocity.x;

        float accelRate;
        if (Mathf.Abs(targetSpeed) > 0.01f)
        {
            accelRate = isGrounded ? acceleration : airAcceleration;
        }
        else
        {
            accelRate = isGrounded ? deceleration : airDeceleration;
        }

        float speedDiff = targetSpeed - currentVelocityX;
        float movement = speedDiff * accelRate * Time.fixedDeltaTime;

        float newVelocityX = currentVelocityX + movement;
        float newVelocityY = _rgb.linearVelocity.y;

        _rgb.linearVelocity = new Vector2(newVelocityX, newVelocityY);

        HandleFlip(horizontalInput);
    }

    void HandleFlip(float horizontalInput)
    {
        if (horizontalInput > 0.01f)
        {
            direction = 1;
        }
        else if (horizontalInput < -0.01f)
        {
            direction = -1;
        }

        if (prevDirection != direction)
        {
            FlipSprite();
            prevDirection = direction;
        }
    }

    void FlipSprite()
    {
        transform.eulerAngles = new Vector3(0, direction == 1 ? 0 : 180, 0);
        if (isGrounded)
            CreateDust(0);
    }
    
    #endregion

    #region Jump

    void HandleJump()
    {
        if (lastJumpPressedTime > 0)
        {
            if (lastGroundedTime > 0)
            {
                PerformJump();
                lastJumpPressedTime = 0;
                lastGroundedTime = 0;
            }
            else if (canDoubleJump && !isGrounded)
            {
                PerformDoubleJump();
                lastJumpPressedTime = 0;
            }
        }
    }

    void PerformJump()
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

        _rgb.linearVelocity = new Vector2(_rgb.linearVelocity.x + inheritedVelocity.x, jumpVelocity);
    
        isJumping = true;
        AudioMixerManager._instance.CallSFX(SFXType.Player_Jump);
    }

    void PerformDoubleJump()
    {
        CreateDust(1);
        _rgb.linearVelocity = new Vector2(_rgb.linearVelocity.x, jumpForce);
        isJumping = true;
        canDoubleJump = false;
        AudioMixerManager._instance.CallSFX(SFXType.Player_Jump);
    }

    #endregion

    #region Knockback & Effects

    void ApplyKnockback()
    {
        _rgb.linearVelocity = new Vector2(knockBackForce.x * -direction, _rgb.linearVelocity.y);
    }

    public void KnockBack()
    {
        knockBackCounter = knockBackLength;
        _rgb.linearVelocity = new Vector2(0, knockBackForce.y);
        _anim.SetTrigger(Hurt);
    }

    public void Bounce()
    {
        _rgb.linearVelocity = new Vector2(_rgb.linearVelocity.x, bounceForce);
        AudioMixerManager._instance.CallSFX(SFXType.Player_Jump);
    }
    
    public void Bounce(float _bounceFactor)
    {
        float bounceYfactor = bounceForce * _bounceFactor;
        _rgb.linearVelocity = new Vector2(_rgb.linearVelocity.x, bounceYfactor);
        AudioMixerManager._instance.CallSFX(SFXType.Player_Jump);
    }

    #endregion

    #region Level End

    void EndingLevel()
    {
        if (endTimer > 0)
        {
            GroundCheck();
            float animValue = animationCurve.Evaluate(endTimer);
            _rgb.linearVelocity = new Vector2(_rgb.linearVelocity.x * animValue, _rgb.linearVelocity.y);
            endTimer -= Time.deltaTime * growFactor;
        }
    }

    #endregion

    #region Animations & Particles

    void UpdateAnimations()
    {
        float displaySpeed = Mathf.Abs(moveInput.x * movementSpeed);
    
        _anim.SetFloat(MoveSpeed, displaySpeed);
        _anim.SetBool(IsGrounded, isGrounded);
    }

    void CreateDust(int dustType)
    {
        if (dustType == 0)
            dust.Play();
        else
            jumpDust.Play();
    }
    
    #endregion

    private void OnDrawGizmos()
    {
        if (groundCheckPoint == null) return;
    
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(groundCheckPoint.position, radius);
    }
}
