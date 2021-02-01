using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Random = UnityEngine.Random;
using Vector2 = UnityEngine.Vector2;

public class StompBox : MonoBehaviour
{
    [SerializeField] private GameObject collectible;
    [Range(0,100)]
    [SerializeField] private float chanceToDrop;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(StringUtils.tag_Enemy))
        {
            Vector2 lastPos = other.transform.position;            
            other.gameObject.SetActive(false);
            other.transform.parent.gameObject.GetComponent<EnemyParent>().Death();
            PlayerController2d._instance.Bounce();
            AudioMixerManager._instance.CallSFX(SFXType.Enemy_Death);

            float dropSelect = Random.Range(0, 100);

            if (dropSelect <= chanceToDrop)
                Instantiate(collectible, lastPos, Quaternion.identity);
        }

        if (other.CompareTag(StringUtils.tag_Bouncer))
        {
            PlayerController2d._instance.Bounce(1.5f);
            other.GetComponent<Animator>().SetTrigger("Bounce");
        }
    }
}
