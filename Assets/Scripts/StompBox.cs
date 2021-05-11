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
    [Range(0, 100)] [SerializeField] private float chanceToDrop;

    private Bouncer tempBouncer = null;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(StringUtils.tag_Enemy))
            EnemyBehaviour(other);

        if (other.CompareTag(StringUtils.tag_Bouncer))
        {
            float bounceValue = 2f;
            if (other.GetComponent<Bouncer>() != null)
            {
                tempBouncer = other.GetComponent<Bouncer>();
                bounceValue = tempBouncer.bouncePower;
            }

            PlayerController2d._instance.Bounce(bounceValue);
            other.GetComponent<Animator>().SetTrigger("Bounce");
        }
    }

    private void EnemyBehaviour(Collider2D other)
    {
        Vector2 lastPos = other.transform.position;
        other.gameObject.SetActive(false);
        GameObject parent = other.transform.parent.gameObject;
        parent.GetComponent<EnemyParent>().Death();
        
        PlayerController2d._instance.Bounce();
        AudioMixerManager._instance.CallSFX(SFXType.Enemy_Death);

        DropItem(lastPos);
    }

    private void DropItem(Vector2 _lastPos)
    {
        float dropSelect = Random.Range(0, 100);

        if (dropSelect <= chanceToDrop)
            Instantiate(collectible, _lastPos, Quaternion.identity);
    }

    public void TurnLayer_Active()
    {
        gameObject.layer = 10;
    }

    public void TurnLayer_Inactive()
    {
        gameObject.layer = 13;
    }
}