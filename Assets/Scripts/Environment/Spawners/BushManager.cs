using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BushManager : MonoBehaviour
{
    [SerializeField] private ParticleSystem _particleSystem;
    [SerializeField] private float cooldownTimer;
    [SerializeField] private float cooldownMaxTimer;
    [SerializeField] private Animator anim;

    private bool hasCollided;
    
    void Start()
    {
        
    }

    void Update()
    {
        if (hasCollided)
        {
            cooldownTimer += Time.deltaTime;
            if (cooldownTimer >= cooldownMaxTimer)
            {
                hasCollided = false;
            }
        }
    }

    public void RunParticle()
    {
        _particleSystem.Play();
        print("Call");
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            anim.SetTrigger("Animate");
            hasCollided = true;
        }
    }
}
