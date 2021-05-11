using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dart : MonoBehaviour
{
    [SerializeField] private float speed;

    [SerializeField] private int direction;

    [SerializeField] private GameObject smoke;

    [SerializeField] private Rigidbody2D rgb;
    void Start()
    {
        rgb = GetComponent<Rigidbody2D>();
        AudioMixerManager._instance.CallSFX(SFXType.Boss_Shoot);
        rgb.velocity = Vector2.zero;
    }

    void Update()
    {
        rgb.velocity += speed * direction * Time.deltaTime * Vector2.right;
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth._instance.DealDamage();
            PlayerController2d._instance.KnockBack();
            Destroy(this.gameObject);
            ScreenShake._instance.StartShake(.3f,.5f);
        }
        if(other.CompareTag("Ground") || other.CompareTag("Platform"))
            Destroy(this.gameObject);
        Instantiate(smoke, transform.position, transform.rotation);
        AudioMixerManager._instance.CallSFX(SFXType.Boss_Impact);
    }
}
