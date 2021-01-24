using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int currentHealth, maxHealth;

    void Awake()
    {
        currentHealth = maxHealth;
    }
    void Update()
    {
        
    }

    public void DealDamage()
    {
        currentHealth--;
        
        //Validating
        if(currentHealth<=0)
            gameObject.SetActive(false);
    }
}
