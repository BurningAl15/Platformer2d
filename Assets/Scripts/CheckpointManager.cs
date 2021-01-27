using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager _instance;
    [SerializeField] private List<Checkpoint> _checkpoints = new List<Checkpoint>();

    [SerializeField] private Vector2 currentPosition;
    
    void Awake()
    {
        _instance = this;
    }

    public void UpdateCurrentPosition(Transform _currentPosition)
    {
        AudioMixerManager._instance.CallSFX(SFXType.Level_Selected);
        currentPosition = _currentPosition.position;
    }
    
    public void UpdateCurrentPosition(Vector2 _currentPosition)
    {
        currentPosition = _currentPosition;
    }

    public Vector2 GetPosition()
    {
        return currentPosition;
    }
    
    public void ResetAllCheckpoints()
    {
        for (int i = 0; i < _checkpoints.Count; i++)
        {
            _checkpoints[i].ResetCheckpoint();
        }
    }
}
