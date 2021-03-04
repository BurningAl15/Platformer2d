using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PendulumMovement : MonoBehaviour
{
    [SerializeField] private Transform myPivotTransform;
    [SerializeField] float MaxAngleDeflection = 30.0f;
    float SpeedOfPendulum = 1.0f;

    private void Update()
    {
        float angle = MaxAngleDeflection * Mathf.Sin( Time.time * SpeedOfPendulum);
        myPivotTransform.localRotation = Quaternion.Euler( 0, 0, angle);
    }
}
