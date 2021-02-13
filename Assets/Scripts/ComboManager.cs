using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComboManager : MonoBehaviour
{
    public static ComboManager _instance;

    private bool isGemCombo = false;

    public bool IsGemCombo
    {
        get => isGemCombo;
        set => isGemCombo = value;
    }

    public float timer = 0;
    public float maxTimer = 0;

    public bool isRunning = false;
    
    private void Awake()
    {
        _instance = this;
    }

    private void Start()
    {
    }

    private void Update()
    {
        if (isRunning)
        {
            if (isGemCombo)
            {
                SetTimer();
            }
            else
            {
                if(timer>0)
                    timer -= Time.deltaTime;
                else if (timer <= 0)
                {
                    timer = 0;
                    AudioMixerManager._instance.ResetPitch();
                    isRunning = false;
                }            
            }            
        }
    }

    void SetTimer()
    {
        timer = maxTimer;
        isGemCombo = false;
    }

    public void RunTimer()
    {
        isGemCombo = true;
        isRunning = true;
    }
}
