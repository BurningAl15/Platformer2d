using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Smasher : MonoBehaviour
{
    [SerializeField] private SmasherSignal _smasherSignal;
    [SerializeField] private Transform smasher;
    [SerializeField] private Vector3 initialPoint;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float initialMoveSpeed;
    [SerializeField] private AnimationCurve _animationCurve;
    [SerializeField] private ParticleSystem _particleSystem;
    
    private float timming;
    
    
    void Start()
    {
        initialMoveSpeed = moveSpeed;
        initialPoint = smasher.position;
        _particleSystem.Stop();
    }

    void Update()
    {
        if (_smasherSignal.smasherOn)
        {
            // float yValue = Mathf.InverseLerp(initialPoint.y, _smasherSignal.transform.position.y, smasher.position.y);
            // *animValue
            // moveSpeed += Time.deltaTime*3;
            timming += Time.deltaTime;
            float animValue = _animationCurve.Evaluate(timming)*3;

            smasher.position = Vector3.MoveTowards(smasher.position,_smasherSignal.transform.position,moveSpeed*Time.deltaTime*animValue);
            if (Vector3.Distance(smasher.position, _smasherSignal.transform.position) < Mathf.Epsilon)
            {
                _particleSystem.Play();
                ScreenShake._instance.StartShake(.25f,.5f);
                AudioMixerManager._instance.CallSFX(SFXType.Smash_Impact);
                _smasherSignal.smasherOn = false;
            }
        }
        else
        {
            moveSpeed = initialMoveSpeed;
            timming = 0;
            smasher.position = Vector3.MoveTowards(smasher.position,initialPoint,moveSpeed*Time.deltaTime);
        }
    }
}
