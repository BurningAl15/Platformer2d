using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingSpike : MonoBehaviour
{
   private void OnTriggerEnter2D(Collider2D other)
   {
      if (other.CompareTag("HangingLog") || other.CompareTag("FallingLog"))
      {
         Destroy(this.gameObject);
      }
   }
}
