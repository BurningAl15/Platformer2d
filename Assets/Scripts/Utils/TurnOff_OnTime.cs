using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class TurnOff_OnTime : MonoBehaviour
{
    public bool finish;
    [SerializeField] private bool mustDestroy;
    public void TurnOnTimer()
    {
        if(mustDestroy)
            Destroy(this.gameObject);
        else
            gameObject.SetActive(false);

        finish = true;
    }

    public void Reset()
    {
        finish = false;
    }
}
