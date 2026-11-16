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

    private void OnTriggerEnter2D(Collider2D other)
    {
        // The stomp sensor has its own enemy-contact handler and must not deal damage.
        if (other.CompareTag("StompBox"))
            return;

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
