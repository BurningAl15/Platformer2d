using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerSwitch : MonoBehaviour
{
    [SerializeField] private GameObject enemy;
    [SerializeField] private float maxTimer;
    private float timer;

    private bool runSpawning = false;
    [SerializeField] private Collider2D _collider2D;
    [SerializeField] private Transform initialPosition;
    void Start()
    {
        
    }

    void Update()
    {
        if (runSpawning)
        {
            timer += Time.deltaTime;
            if (timer >= maxTimer)
            {
                ResetTimer();
                _collider2D.enabled = true;
                runSpawning = false;
            }
        }
    }

    void ResetTimer()
    {
        timer = 0;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !runSpawning)
        {
            runSpawning = true;
            Instantiate(enemy, initialPosition.position, Quaternion.identity);
            _collider2D.enabled = false;
        }
    }
}
