using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Random = UnityEngine.Random;
using Vector2 = UnityEngine.Vector2;

public class StompBox : MonoBehaviour
{
    [SerializeField] private GameObject collectible;
    [Range(0,100)]
    [SerializeField] private float chanceToDrop;
    
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            Vector2 lastPos = other.transform.position;            
            other.gameObject.SetActive(false);
            other.transform.parent.gameObject.GetComponent<EnemyController>().Death();
            PlayerController2d._instance.Bounce();
            
            float dropSelect = Random.Range(0, 100);

            if (dropSelect <= chanceToDrop)
                Instantiate(collectible, lastPos, Quaternion.identity);
        }
    }
}
