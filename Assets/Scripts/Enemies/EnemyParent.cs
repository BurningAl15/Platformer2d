using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class EnemyParent : MonoBehaviour, ISpawnable
{
    [SerializeField] protected GameObject deathEffect;

    protected bool isDeath = false;
    protected Coroutine currentCoroutine = null;

    public bool IsDying => isDeath;

    /// <summary>Raised once when the enemy is actually defeated.</summary>
    public event Action Died;

    [SerializeField] protected Collider2D _collider2D;
    [SerializeField] protected SpriteRenderer _spriteRenderer;

    [Header("Death Feedback")]
    [SerializeField, Min(0f)] private float squashDuration = 0.1f;
    [SerializeField] private Vector2 squashScale = new Vector2(1.35f, 0.3f);

    private readonly List<Collider2D> disabledColliders = new List<Collider2D>();
    private Vector3 visualLocalPosition;
    private Vector3 visualLocalScale;
    private bool visualPoseSaved;

    /// <summary>Restores the alive state when the factory reuses this enemy.</summary>
    public virtual void OnSpawned()
    {
        isDeath = false;
        currentCoroutine = null;

        foreach (Collider2D enemyCollider in disabledColliders)
        {
            if (enemyCollider != null)
                enemyCollider.enabled = true;
        }
        disabledColliders.Clear();

        if (_spriteRenderer != null)
        {
            RestoreVisualPose();
            _spriteRenderer.enabled = true;
        }

        if (deathEffect != null)
            deathEffect.SetActive(false);
    }

    private void RestoreVisualPose()
    {
        if (!visualPoseSaved) return;
        Transform visual = _spriteRenderer.transform;
        visual.DOKill();
        visual.localPosition = visualLocalPosition;
        visual.localScale = visualLocalScale;
        visualPoseSaved = false;
    }

    public void Death(bool destroy = false)
    {
        if (currentCoroutine == null)
            currentCoroutine = StartCoroutine(EnemyDie(destroy));
    }

    IEnumerator EnemyDie(bool destroy)
    {
        isDeath = true;
        Died?.Invoke();
        Transform visual = _spriteRenderer.transform;
        Vector3 effectPosition = visual.position;

        // Every collider is disabled so the visual can stay active for the squash.
        foreach (Collider2D enemyCollider in GetComponentsInChildren<Collider2D>())
        {
            if (!enemyCollider.enabled) continue;
            enemyCollider.enabled = false;
            disabledColliders.Add(enemyCollider);
        }

        visualLocalPosition = visual.localPosition;
        visualLocalScale = visual.localScale;
        visualPoseSaved = true;

        if (squashDuration > 0f)
        {
            // Flatten toward the feet, then hand over to the existing death effect.
            Vector3 baseScale = visual.localScale;
            float feetOffset = _spriteRenderer.bounds.extents.y * (1f - squashScale.y);
            Tween squash = DOTween.Sequence()
                .Join(visual.DOScale(new Vector3(baseScale.x * squashScale.x, baseScale.y * squashScale.y, baseScale.z), squashDuration).SetEase(Ease.OutQuad))
                .Join(visual.DOMoveY(effectPosition.y - feetOffset, squashDuration).SetEase(Ease.OutQuad))
                .SetTarget(visual)
                .SetLink(gameObject);
            yield return squash.WaitForCompletion();
        }

        deathEffect.transform.position = effectPosition;
        deathEffect.SetActive(true);
        _spriteRenderer.enabled = false;
        RestoreVisualPose();
        yield return new WaitUntil(() => deathEffect.GetComponent<TurnOff_OnTime>().finish);
        currentCoroutine = null;
        // Factory enemies return for reuse; scene-placed enemies keep their old behavior.
        if (destroy || SpawnFactory.IsManaged(gameObject))
            SpawnFactory.Release(gameObject);
        else
            gameObject.SetActive(false);
    }
}
