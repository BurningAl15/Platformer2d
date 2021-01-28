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

    [Header("Enemy Effect Variabels")] 
    [SerializeField] private float bounceForce;

    [SerializeField] private AnimationCurve animationCurve;
    [SerializeField] private float endTimer = 1f;
    
    
    private void Awake()
    {
        _rgb = GetComponent<Rigidbody2D>();
        _anim = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();

        _instance = this;
    }

    void Update()
    {
        if (!LevelManager._instance.stopGame)
        {
            if (!PauseMenu._instance.isPaused)
            {
                if (knockBackCounter <= 0)
                {
                    //Horizontal Movement
                    Movement();

                    //Flipping sprite by direction
                    FlipSprite();

                    //Checking ground and Jump
                    GroundCheck();
                    if (Input.GetButtonDown("Jump"))
                        Jump();   
                }
                else
                {
                    knockBackCounter -= Time.deltaTime;
                    _rgb.velocity = new Vector2(knockBackForce.x * -direction, _rgb.velocity.y);
                }
            }
        }
        else
        {
            if (endTimer > 0)
            {
                float animValue = animationCurve.Evaluate(endTimer);
                _rgb.velocity = new Vector2(_rgb.velocity.x*animValue, _rgb.velocity.y);
                endTimer -= Time.deltaTime*2;
                _anim.SetFloat("moveSpeed", Mathf.Abs(_rgb.velocity.x));
            }
          
        }
    }
    
    #region Interactions

    void Jump()
    {
        if (isGrounded)
        {
            _rgb.velocity = new Vector2(_rgb.velocity.x, jumpForce);
            AudioMixerManager._instance.CallSFX(SFXType.Player_Jump);            
        }
        else
        {
            //Start this when you are on air
            if (canDoubleJump)
            {
                _rgb.velocity = new Vector2(_rgb.velocity.x, jumpForce);
                AudioMixerManager._instance.CallSFX(SFXType.Player_Jump);            
                canDoubleJump = false;
            }
        }
    }
    
    void Movement()
    {
        float horizontalMovement = Input.GetAxis("Horizontal") * movementSpeed;

        if (horizontalMovement > 0)
            direction = 1;
        else if (horizontalMovement < 0)
            direction = -1;

        _rgb.velocity = new Vector2(horizontalMovement, _rgb.velocity.y);
        _anim.SetFloat("moveSpeed", Mathf.Abs(horizontalMovement));
    }

    #endregion

    #region Utils

    void GroundCheck()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheckPoint.position,radius,whatIsGround);
        
        //If we are in floor, then reset the doubleJump bool so you can double jump again.
        if (isGrounded)
            canDoubleJump = true;
        
        _anim.SetBool("isGrounded",isGrounded);
    }

    public void KnockBack()
    {
        knockBackCounter = knockBackLength;
        _rgb.velocity = new Vector2(0, knockBackForce.y);
        _anim.SetTrigger("Hurt");
    }
    
    void FlipSprite()
    {
        //By rotations
        // transform.eulerAngles = new Vector3(0, direction == 1 ? 0 : 180, 0);
        //By spriteRenderer
        _spriteRenderer.flipX = direction != 1;
    }

    public void Bounce()
    {
        _rgb.velocity = new Vector2(_rgb.velocity.x, bounceForce);
        AudioMixerManager._instance.CallSFX(SFXType.Player_Jump);            
    }
    
    #endregion

    //Gizmos
    private void OnDrawGizmos()
    {
        if(isGrounded)
            Gizmos.color = new Color(0, 1, 0);
        else
            Gizmos.color = new Color(1, 0, 0);
        Gizmos.DrawSphere(groundCheckPoint.position,radius);
    }
}
