using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class EnemyParent : MonoBehaviour
{
    [SerializeField] protected GameObject deathEffect;

    protected bool isDeath = false;
    protected Coroutine currentCoroutine = null;

    public bool IsDying => isDeath;

    [SerializeField] protected Collider2D _collider2D;
    [SerializeField] protected SpriteRenderer _spriteRenderer;

    [Header("Death Feedback")]
    [SerializeField, Min(0f)] private float squashDuration = 0.1f;
    [SerializeField] private Vector2 squashScale = new Vector2(1.35f, 0.3f);

    public void Death(bool destroy = false)
    {
        if (currentCoroutine == null)
            currentCoroutine = StartCoroutine(EnemyDie(destroy));
    }

    IEnumerator EnemyDie(bool destroy)
    {
        isDeath = true;
        Transform visual = _spriteRenderer.transform;
        Vector3 effectPosition = visual.position;

        // Every collider is disabled so the visual can stay active for the squash.
        foreach (Collider2D enemyCollider in GetComponentsInChildren<Collider2D>())
            enemyCollider.enabled = false;

        if (squashDuration > 0f)
        {
            // Flatten toward the feet, then hand over to the existing death effect.
            Vector3 baseScale = visual.localScale;
            float feetOffset = _spriteRenderer.bounds.extents.y * (1f - squashScale.y);
            Tween squash = DOTween.Sequence()
                .Join(visual.DOScale(new Vector3(baseScale.x * squashScale.x, baseScale.y * squashScale.y, baseScale.z), squashDuration).SetEase(Ease.OutQuad))
                .Join(visual.DOMoveY(effectPosition.y - feetOffset, squashDuration).SetEase(Ease.OutQuad))
                .SetLink(gameObject);
            yield return squash.WaitForCompletion();
        }

        deathEffect.transform.position = effectPosition;
        deathEffect.SetActive(true);
        _spriteRenderer.enabled = false;
        yield return new WaitUntil(() => deathEffect.GetComponent<TurnOff_OnTime>().finish);
        if(!destroy)
            gameObject.SetActive(false);
        else
            Destroy(gameObject);            
        currentCoroutine = null;
    }
}
