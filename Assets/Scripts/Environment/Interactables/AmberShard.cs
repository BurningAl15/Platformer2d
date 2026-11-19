using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// Amber Heart shard: an exploration collectible placed freely in levels. Each
/// placed shard has a stable id, so it is remembered across runs; a shard found
/// in an earlier run shows translucent and no longer counts. Shards do not
/// affect gems, stars or perfect-gem records.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class AmberShard : MonoBehaviour
{
    [Tooltip("Stable id of this shard within its level. Generated automatically; do not reuse.")]
    [SerializeField] private string shardId;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite[] frames;
    [SerializeField, Min(1f)] private float framesPerSecond = 8f;
    [SerializeField] private GameObject collectEffect;
    [SerializeField, Range(0f, 1f)] private float foundBeforeAlpha = 0.35f;

    private static readonly List<AmberShard> inScene = new List<AmberShard>();

    /// <summary>Raised when a shard is registered or collected, for counters.</summary>
    public static event Action Changed;

    /// <summary>Shards placed in the current level.</summary>
    public static int CountInScene => inScene.Count;

    private int levelNumber;
    private bool taken;
    private float frameTimer;
    private int frameIndex;

    private void OnEnable() { if (!inScene.Contains(this)) inScene.Add(this); }

    private void OnDisable() => inScene.Remove(this);

    private void Start()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        levelNumber = SceneUtils.Get_CurrentLevelName();
        SaveManager save = SaveManager.Instance;
        if (save != null)
        {
            save.SaveAmberTotal(levelNumber, CountInScene);
            taken = save.HasAmberShard(levelNumber, shardId);
        }
        if (taken && spriteRenderer != null)
        {
            Color c = spriteRenderer.color; c.a = foundBeforeAlpha; spriteRenderer.color = c;
        }
        transform.DOMoveY(transform.position.y + 0.15f, 0.9f).SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo).SetLink(gameObject);
        Changed?.Invoke();
    }

    private void Update()
    {
        if (frames == null || frames.Length < 2 || spriteRenderer == null) return;
        frameTimer += Time.deltaTime;
        if (frameTimer < 1f / framesPerSecond) return;
        frameTimer = 0f;
        frameIndex = (frameIndex + 1) % frames.Length;
        spriteRenderer.sprite = frames[frameIndex];
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (taken || !other.CompareTag(StringUtils.tag_Player)) return;
        taken = true;
        SaveManager.Instance?.CollectAmberShard(levelNumber, shardId);
        AudioMixerManager._instance?.CallSFX_Gems();
        AppBootstrap.TriggerHapticFeedback();
        if (collectEffect != null) SpawnFactory.Spawn(collectEffect, transform.position, Quaternion.identity);

        GetComponent<Collider2D>().enabled = false;
        transform.DOKill();
        Sequence collect = DOTween.Sequence().SetLink(gameObject);
        collect.Append(transform.DOScale(transform.localScale * 1.6f, 0.15f).SetEase(Ease.OutBack));
        if (spriteRenderer != null) collect.Append(spriteRenderer.DOFade(0f, 0.2f));
        collect.OnComplete(() => gameObject.SetActive(false));
        Changed?.Invoke();
    }

#if UNITY_EDITOR
    // Give each placed shard a unique id, also when it is duplicated in the editor.
    private void OnValidate()
    {
        if (Application.isPlaying || UnityEditor.PrefabUtility.IsPartOfPrefabAsset(this)) return;
        bool duplicate = false;
        foreach (AmberShard other in FindObjectsByType<AmberShard>(FindObjectsInactive.Include))
            if (other != this && other.shardId == shardId && other.gameObject.scene == gameObject.scene) { duplicate = true; break; }
        if (string.IsNullOrEmpty(shardId) || duplicate)
        {
            shardId = Guid.NewGuid().ToString("N").Substring(0, 8);
            UnityEditor.EditorUtility.SetDirty(this);
        }
    }
#endif
}
