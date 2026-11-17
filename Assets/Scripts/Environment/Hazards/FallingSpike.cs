using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingSpike : MonoBehaviour, ISpawnable
{
   public void OnSpawned()
   {
      // A reused spike must start its fall from rest.
      if (TryGetComponent(out Rigidbody2D body))
      {
         body.linearVelocity = Vector2.zero;
         body.angularVelocity = 0f;
      }
   }

   private void OnTriggerEnter2D(Collider2D other)
   {
      if (other.CompareTag("HangingLog") || other.CompareTag("FallingLog"))
      {
         SpawnFactory.Release(gameObject);
      }
   }
}
