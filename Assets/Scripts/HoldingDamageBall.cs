using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoldingDamageBall : MonoBehaviour
{
    [SerializeField] private Transform holdingBall;

    [SerializeField] private float speed;

    // [SerializeField] private HoldingDamageBall_Signal _signal;
    
    private void Update()
    {
        holdingBall.RotateAround(this.transform.position, Vector3.forward,   speed * Mathf.Sin( Time.time));
    }
}
