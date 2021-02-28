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
        maxTimer = Random.Range(.5f, 2f);
        timer = 0;
    }
}
