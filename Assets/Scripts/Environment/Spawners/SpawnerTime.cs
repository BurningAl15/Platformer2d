using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerTime : SpawnerParent
{
    public float waitTimer;
    public float maxWaitTimer;

    private bool spawnEnemy = false;

    public int direction;
    
    private void Start()
    {
        // waitTimer = maxWaitTimer;
    }

    public override void Spawn()
    {
        GameObject _enemy = Instantiate(enemy, transform.position, Quaternion.identity);
        _enemy.GetComponent<RunningEnemy>().SetDirection(direction);
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

    public void TurnOnSpawning()
    {
        runSpawning = true;
        Spawn();
    }
    
    //If Player is too far from the spawner, then change it to not spawn
    public void TurnOffSpawning()
    {
        runSpawning = false;
    }
}
