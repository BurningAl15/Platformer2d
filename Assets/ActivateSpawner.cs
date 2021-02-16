using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateSpawner : MonoBehaviour
{
   private bool turnOn = false;
   [SerializeField] private SpawnerTime _spawnerTime;
   private void OnTriggerEnter2D(Collider2D other)
   {
      if (other.CompareTag("Player") && !turnOn)
      {
         print("AAA");
         _spawnerTime.TurnOnSpawning();
         turnOn = true;
      }
   }
}
