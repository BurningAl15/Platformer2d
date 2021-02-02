using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossTankHitbox : MonoBehaviour
{
    [SerializeField] private BossTankController bossCont;
    
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && PlayerController2d._instance.transform.position.y > transform.position.y)
        {
            bossCont.TakeHit();
            PlayerController2d._instance.Bounce();
            gameObject.SetActive(false);
        }
    }
}
