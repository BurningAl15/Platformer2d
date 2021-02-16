using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunningEnemy : EnemyParent
{
    [Header("Components")]
    [SerializeField] private Rigidbody2D rgb;
    [SerializeField] private Animator anim;

    [Header("Movement Variables")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private int direction=-1;

    
    void Awake()
    {
        rgb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        _collider2D = GetComponent<Collider2D>();
        FlipSpriteRenderer();
    }

    public void SetDirection(int _direction)
    {
        direction = _direction;
        FlipSpriteRenderer();
    }

    void Update()
    {
        if(!isDeath)
            Movement();
        else
            rgb.velocity = Vector2.zero;
    }

    void Movement()
    {
        rgb.velocity = new Vector2(moveSpeed * direction, rgb.velocity.y);
    }
    
    void FlipSpriteRenderer()
    {
        _spriteRenderer.flipX=direction!=-1?true:false;
    }
}
