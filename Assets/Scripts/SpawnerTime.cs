using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerTime : SpawnerParent
{
    public float waitTimer;
    public float maxWaitTimer;

    private bool spawnEnemy = false;
    
    private void Start()
    {
        // waitTimer = maxWaitTimer;
        runSpawning = true;
    }

    public override void Spawn()
    {
        Instantiate(enemy, transform.position, Quaternion.identity);
    }

    public override void ResetSpawner()
    {
        base.ResetSpawner();
        if (!spawnEnemy)
        {
            Spawn();
            spawnEnemy = true;
        }
        waitTimer += Time.deltaTime;
        if (waitTimer >= maxWaitTimer)
        {
            ResetTimer();
            ResetWaitTime();
        }
    }

    void ResetWaitTime()
    {
        waitTimer = 0;
        spawnEnemy = false;
    }
    
    //If Player is too far from the spawner, then change it to not spawn
    void TurnOffSpawning()
    {
        runSpawning = false;
    }
}
