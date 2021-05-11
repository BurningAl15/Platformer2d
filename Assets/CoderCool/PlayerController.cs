using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D _rigidbody;
    public float moveSpeed;
    public float jumpForce;
    private Vector2 input;
    
    //     
    // public Transform groundCheckPoint;
    // public float radius;
    // public LayerMask whatIsGround;
    // private bool isGrounded = false;
    
    void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        input = new Vector2(Input.GetAxis("Horizontal") * moveSpeed, _rigidbody.velocity.y);
        _rigidbody.velocity = input*moveSpeed;
        
        if(Input.GetButtonDown("Jump"))
            _rigidbody.velocity = new Vector2(_rigidbody.velocity.x, jumpForce);
        
        // GroundCheck();
        // if (Input.GetButtonDown("Jump"))
        // {
        //     // Jump();
        //     _rigidbody.velocity = new Vector2(_rigidbody.velocity.x, jumpForce);
        // }
    }

    // void Jump()
    // {
    //     if (isGrounded)
    //     {
    //         _rigidbody.velocity = new Vector2(_rigidbody.velocity.x, jumpForce);
    //     }
    // }
    //
    // void GroundCheck()
    // {
    //     isGrounded = Physics2D.OverlapCircle(groundCheckPoint.position, radius, whatIsGround);
    // }
    
}
