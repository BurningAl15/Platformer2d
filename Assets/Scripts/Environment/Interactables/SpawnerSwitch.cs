using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerSwitch : SpawnerParent
{
    [Header("Spawner Switch")]
    [SerializeField] private Collider2D _collider2D;
    [SerializeField] private Transform initialPosition;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !runSpawning)
        {
            runSpawning = true;
            Spawn();
            _collider2D.enabled = false;
        }
    }

    public override void Spawn()
    {
        Instantiate(enemy, initialPosition.position, enemy.transform.rotation);
    }

    public override void ResetSpawner()
    {
        base.ResetSpawner();
        ResetTimer();
        _collider2D.enabled = true;
        runSpawning = false;
    }
}
