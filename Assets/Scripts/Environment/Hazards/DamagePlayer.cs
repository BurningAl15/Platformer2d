using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamagePlayer : MonoBehaviour
{
    private readonly HashSet<PlayerHealth> pendingDamage = new HashSet<PlayerHealth>();
    private EnemyParent enemyParent;

    private void Awake()
    {
        enemyParent = GetComponentInParent<EnemyParent>();
    }

    private void OnDisable()
    {
        // Coroutines stop with the object, so pending entries would never clear.
        pendingDamage.Clear();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // The player's body and the Enemy layer ignore each other, so the StompBox
        // is the sensor that reports enemy contact. Enemy damage is deferred
        // below so a valid stomp from the same contact can win first.
        PlayerHealth player = other.GetComponentInParent<PlayerHealth>();
        if (player == null)
            return;

        // Non-enemy hazards keep their immediate damage behavior.
        if (enemyParent == null)
        {
            player.DealDamage();
            return;
        }

        if (!pendingDamage.Add(player))
            return;

        StartCoroutine(ResolveDamageAfterPhysics(player));
    }

    private IEnumerator ResolveDamageAfterPhysics(PlayerHealth player)
    {
        // Let the player's foot sensor resolve a stomp from the same approach first.
        yield return new WaitForFixedUpdate();
        yield return null;

        if (player != null && (enemyParent == null || !enemyParent.IsDying))
            player.DealDamage();

        pendingDamage.Remove(player);
    }
}
