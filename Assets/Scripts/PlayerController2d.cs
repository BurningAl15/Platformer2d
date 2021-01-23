using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerController2d : MonoBehaviour
{
    [Header("Components")] 
    [SerializeField] private Rigidbody2D rgb;

    [SerializeField] private Animator anim;
    private int direction = 1;

    [Header("Movement Variables")] 
    [SerializeField] private float movementSpeed;

    [Header("Jump Variables")] 
    [SerializeField] private float jumpForce;

    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private float radius;
    [SerializeField] private LayerMask whatIsGround;
    private bool isGrounded = false;
    
    
    private void Awake()
    {
        rgb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        
    }

    void Update()
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

    void GroundCheck()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheckPoint.position,radius,whatIsGround);
    }
    
    void Jump()
    {
        if(isGrounded)
            rgb.velocity = new Vector2(rgb.velocity.x, jumpForce);
    }
    
    
    void Movement()
    {
        float horizontalMovement = Input.GetAxis("Horizontal") * movementSpeed;

        if (horizontalMovement > 0)
            direction = 1;
        else if (horizontalMovement < 0)
            direction = -1;
        
        rgb.velocity = new Vector2(horizontalMovement, rgb.velocity.y);
    }

    void FlipSprite()
    {
        transform.eulerAngles = new Vector3(0, direction == 1 ? 0 : 180, 0);
    }

    private void OnDrawGizmos()
    {
        if(isGrounded)
            Gizmos.color = new Color(0, 1, 0);
        else
            Gizmos.color = new Color(1, 0, 0);
        Gizmos.DrawSphere(groundCheckPoint.position,radius);
    }
}
