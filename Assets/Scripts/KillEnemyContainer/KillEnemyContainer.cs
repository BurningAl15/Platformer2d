using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillEnemyContainer : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("EnemyContainer") && other.GetComponent<RunningEnemy>())
            other.GetComponent<RunningEnemy>().Death(true);
    }
}
