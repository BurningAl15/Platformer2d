using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public static PlayerHealth _instance;

    [SerializeField] private int currentHealth, maxHealth;

    void Awake()
    {
        currentHealth = maxHealth;

        _instance = this;
    }

    public void DealDamage()
    {
        currentHealth--;
        
        //Validating
        if (currentHealth <= 0)
        {
            currentHealth = 0;            
            gameObject.SetActive(false);
        }
        UIController._instance.UpdateHealthDisplay_Damage(currentHealth);
    }
}
