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
    void Awake()
    {
        _instance = this;
        currentHealth = maxHealth;

        _spriteRenderer = GetComponent<SpriteRenderer>();
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
                gameObject.SetActive(false);
            }
            else
            {
                invicibilityCounter = invincibilityLength;
          
                PlayerController2d._instance.KnockBack();
                
                isInvincible = true;
                if (currentCoroutine == null)
                    currentCoroutine = StartCoroutine(BlinkEffect());
            }
            UIController._instance.UpdateHealthDisplay_Damage(currentHealth);
        }
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
        if (Input.GetKeyDown(KeyCode.Space))
            CureDamage();

        if (invicibilityCounter > 0)
        {
            invicibilityCounter -= Time.deltaTime;
            
            if (invicibilityCounter <= 0)
                isInvincible = false;
        }
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
