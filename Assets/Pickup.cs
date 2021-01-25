using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour
{
    public enum PickupType
    {
        Gem,
        Health
    }

    public PickupType _pickupType;

    [SerializeField] private GameObject effect;

    private Coroutine currentCoroutine = null;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (_pickupType == PickupType.Gem)
                LevelManager._instance.UpdateGemsCollected();
            if(_pickupType==PickupType.Health)
                PlayerHealth._instance.CureDamage();
            // if(_pickupType==PickupType.Coin)
            if (currentCoroutine == null)
                currentCoroutine = StartCoroutine(CollectEffect());
        }
    }

    IEnumerator CollectEffect()
    {
        GetComponent<SpriteRenderer>().enabled = false;
        effect.SetActive(true);
        yield return new WaitUntil(() => effect.GetComponent<TurnOff_OnTime>().finish);
        gameObject.SetActive(false);
    }
}
