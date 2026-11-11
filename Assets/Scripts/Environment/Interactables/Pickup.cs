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
    private bool hasBeenCollected;

    [SerializeField] private Transform pickupTransform;
    public Transform PickupTransform => pickupTransform;

    private void Awake()
    {
        pickupTransform = transform;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(StringUtils.tag_Player) || hasBeenCollected)
            return;

        if (_pickupType == PickupType.Gem)
        {
            // A player can have multiple colliders and receive repeated trigger
            // callbacks before the collect animation destroys this object.
            // Claim this pickup before changing the level total.
            hasBeenCollected = true;
            LevelManager._instance?.UpdateGemsCollected();
            AudioMixerManager._instance?.CallSFX_Gems();
            // Las gemas globales se acumulan al completar el nivel.
            currentCoroutine = StartCoroutine(CollectEffect());
            return;
        }

        if (_pickupType == PickupType.Health && PlayerHealth._instance != null &&
            PlayerHealth._instance.isDamaged())
        {
            hasBeenCollected = true;
            PlayerHealth._instance.CureDamage();
            AudioMixerManager._instance?.CallSFX(SFXType.Player_Pickup_Health);
            currentCoroutine = StartCoroutine(CollectEffect());
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
