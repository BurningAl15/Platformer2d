using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingEnemyController : EnemyParent
{
    [SerializeField] private List<Transform> movingPoints = new List<Transform>();
    [SerializeField] private float moveSpeed;

    private int currentPoint;

    [SerializeField] private float distanceToAttackPlayer;
    [SerializeField] private float chaseSpeed;

    private Vector3 attackTarget;

    [SerializeField] private float waitingAfterAttack;
    [SerializeField] private float attackCounter;

    private void Awake()
    {
        _collider2D = GetComponent<Collider2D>();
    }

    private void Start()
    {
        for (var i = 0; i < movingPoints.Count; i++)
            movingPoints[i].parent = null;
        transform.position = movingPoints[currentPoint].position;
        FlipSprite(currentPoint);
    }

    private void Update()
    {
        if (attackCounter > 0)
        {
            attackCounter -= Time.deltaTime;
        }
        else
        {
            if (Vector3.Distance(transform.position, PlayerController2d._instance.transform.position) >
                distanceToAttackPlayer)
            {
                if (attackTarget != Vector3.zero)
                {
                    attackTarget = Vector3.zero;
                    FlipSprite();
                }

                transform.position = Vector3.MoveTowards(transform.position,
                    movingPoints[currentPoint].position, moveSpeed * Time.deltaTime);

                if (Vector3.Distance(transform.position, movingPoints[currentPoint].position) <= Mathf.Epsilon)
                {
                    currentPoint++;
                    if (currentPoint >= movingPoints.Count)
                        currentPoint = 0;
                    FlipSprite();
                }
            }
            else
            {
                if (attackTarget == Vector3.zero)
                {
                    attackTarget = PlayerController2d._instance.transform.position;
                    FlipSprite(attackTarget);
                }

                transform.position = Vector3.MoveTowards(transform.position,
                    attackTarget, chaseSpeed * Time.deltaTime);

                if (Vector3.Distance(transform.position, attackTarget) <= Mathf.Epsilon)
                {
                    print("Attack");
                    attackCounter = waitingAfterAttack;
                    FlipSprite(attackTarget);
                    attackTarget = Vector3.zero;
                }
            }
        }
    }

    private void FlipSprite()
    {
        //By rotations
        // transform.eulerAngles = new Vector3(0, direction == 1 ? 0 : 180, 0);
        //By spriteRenderer
        _spriteRenderer.flipX = movingPoints[currentPoint].position.x > transform.position.x;
    }

    private void FlipSprite(int _init)
    {
        //By rotations
        // transform.eulerAngles = new Vector3(0, direction == 1 ? 0 : 180, 0);
        //By spriteRenderer
        _spriteRenderer.flipX = movingPoints[_init + 1].position.x > transform.position.x;
    }

    void FlipSprite(Vector2 _player)
    {
        _spriteRenderer.flipX = _player.x > transform.position.x;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 0, 0, .25f);
        for (var i = 0; i < movingPoints.Count; i++)
            Gizmos.DrawIcon(movingPoints[i].transform.position, StringUtils.Get_GizmosIconNumbers(i));

        Gizmos.color = new Color(1, 1, 1, .25f);
        Gizmos.DrawWireSphere(transform.position,distanceToAttackPlayer);
    }
}
