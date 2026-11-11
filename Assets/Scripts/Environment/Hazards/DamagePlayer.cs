using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamagePlayer : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        // The trigger may receive a child collider (for example StompBox)
        // whose tag is not Player. Resolve the health component up the hierarchy.
        PlayerHealth player = other.GetComponentInParent<PlayerHealth>();
        if (player != null)
            player.DealDamage();
    }
}
