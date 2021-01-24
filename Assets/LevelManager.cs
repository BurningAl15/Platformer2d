using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager _instance;
    [SerializeField] private float respawnMaxTime;

    private Coroutine currentCoroutine = null;

    private void Awake()
    {
        _instance = this;
    }

    // private void Update()
    // {
    //     if (!PlayerHealth._instance.IsAlive())
    //     {
    //         if (currentCoroutine == null)
    //             currentCoroutine = StartCoroutine(Respawning());
    //     }
    // }

    public void RespawnPlayer()
    {
        if (currentCoroutine == null)
            currentCoroutine = StartCoroutine(Respawning());
    }

    IEnumerator Respawning()
    {
        print("AA");
        PlayerHealth._instance.TurnOffPlayer();
        yield return new WaitForSeconds(respawnMaxTime);
        print("BB");
        PlayerHealth._instance.ResetHealth();
        UIController._instance.ResetHearts();
        PlayerHealth._instance.ResetPlayer();
        currentCoroutine = null;
    }
}
