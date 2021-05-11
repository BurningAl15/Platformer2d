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
                AudioMixerManager._instance.CallSFX_Gems();
                if (currentCoroutine == null)
                    currentCoroutine = StartCoroutine(CollectEffect());
            }

            if (_pickupType == PickupType.Health && PlayerHealth._instance.isDamaged())
            {
                PlayerHealth._instance.CureDamage();
                AudioMixerManager._instance.CallSFX(SFXType.Player_Pickup_Health);
                if (currentCoroutine == null)
                    currentCoroutine = StartCoroutine(CollectEffect());
            }
        }
    }

    IEnumerator CollectEffect()
    {
        GetComponent<SpriteRenderer>().enabled = false;
        effect.SetActive(true);
        yield return new WaitUntil(() => effect.GetComponent<TurnOff_OnTime>().finish);
        // gameObject.SetActive(false);
        Destroy(this.gameObject);
    }

    public bool IsGem()
    {
        return _pickupType == PickupType.Gem;
    }
}
