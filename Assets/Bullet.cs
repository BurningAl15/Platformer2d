using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float speed;

    private int direction;
    
    void Start()
    {
        AudioMixerManager._instance.CallSFX(SFXType.Boss_Shoot);
    }

    void Update()
    {
        direction = transform.eulerAngles.y == 180 ? -1 : 1;
        transform.position += new Vector3(-speed * direction * Time.deltaTime, 0f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth._instance.DealDamage();
            PlayerController2d._instance.KnockBack();
            Destroy(this.gameObject);
            ScreenShake._instance.StartShake(.6f,.5f);
        }
        AudioMixerManager._instance.CallSFX(SFXType.Boss_Impact);

    }
}
