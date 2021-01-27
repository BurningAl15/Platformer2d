using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamagePlayer : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(StringUtils.tag_Player))
        {
            // other.GetComponent<PlayerHealth>().DealDamage();
            PlayerHealth._instance.DealDamage();            
        }
    }
}
