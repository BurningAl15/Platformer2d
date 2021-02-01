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
    
    public void Death()
    {
        if (currentCoroutine == null)
            currentCoroutine = StartCoroutine(EnemyDie());
    }

    IEnumerator EnemyDie()
    {
        isDeath = true;
        deathEffect.transform.position = _spriteRenderer.transform.position;
        _collider2D.enabled = false;
        deathEffect.SetActive(true);
        yield return new WaitUntil(() => deathEffect.GetComponent<TurnOff_OnTime>().finish);
        gameObject.SetActive(false);
    }
}
