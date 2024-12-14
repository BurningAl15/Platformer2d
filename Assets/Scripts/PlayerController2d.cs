using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerController2d : MonoBehaviour
{
    public static PlayerController2d _instance;

    [Header("Components")]
    [SerializeField] private Rigidbody2D _rgb;
    [SerializeField] private Animator _anim;
    [SerializeField] private SpriteRenderer _spriteRenderer;

    private int direction = 1;
    private int prevDirection = 1;

    [Header("Movement Variables")]
    [SerializeField] private float movementSpeed;

    [Header("Jump Variables")]
    [SerializeField] private float jumpForce;
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private float radius;
    [SerializeField] private LayerMask whatIsGround;
    private bool isGrounded = false;
    private bool canDoubleJump = true;

    [Header("Knockback Effect Variables")]
    public float knockBackLength;
    public Vector2 knockBackForce;
    public float knockBackCounter;

    [Header("Enemy Effect Variables")]
    [SerializeField] private float bounceForce;

    [SerializeField] private AnimationCurve animationCurve;
    [SerializeField] private float endTimer = 1f;
    [SerializeField] private float growFactor;

    [SerializeField] private ParticleSystem dust;
    [SerializeField] private ParticleSystem jumpDust;
    [SerializeField] private ParticleSystem fallDust;

    bool hasCheckedGround = false;
    private static readonly int MoveSpeed = Animator.StringToHash("moveSpeed");
    private static readonly int IsGrounded = Animator.StringToHash("isGrounded");
    private static readonly int Hurt = Animator.StringToHash("Hurt");

    private MovingPlatform currentPlatformScript = null;

    private void Awake()
    {
        _rgb = GetComponent<Rigidbody2D>();
        _anim = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _instance = this;
    }

    private void FixedUpdate()
    {
        if (!LevelManager._instance.stopGame && !PauseMenu._instance.isPaused)
        {
            if (knockBackCounter <= 0)
            {
                GroundCheck();

                // Base horizontal input
                float horizontalInput = Input.GetAxis("Horizontal");
                float horizontalVelocity = horizontalInput * movementSpeed;

                // Start with player's desired velocity
                Vector2 finalVelocity = new Vector2(horizontalVelocity, _rgb.velocity.y);

                // If on a moving platform, add platform's full velocity
                if (isGrounded && currentPlatformScript != null)
                {
                    Vector2 platformVel = currentPlatformScript.GetVelocity();
                    // Add platform's velocity so player moves with it
                    finalVelocity += platformVel * Time.fixedDeltaTime / Time.fixedDeltaTime;
                    // Time multiplications cancel out, included for clarity.
                    // Essentially: finalVelocity += platformVel;

                    // This ensures vertical movement too, so we don't slip off vertically
                    finalVelocity.y = platformVel.y; 

                    // The player now has platform velocity plus input horizontally.
                    // Player is fully carried if no input, and can override horizontally by input.
                    // Player is also carried vertically by platformVel.y.
                }

                _rgb.velocity = finalVelocity;

                // Now handle jumps after setting the base velocity
                if (Input.GetButtonDown("Jump"))
                {
                    Jump();
                }

                // Handle sprite flipping and animations
                HandleSpriteFlip(horizontalInput);
            }
            else
            {
                knockBackCounter -= Time.fixedDeltaTime;
                _rgb.velocity = new Vector2(knockBackForce.x * -direction, _rgb.velocity.y);
            }
        }
        else
        {
            GroundCheck();
            EndingLevel();
        }
    }

    void EndingLevel()
    {
        if (endTimer > 0)
        {
            float animValue = animationCurve.Evaluate(endTimer);
            _rgb.velocity = new Vector2(_rgb.velocity.x * animValue, _rgb.velocity.y);
            endTimer -= Time.deltaTime * growFactor;
            _anim.SetFloat(MoveSpeed, Mathf.Abs(_rgb.velocity.x));
        }
    }

    void GroundCheck()
    {
        Collider2D groundCollider = Physics2D.OverlapCircle(groundCheckPoint.position, radius, whatIsGround);
        bool wasGrounded = isGrounded;
        isGrounded = (groundCollider != null);

        if (isGrounded)
        {
            if (groundCollider.CompareTag("MovingPlatform"))
            {
                currentPlatformScript = groundCollider.GetComponentInParent<MovingPlatform>();
            }
            else
            {
                currentPlatformScript = null;
            }

            if (hasCheckedGround)
            {
                fallDust.Play();
                hasCheckedGround = false;
            }
            canDoubleJump = true;
        }
        else
        {
            currentPlatformScript = null;
            hasCheckedGround = true;
        }

        _anim.SetBool(IsGrounded, isGrounded);
    }

    void Jump()
    {
        if (isGrounded)
        {
            // On jump, break from platform vertical sync by setting jumpForce
            _rgb.velocity = new Vector2(_rgb.velocity.x, jumpForce);
            AudioMixerManager._instance.CallSFX(SFXType.Player_Jump);
        }
        else if (canDoubleJump)
        {
            CreateDust(1);
            _rgb.velocity = new Vector2(_rgb.velocity.x, jumpForce);
            AudioMixerManager._instance.CallSFX(SFXType.Player_Jump);
            canDoubleJump = false;
        }
    }

    public void KnockBack()
    {
        knockBackCounter = knockBackLength;
        _rgb.velocity = new Vector2(0, knockBackForce.y);
        _anim.SetTrigger(Hurt);
    }

    void HandleSpriteFlip(float horizontalInput)
    {
        if (horizontalInput > 0 && direction != 1)
        {
            direction = 1;
            FlipSprite();
        }
        else if (horizontalInput < 0 && direction != -1)
        {
            direction = -1;
            FlipSprite();
        }

        _anim.SetFloat(MoveSpeed, Mathf.Abs(horizontalInput * movementSpeed));
    }

    void FlipSprite()
    {
        transform.eulerAngles = new Vector3(0, direction == 1 ? 0 : 180, 0);
        if (isGrounded)
            CreateDust(0);
    }

    public void Bounce()
    {
        _rgb.velocity = new Vector2(_rgb.velocity.x, bounceForce);
        AudioMixerManager._instance.CallSFX(SFXType.Player_Jump);
    }

    public void Bounce(float _bounceFactor)
    {
        float bounceXfactor = bounceForce * _bounceFactor;
        _rgb.velocity = new Vector2(_rgb.velocity.x, bounceXfactor);
        AudioMixerManager._instance.CallSFX(SFXType.Player_Jump);
    }

    void CreateDust(int dustType)
    {
        if (dustType == 0)
            dust.Play();
        else
            jumpDust.Play();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawSphere(groundCheckPoint.position, radius);
    }
}
