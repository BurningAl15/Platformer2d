using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyController : EnemyParent
{
    [Header("Components")]
    [SerializeField] private Rigidbody2D rgb;
    [SerializeField] private Animator anim;
    
    [Header("Movement Variables")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private int direction=-1;

    [Header("Movement Limits")]
    [SerializeField] private Transform leftPoint;
    [SerializeField] private Transform rightPoint;

    [SerializeField] private float moveTime, waitTime;
    private float moveCount, waitCount;

    void Awake()
    {
        rgb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        _collider2D = GetComponent<Collider2D>();
        
        leftPoint.name = leftPoint.name + "- from -" + this.name;
        rightPoint.name = rightPoint.name + "- from -" + this.name;
        
        leftPoint.parent = rightPoint.parent = null;

        moveCount = moveTime;
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
        if (moveCount > 0)
        {
            moveCount -= Time.deltaTime;
            anim.SetBool("Moving", true);
            rgb.velocity = new Vector2(moveSpeed * direction, rgb.velocity.y);

            if (transform.position.x > rightPoint.position.x)
                direction = -1;
            else if (transform.position.x < leftPoint.position.x)
                direction = 1;
            
            FlipSpriteRenderer();

            if (moveCount <= 0)
            {
                waitCount = Random.Range(waitTime * .75f, waitTime * 1.25f);
            }            
        }
        else if (waitCount > 0)
        {
            waitCount -= Time.deltaTime;

            anim.SetBool("Moving", false);
            rgb.velocity = new Vector2(0, rgb.velocity.y);

            if (waitCount <= 0)
            {
                moveCount = Random.Range(moveTime * .75f, moveTime * 1.25f);
            }
        }
    }

    void FlipSpriteRenderer()
    {
        _spriteRenderer.flipX=direction!=-1?true:false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(.5f, .5f, .5f);
        Gizmos.DrawSphere(leftPoint.position,.5f);
        Gizmos.color = new Color(1f, 1f, 1f);
        Gizmos.DrawSphere(rightPoint.position,.5f);
    }
}
