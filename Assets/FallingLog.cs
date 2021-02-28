using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingLog : MonoBehaviour
{
    [SerializeField] private bool activateFalling;
    [SerializeField] private float waitingTime;
    private float timer=0;
    [SerializeField] private Rigidbody2D rgb;
    
    // bool wait
    [SerializeField] private Transform initialPosition;
    void Start()
    {
        rgb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (activateFalling)
        {
            timer += Time.deltaTime;
            if (timer >= waitingTime)
            {
                rgb.bodyType = RigidbodyType2D.Dynamic;
            }
        }
    }

    public void ResetLog()
    {
        transform.position = initialPosition.position;
        transform.rotation=Quaternion.identity;
        activateFalling = false;
        rgb.bodyType = RigidbodyType2D.Static;
        timer = 0;
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("StompBox"))
        {
            activateFalling = true;
        }
    }
}
