using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TutorialEnding : MonoBehaviour
{
    [SerializeField] UnityEvent OnComplete;
    [SerializeField] private TutorialEnemy _tutorialEnemy;
    
    private void OnEnable()
    {
        _tutorialEnemy.OnEnemyDeath += Finish;
    }

    void Finish()
    {
        OnComplete.Invoke();
    }
}
