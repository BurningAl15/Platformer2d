using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossSwitch : MonoBehaviour
{
  [SerializeField] private GameObject boss;
  private Coroutine currentCoroutine = null;

  private void Start()
  {
    boss.SetActive(false);
  }

  private void OnTriggerEnter2D(Collider2D other)
  {
    if (other.CompareTag("Player"))
    {
      if (currentCoroutine == null)
        currentCoroutine = StartCoroutine(CallBoss(1));
    }
  }

  IEnumerator CallBoss(float _waitTime)
  {
    //UI make a "boss battle" showing
    AudioMixerManager._instance.PlayBossBackground();
    yield return new WaitForSeconds(_waitTime);
    boss.SetActive(true);
    gameObject.SetActive(false);
    currentCoroutine = null;
  }
}
