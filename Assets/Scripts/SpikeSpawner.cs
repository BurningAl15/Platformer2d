using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class SpikeSpawner : SpawnerParent
{
    public void Initialize()
    {
        runSpawning = true;
        ResetSpawner();
    }
    
    public override void Spawn()
    {
        Instantiate(enemy, transform.position, Quaternion.identity);
    }

    public override void ResetSpawner()
    {
        Spawn();
        maxTimer = Random.Range(1f, 3f);
        timer = 0;
    }
}
