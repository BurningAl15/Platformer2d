using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleCameraController2d : MonoBehaviour
{
    [SerializeField] private Transform target;
    
    void LateUpdate()
    {
        //Horizontal Movement
        transform.position = new Vector3(target.position.x, transform.position.y, transform.position.z);
    }
}
