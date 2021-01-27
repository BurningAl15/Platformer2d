using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Testing : MonoBehaviour
{
    [SerializeField] private GameObject instanceObj;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            Instantiate(instanceObj, transform.position, Quaternion.identity);
    }
}
