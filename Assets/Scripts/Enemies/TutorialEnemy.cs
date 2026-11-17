using System;
using UnityEngine;

/// <summary>
/// Reports the defeat of the tutorial enemy. It listens to the enemy's real
/// death, so touching the enemy without a valid stomp does not count.
/// </summary>
public class TutorialEnemy : MonoBehaviour
{
    public event Action OnEnemyDeath;

    private EnemyParent enemy;

    private void Awake()
    {
        enemy = GetComponentInParent<EnemyParent>();
        if (enemy == null)
            Debug.LogError("[TutorialEnemy] No EnemyParent found; the tutorial cannot detect the enemy's defeat.", this);
    }

    private void OnEnable()
    {
        if (enemy != null)
            enemy.Died += HandleEnemyDied;
    }

    private void OnDisable()
    {
        if (enemy != null)
            enemy.Died -= HandleEnemyDied;
    }

    private void HandleEnemyDied()
    {
        OnEnemyDeath?.Invoke();
    }
}
