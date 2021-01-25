using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnOff_OnTime : MonoBehaviour
{
    public bool finish;
    
    public void TurnOnTimer()
    {
        gameObject.SetActive(false);
        finish = true;
    }
}
