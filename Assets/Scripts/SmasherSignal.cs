using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmasherSignal : MonoBehaviour
{
    public bool smasherOn;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !smasherOn)
        {
            smasherOn = true;
        }
    }
    
    // private void OnTriggerExit2D(Collider2D other)
    // {
    //     if (other.CompareTag("Player") && smasherOn)
    //     {
    //         smasherOn = false;
    //     }
    // }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0, 1, 0,.5f);
        Gizmos.DrawCube(this.transform.position,new Vector2(.5f,.5f));
    }
}
