using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HangingHazzard : MonoBehaviour
{
    [SerializeField] private LineRenderer _lineRenderer;
    [SerializeField] private Transform spikePos;
    [SerializeField] private Transform holdingPointPos;

    private void Awake()
    {
        // _lineRenderer.SetPosition(0,this.transform.localPosition);
        // _lineRenderer.SetPosition(1,spikePos.InverseTransformPoint(spikePos.position));
        
        _lineRenderer.SetPosition(0,holdingPointPos.position);
        _lineRenderer.SetPosition(1,spikePos.position);

    }
}
