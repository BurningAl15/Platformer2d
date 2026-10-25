using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class ScreenShake : MonoBehaviour
{
    public static ScreenShake _instance;
    [SerializeField] private float shakeTimeRamaining,shakePower;
    [SerializeField] private float shakeFadeTime;
    [SerializeField] private float shakeRotation;
    [SerializeField] private float rotationMultiplier = 2f;
    
    
    private void Awake()
    {
        _instance = this;
    }

    void Start()
    {
        
    }

    void LateUpdate()
    {
        if (shakeTimeRamaining > 0)
        {
            // SimpleCameraController2d._instance.StopFollow();
            shakeTimeRamaining -= Time.deltaTime;
            float xAmount = Random.Range(-1f, 1f) * shakePower;
            float yAmount = Random.Range(-1f, 1f) * shakePower;

            transform.position += new Vector3(xAmount, yAmount, 0f);

            shakePower = Mathf.MoveTowards(shakePower, 0f, shakeFadeTime*Time.deltaTime);

            shakeRotation = Mathf.MoveTowards(shakeRotation, 0f, shakeFadeTime * rotationMultiplier * Time.deltaTime);
        }
        else
        {
            // SimpleCameraController2d._instance.Follow();
        }

        transform.rotation = Quaternion.Euler(0f, 0f, shakeRotation * Random.Range(-1f, 1f));
    }

    public void StartShake(float length, float power)
    {
        shakeTimeRamaining = length;
        shakePower = power;

        shakeFadeTime = power / length;
        shakeRotation = power * rotationMultiplier;
    }
}
