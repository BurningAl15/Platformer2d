using UnityEngine;
using UnityEngine.Serialization;

public class Checkpoint : MonoBehaviour
{
    public SpriteRenderer _spriteRenderer;

    [FormerlySerializedAs("notActive")] [SerializeField] private Sprite checkpointSprite_On;
    [FormerlySerializedAs("active")] [SerializeField] private Sprite checkpointSprite_Off;

    private bool isSelected = false;

    [SerializeField] private ParticleSystem _particle;
    
    [SerializeField] private int checkpointIndex = 0;
    
    void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _spriteRenderer.sprite = checkpointSprite_Off;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isSelected)
        {
            CheckpointManager._instance.ResetAllCheckpoints();
            CheckpointManager._instance.UpdateCurrentPosition(this.transform);
            _spriteRenderer.sprite = checkpointSprite_On;
            _particle.Play();
            isSelected = true;
            
            if (AnalyticsManager.Instance != null)
                AnalyticsManager.Instance.TrackCheckpointReached(
                    SceneUtils.Get_CurrentLevelName(),
                    checkpointIndex
                );
        }
    }

    public void ResetCheckpoint()
    {
        _spriteRenderer.sprite = checkpointSprite_Off;
        isSelected = false;
    }
}
