using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController2d : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rgb;

    [Header("Property values")] 
    [SerializeField] private float movementSpeed;

    private int direction = 1;
    
    private void Awake()
    {
        rgb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        
    }

    void Update()
    {
        Movement();
        FlipSprite();
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
}
