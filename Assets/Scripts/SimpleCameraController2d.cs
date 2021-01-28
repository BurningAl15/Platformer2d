using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleCameraController2d : MonoBehaviour
{
    public static SimpleCameraController2d _instance;
    
    [SerializeField] private Transform target;

    [Header("Parallax Layers")]
    [SerializeField] private Transform farBackground, middleBackground;
    private Vector2 lastPos;

    [Header("Camera Height Clamping")] 
    [SerializeField] private float minHeight;
    [SerializeField] private float maxHeight;

    private bool stopFollow = false;

    private void Awake()
    {
        _instance = this;
    }

    private void Start()
    {
        lastPos = transform.position;
    }

    void LateUpdate()
    {
        if (!stopFollow)
        {
            //Horizontal Movement
            transform.position = new Vector3(target.position.x, target.position.y, transform.position.z);

            //Clamping Vertical Camera
            float clampedY = Mathf.Clamp(transform.position.y, minHeight, maxHeight);
            transform.position = new Vector3(transform.position.x, clampedY, transform.position.z);

            //Parallax - Horizontal (X) - Vertical (Y)
            Vector2 amountToMove = new Vector2(transform.position.x - lastPos.x,transform.position.y - lastPos.y);

            farBackground.position += new Vector3(amountToMove.x, amountToMove.y, 0);
            middleBackground.position += new Vector3(amountToMove.x, amountToMove.y, 0) * .5f;
        
            lastPos = transform.position;
        }
    }

    public void StopFollow()
    {
        stopFollow = true;
    }
}
