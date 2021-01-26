using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StompBox : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            print("Hit Enemy");
            other.gameObject.SetActive(false);
            
            other.transform.parent.gameObject.GetComponent<EnemyController>().Death();
        }
    }
}
