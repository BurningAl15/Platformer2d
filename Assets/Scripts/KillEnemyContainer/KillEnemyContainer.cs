using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillEnemyContainer : MonoBehaviour
{
    [SerializeField] private BoxCollider2D _boxCollider;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("EnemyContainer") && other.GetComponent<RunningEnemy>())
            other.GetComponent<RunningEnemy>().Death(true);
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0, 0, 1,.25f);
    
        Gizmos.DrawCube(transform.position,_boxCollider.size);
    }

}
