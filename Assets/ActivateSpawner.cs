using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateSpawner : MonoBehaviour
{
   private bool turnOn = false;
   [SerializeField] private SpawnerTime _spawnerTime;
   [SerializeField] private BoxCollider2D _boxCollider;
   
   private void OnTriggerEnter2D(Collider2D other)
   {
      if (other.CompareTag("Player") && !turnOn)
      {
         _spawnerTime.TurnOnSpawning();
         turnOn = true;
      }
   }
   
   private void OnDrawGizmos()
   {
      Gizmos.color = new Color(0, 1, 0,.25f);
    
      Gizmos.DrawCube(transform.position,_boxCollider.size);
   }
}
