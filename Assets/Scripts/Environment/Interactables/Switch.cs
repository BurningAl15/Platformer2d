using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Switch : MonoBehaviour
{
    [SerializeField] private GameObject _objectToSwitch;

    [SerializeField] private SpriteRenderer _spriteRenderer;

    [SerializeField] private bool switch_Both;
    
    [SerializeField] private Sprite switchDown_Sprite;
    [SerializeField] private Sprite switchUp_Sprite;
    [SerializeField] private bool hasSwitched;
    [Tooltip("If true, then you will turn off. If false, then you will turn on the _objectToSwitch")]
    [SerializeField] private bool deactivateOnSwitch;

    private void Awake()
    {
        if (!hasSwitched)
        {
            if (deactivateOnSwitch)
                _objectToSwitch.SetActive(true);
            else
                _objectToSwitch.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Player"))
        {
            if (switch_Both)
            {
                if (!hasSwitched)
                {
                    _objectToSwitch.SetActive(false);
                    _spriteRenderer.sprite = switchDown_Sprite;
                    hasSwitched = true;
                    PlaySwitchFeedback();
                }
                else
                {
                    _objectToSwitch.SetActive(true);
                    _spriteRenderer.sprite = switchUp_Sprite;
                    hasSwitched = false;
                    PlaySwitchFeedback();
                }
            }
            else
            {
                if (!hasSwitched)
                {
                    if (deactivateOnSwitch)
                        _objectToSwitch.SetActive(false);
                    else
                        _objectToSwitch.SetActive(true);
                    
                    _spriteRenderer.sprite = switchDown_Sprite;
                    hasSwitched = true;
                    PlaySwitchFeedback();
                }
            }
        }
    }

    private void PlaySwitchFeedback()
    {
        if (_spriteRenderer == null) return;
        Transform visual = _spriteRenderer.transform;
        visual.DOKill(true);
        visual.DOPunchScale(new Vector3(0.2f, -0.2f, 0f), 0.25f, 6, 0.6f).SetLink(visual.gameObject);
    }
}
