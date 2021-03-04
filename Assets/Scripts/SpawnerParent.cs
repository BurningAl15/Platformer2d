using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerParent : MonoBehaviour
{
    public GameObject enemy;
    public float maxTimer;
    public float timer;

    public bool runSpawning = false;

    protected void Update()
    {
        if (!LevelManager._instance.stopGame)
        {
            if (runSpawning)
            {
                timer += Time.deltaTime;
                if (timer >= maxTimer)
                {
                    ResetSpawner();
                }
            }
        }
    }
    
    protected void ResetTimer()
    {
        timer = 0;
    }

    
    public virtual void Spawn(){}

    public virtual void ResetSpawner() {}
}
