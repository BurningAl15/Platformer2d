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
        if (other.CompareTag(StringUtils.tag_Player))
        {
            if (_pickupType == PickupType.Gem)
            {
                LevelManager._instance.UpdateGemsCollected();
                AudioMixerManager._instance.CallSFX(SFXType.Player_Pickup_Gem);
            }

            if (_pickupType == PickupType.Health)
            {
                PlayerHealth._instance.CureDamage();
                AudioMixerManager._instance.CallSFX(SFXType.Player_Pickup_Health);
            }
            // if(_pickupType==PickupType.Coin)
            
            if (GetComponent<Rigidbody2D>())
                GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            
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
