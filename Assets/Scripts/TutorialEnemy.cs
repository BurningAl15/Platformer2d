using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialEnemy : MonoBehaviour
{
    public event Action OnEnemyDeath;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("StompBox"))
        {
            OnEnemyDeath?.Invoke();
        }
    }
}
