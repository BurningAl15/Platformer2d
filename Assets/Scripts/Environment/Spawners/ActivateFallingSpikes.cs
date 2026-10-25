using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateFallingSpikes : MonoBehaviour
{
    [SerializeField] private List<SpikeSpawner> _spikeSpawners = new List<SpikeSpawner>();
    private bool isActivated = false;
    [SerializeField] private BoxCollider2D _boxCollider;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isActivated)
        {
            for(int i=0;i<_spikeSpawners.Count;i++) 
                _spikeSpawners[i].Initialize();
            isActivated = true;
        }
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0, 1, 0,.25f);
        Gizmos.DrawCube(transform.position,_boxCollider.size);
    }
}
