using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager _instance;
    [SerializeField] private float respawnMaxTime;

    private Coroutine currentCoroutine = null;

    int gemsCollected = 0;
    
    private void Awake()
    {
        _instance = this;
    }

    private void Start()
    {
        UIController._instance.UpdateGems_UI(gemsCollected);

    }

    // private void Update()
    // {
    //     if (!PlayerHealth._instance.IsAlive())
    //     {
    //         if (currentCoroutine == null)
    //             currentCoroutine = StartCoroutine(Respawning());
    //     }
    // }

    public void UpdateGemsCollected()
    {
        gemsCollected++;
        UIController._instance.UpdateGems_UI(gemsCollected);
    }
    
    public void RespawnPlayer()
    {
        if (currentCoroutine == null)
            currentCoroutine = StartCoroutine(Respawning());
    }

    IEnumerator Respawning()
    {
        PlayerHealth._instance.DeathEffect();
        PlayerHealth._instance.TurnOffPlayer();
        yield return new WaitUntil(() => PlayerHealth._instance.isDeathEffectEnd());
        FadeEffect._instance.Fade_Out();
        yield return new WaitUntil(() => FadeEffect._instance.endFade);
        yield return new WaitForSeconds(respawnMaxTime);
        PlayerHealth._instance.ResetHealth();
        UIController._instance.ResetHearts();
        PlayerHealth._instance.ResetPlayer();
        PlayerHealth._instance.ResetDeathEffect();
        FadeEffect._instance.Fade_In();
        currentCoroutine = null;
    }
}
