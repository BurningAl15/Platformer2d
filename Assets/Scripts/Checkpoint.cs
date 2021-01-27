using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Checkpoint : MonoBehaviour
{
    public SpriteRenderer _spriteRenderer;

    [FormerlySerializedAs("notActive")] [SerializeField] private Sprite checkpointSprite_On;
    [FormerlySerializedAs("active")] [SerializeField] private Sprite checkpointSprite_Off;

    private bool isSelected = false;

    void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _spriteRenderer.sprite = checkpointSprite_Off;
    }

    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isSelected)
        {
            CheckpointManager._instance.ResetAllCheckpoints();
            CheckpointManager._instance.UpdateCurrentPosition(this.transform);
            _spriteRenderer.sprite = checkpointSprite_On;
            isSelected = true;
        }
    }

    public void ResetCheckpoint()
    {
        _spriteRenderer.sprite = checkpointSprite_Off;
        isSelected = false;
    }
}
