using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoldingDamageBall_Signal : MonoBehaviour
{
   [SerializeField] private int direction = 1;

   public int Direction => direction;
   
   private void OnTriggerEnter2D(Collider2D other)
   {
      if(other.CompareTag("ChangeDirection"))
         direction *= -1;
   }
}
