using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [SerializeField] private List<Transform> movingPoints = new List<Transform>();
    [SerializeField] private float moveSpeed;

    [SerializeField] int currentPoint;

    [SerializeField] Transform platform;

    private void Start()
    {
        platform.position = movingPoints[currentPoint].position;
    }

    private void Update()
    {
        platform.position = Vector3.MoveTowards(platform.position, movingPoints[currentPoint].position, moveSpeed*Time.deltaTime);
        if (Vector3.Distance(platform.position, movingPoints[currentPoint].position) <= Mathf.Epsilon)
        {
            currentPoint++;
            if (currentPoint >= movingPoints.Count)
                currentPoint = 0;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0, 0, 1);
        for (var i = 0; i < movingPoints.Count; i++)
            Gizmos.DrawIcon(movingPoints[i].transform.position, StringUtils.Get_GizmosIconNumbers(i));
    }
}
