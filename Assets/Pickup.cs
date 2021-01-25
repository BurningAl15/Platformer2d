using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour
{
    public enum PickupType
    {
        Gem,
        Coin,
        Health
    }

    public PickupType _pickupType;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (_pickupType == PickupType.Gem)
                LevelManager._instance.UpdateGemsCollected();
            if(_pickupType==PickupType.Health)
                PlayerHealth._instance.CureDamage();
            // if(_pickupType==PickupType.Coin)
            
            gameObject.SetActive(false);
        }
    }
}
