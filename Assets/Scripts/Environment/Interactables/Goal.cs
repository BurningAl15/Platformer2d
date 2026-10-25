using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal : MonoBehaviour
{
    private Animator anim;
    
    void Awake()
    { 
        anim = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(StringUtils.tag_Player))
        {
            anim.SetTrigger("Flap");
            GetComponent<BoxCollider2D>().enabled = false;
            LevelManager._instance.EndLevel();
        }
    }
}
