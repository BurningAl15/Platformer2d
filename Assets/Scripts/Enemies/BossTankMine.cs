using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossTankMine : MonoBehaviour
{
    public GameObject explosion;
    [SerializeField] private float mineTimer;
    private float mineCounter;

    private void Start()
    {
        mineCounter = mineTimer;
    }

    // private void Update()
    // {
    //     mineCounter -= Time.deltaTime;
    //     if (mineCounter <= 0)
    //     {
    //         DestroyMine();
    //     }
    // }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            DestroyMine();
            PlayerHealth._instance.DealDamage();
            ScreenShake._instance.StartShake(.6f,.5f);
        }
    }

    public void DestroyMine()
    {
        SpawnFactory.Spawn(explosion, transform.position, transform.rotation);
        SpawnFactory.Release(gameObject);
        AudioMixerManager._instance.CallSFX(SFXType.Enemy_Death);
    }
}
