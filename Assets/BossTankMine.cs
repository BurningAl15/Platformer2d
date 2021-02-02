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
        }
    }

    public void DestroyMine()
    {
        Instantiate(explosion, transform.position, transform.rotation);
        Destroy(gameObject);
        AudioMixerManager._instance.CallSFX(SFXType.Enemy_Death);
    }
}
