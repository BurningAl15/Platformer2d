using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PendulumChainPoint
{
    public Transform myPivotTransform;
    public float maxAngleDeflection;
    public float SpeedOfPendulum = 1.0f;
}

public class PendulumChain : MonoBehaviour
{
    [SerializeField] private List<PendulumChainPoint> _pendulumChainPoints = new List<PendulumChainPoint>();
    
    void Update()
    {
        for (int i = 0; i < _pendulumChainPoints.Count; i++)
            PendulumMovement(_pendulumChainPoints[i].myPivotTransform,_pendulumChainPoints[i].maxAngleDeflection,_pendulumChainPoints[i].SpeedOfPendulum);
    }

    void PendulumMovement(Transform _pivot, float _deflectionAngle, float _speedPendulum)
    {
        float angle = _deflectionAngle * Mathf.Sin( Time.time * _speedPendulum);
        _pivot.localRotation = Quaternion.Euler( 0, 0, angle);
    }
}
