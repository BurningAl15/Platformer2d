using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyParent : MonoBehaviour
{
    [SerializeField] protected GameObject deathEffect;

    protected bool isDeath = false;
    protected Coroutine currentCoroutine = null;

    [SerializeField] protected Collider2D _collider2D;
    [SerializeField] protected SpriteRenderer _spriteRenderer;

    public void Death(bool destroy = false)
    {
        if (currentCoroutine == null)
            currentCoroutine = StartCoroutine(EnemyDie(destroy));
    }

    IEnumerator EnemyDie(bool destroy)
    {
        isDeath = true;
        deathEffect.transform.position = _spriteRenderer.transform.position;
        _collider2D.enabled = false;
        deathEffect.SetActive(true);
        yield return new WaitUntil(() => deathEffect.GetComponent<TurnOff_OnTime>().finish);
        if(!destroy)
            gameObject.SetActive(false);
        else
            Destroy(gameObject);            
        currentCoroutine = null;
    }
}
