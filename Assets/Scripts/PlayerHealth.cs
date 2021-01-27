using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerHealth : MonoBehaviour
{
    public static PlayerHealth _instance;

    [Header("Components")]
    [SerializeField] private SpriteRenderer _spriteRenderer;
    
    [Header("Health Variables")]
    [SerializeField] private int currentHealth;
    [SerializeField] private int maxHealth;

    [Header("Invincibility Variables")]
    [SerializeField] private float invicibilityCounter;
    [SerializeField] private float invincibilityLength;

    private Coroutine currentCoroutine = null;
    private bool isInvincible = false;

    [Header("Death Effect")] 
    [SerializeField] private GameObject deathEffect;
    
    void Awake()
    {
        _instance = this;
        currentHealth = maxHealth;

        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        CheckpointManager._instance.UpdateCurrentPosition(transform.position);
    }

    public void DealDamage()
    {
        if (invicibilityCounter <= 0)
        {
            currentHealth--;
        
            //Validating
            if (currentHealth <= 0)
            {
                currentHealth = 0;
                LevelManager._instance.RespawnPlayer();
            }
            else
            {
                invicibilityCounter = invincibilityLength;
          
                PlayerController2d._instance.KnockBack();
                AudioMixerManager._instance.CallSFX(SFXType.Player_Hurt);            
  
                isInvincible = true;
                if (currentCoroutine == null)
                    currentCoroutine = StartCoroutine(BlinkEffect());
            }
            UIController._instance.UpdateHealthDisplay_Damage(currentHealth);
        }
    }

    public void DeathEffect()
    {
        deathEffect.transform.position = this.transform.position;
        deathEffect.SetActive(true);
    }

    public bool isDeathEffectEnd()
    {
        return deathEffect.GetComponent<TurnOff_OnTime>().finish;
    }

    public bool isDamaged()
    {
        return currentHealth < maxHealth;
    }

    public void ResetDeathEffect()
    {
        deathEffect.GetComponent<TurnOff_OnTime>().Reset();
    }

    public void TurnOffPlayer()
    {                
        this.gameObject.SetActive(false);
        AudioMixerManager._instance.CallSFX(SFXType.Player_Death);
    }
    
    public void CureDamage()
    {
        if(currentHealth!=maxHealth)
            UIController._instance.UpdateHealthDisplay_Cure(currentHealth);
    
        currentHealth++;
        
        //Validating
        if (currentHealth >= maxHealth)
        {
            currentHealth = maxHealth;            
        }
    }
    
    private void Update()
    {
        // if (Input.GetKeyDown(KeyCode.Space))
        //     CureDamage();

        if (invicibilityCounter > 0)
        {
            invicibilityCounter -= Time.deltaTime;
            
            if (invicibilityCounter <= 0)
                isInvincible = false;
        }
    }

    public bool IsAlive()
    {
        return currentHealth > 0;
    }
    public void ResetHealth()
    {
        currentHealth = maxHealth;
    }
    public void ResetPlayer()
    {
        transform.position = CheckpointManager._instance.GetPosition();
        Color tempColor = _spriteRenderer.color;
        _spriteRenderer.color = new Color(tempColor.r,
            tempColor.g,
            tempColor.b,
            1);
        gameObject.SetActive(true);
        AudioMixerManager._instance.CallSFX(SFXType.Warp_Jingle);
    }
    
    IEnumerator BlinkEffect()
    {
        Color tempColor = _spriteRenderer.color;
        while (isInvincible)
        {
            _spriteRenderer.color = new Color(tempColor.r,
                tempColor.g,
                tempColor.b,
                .5f);
            yield return new WaitForSeconds(.1f);
            _spriteRenderer.color = new Color(tempColor.r,
                tempColor.g,
                tempColor.b,
                1);
            yield return new WaitForSeconds(.1f);
        }
        currentCoroutine = null;
    }
}
