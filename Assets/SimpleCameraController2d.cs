using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleCameraController2d : MonoBehaviour
{
    [SerializeField] private Transform target;

    [Header("Parallax Layers")]
    [SerializeField] private Transform farBackground, middleBackground;

    private float lastXPos;

    private void Start()
    {
        lastXPos = transform.position.x;
    }

    void LateUpdate()
    {
        //Horizontal Movement
        transform.position = new Vector3(target.position.x, transform.position.y, transform.position.z);
        
        float amountToMoveX = transform.position.x - lastXPos;

        farBackground.position += new Vector3(amountToMoveX, 0, 0);
        middleBackground.position += new Vector3(amountToMoveX*.5f,0,0);
        
        lastXPos = transform.position.x;
    }
}
