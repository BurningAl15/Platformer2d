using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;


[Serializable]
public class SpawnGroup
{
    public GameObject spawnPrefab;
    public Transform spawnPrefabPosition;
}

public class SpawnByGroup : SpawnerParent
{
    [Header("Spawner Switch")]
    [FormerlySerializedAs("_collider2D")]
    [SerializeField] private Collider2D _enterTriggerCollider2D;

    [SerializeField] private List<SpawnGroup> _spawnGroup=new List<SpawnGroup>();
    // private bool inactive;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !runSpawning)
        {
            runSpawning = true;
            CallAll();
            // _enterTriggerCollider2D.enabled = false;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && runSpawning)
        {
            runSpawning = false;
        }
    }

    public override void Spawn(int _index)
    {
        Instantiate(_spawnGroup[_index].spawnPrefab, _spawnGroup[_index].spawnPrefabPosition.position, _spawnGroup[_index].spawnPrefab.transform.rotation);
    }

    public override void ResetSpawner()
    {
        base.ResetSpawner();
        CallAll();
        ResetTimer();
        // _collider2D.enabled = true;
        // runSpawning = false;
    }

    void CallAll()
    {
        for(int i=0;i<_spawnGroup.Count;i++)
            Spawn(i);
    }
    
    private void OnDrawGizmos()
    {
        for (var i = 0; i < _spawnGroup.Count; i++)
            Gizmos.DrawIcon(_spawnGroup[i].spawnPrefabPosition.position, StringUtils.Get_GizmosIconNumbers(i));
    }
}
