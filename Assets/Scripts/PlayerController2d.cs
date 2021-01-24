﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerController2d : MonoBehaviour
{
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
    
    private void Awake()
    {
        _rgb = GetComponent<Rigidbody2D>();
        _anim = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
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
        
        //If we are in floor, then reset the doubleJump bool so you can double jump again.
        if (isGrounded)
            canDoubleJump = true;
        
        _anim.SetBool("isGrounded",isGrounded);
    }
    
    void Jump()
    {
        if(isGrounded)
            _rgb.velocity = new Vector2(_rgb.velocity.x, jumpForce);
        else
        {
            //Start this when you are on air
            if (canDoubleJump)
            {
                _rgb.velocity = new Vector2(_rgb.velocity.x, jumpForce);
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

        _anim.SetFloat("moveSpeed", Mathf.Abs(horizontalMovement));
        
        _rgb.velocity = new Vector2(horizontalMovement, _rgb.velocity.y);
    }

    void FlipSprite()
    {
        //By rotations
        // transform.eulerAngles = new Vector3(0, direction == 1 ? 0 : 180, 0);
        //By spriteRenderer
        _spriteRenderer.flipX = direction != 1;
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