﻿using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class HeartUI
{
    public Image heartImage;
    private Sprite heartEmpty;
    private Sprite heartHalf;
    private Sprite heartFull;

    public void Init(Sprite empty, Sprite half, Sprite full)
    {
        heartEmpty = empty;
        heartHalf = half;
        heartFull = full;

        heartImage.sprite = heartFull;
    }
    public void UpdateHeartSprite_Damage(int _)
    {
        if (_ % 2 == 0)
            heartImage.sprite = heartEmpty;
        else if (_ % 2 == 1)
            heartImage.sprite = heartHalf;
    }

    public void UpdateHeartSprite_Cure(int _)
    {
        if (_ % 2 == 1)
            heartImage.sprite = heartFull;
        else if (_ % 2 == 0)
            heartImage.sprite = heartHalf;
    }
}

public class UIController : MonoBehaviour
{
    public static UIController _instance;

    [Header("Hearts")]
    [SerializeField] private List<HeartUI> hearts = new List<HeartUI>();
    
    [Header("Heart Sprites")]
    [SerializeField] private Sprite heartEmpty;
    [SerializeField] private Sprite heartHalf;
    [SerializeField] private Sprite heartFull;

    [Header("Gems UI")] 
    [SerializeField] private TextMeshProUGUI gemsText;
    
    /*
     Heart stuff logic
     x -> empty
     x+1 -> half
     x+2 -> full
    */
    void Awake()
    {
        _instance = this;
        
        for(int i=0;i<hearts.Count;i++)
            hearts[i].Init(heartEmpty,heartHalf,heartFull);
    }

    private void Start()
    {
        FadeEffect._instance.Initialize();
    }

    #region Health
    public void UpdateHealthDisplay_Damage(int index)
    {
        int _currentIndex = index / 2;
        
        hearts[_currentIndex].UpdateHeartSprite_Damage(index);
    }
    public void UpdateHealthDisplay_Cure(int index)
    {
        int _currentIndex = index / 2;
        hearts[_currentIndex].UpdateHeartSprite_Cure(index);
    }
    public void ResetHearts()
    {
        for(int i=0;i<hearts.Count;i++)
            hearts[i].Init(heartEmpty,heartHalf,heartFull);
    }    
    #endregion

    public void UpdateGems_UI(int _currentGems)
    {
        gemsText.text = "" + _currentGems;
    }
}
