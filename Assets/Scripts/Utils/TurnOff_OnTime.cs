using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class TurnOff_OnTime : MonoBehaviour
{
    public bool finish;
    [SerializeField] private bool mustDestroy;
    private void OnEnable()
    {
        finish = false;
    }

    public void TurnOnTimer()
    {
        if (mustDestroy || SpawnFactory.IsManaged(gameObject))
            SpawnFactory.Release(gameObject);
        else
            gameObject.SetActive(false);

        finish = true;
    }

    public void Reset()
    {
        finish = false;
    }
}
