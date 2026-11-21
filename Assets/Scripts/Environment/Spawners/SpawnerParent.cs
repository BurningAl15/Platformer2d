using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerParent : MonoBehaviour
{
    public GameObject enemy;
    public float maxTimer;
    public float timer;

    public bool runSpawning = false;

    /// <summary>True once the goal is reached; spawners create nothing after that.</summary>
    public static bool LevelEnded => LevelManager._instance != null && LevelManager._instance.stopGame;

    protected void Update()
    {
        if (!LevelEnded)
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
    public virtual void Spawn(int _index){}

    public virtual void ResetSpawner() {}
}
