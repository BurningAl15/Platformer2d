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

    private float timer = 0;
    [SerializeField] private AnimationCurve _animationCurve;

    [SerializeField] private Camera cam;
    
    private void Awake()
    {
        _instance = this;
    }

    private void Start()
    {
        target = PlayerController2d._instance.transform;
        lastPos = transform.position;
    }

    void Update()
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
        // else
        // {
        //     if (transform.position.y != target.position.y)
        //     {
        //         timer += Time.deltaTime;
        //         float val = _animationCurve.Evaluate(timer);
        //         float clampedY = Mathf.Lerp(transform.position.y, maxHeight,val);
        //         transform.position = new Vector3(transform.position.x, clampedY, transform.position.z);
        //     }
        // }
    }

    public void StopFollow()
    {
        stopFollow = true;
    }

    public void Follow()
    {
        stopFollow = false;
    }

    private void OnDrawGizmos()
    {
        Matrix4x4 temp = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
        
        if (cam.orthographic) {
            float spread = cam.farClipPlane - cam.nearClipPlane;
            float center = (cam.farClipPlane + cam.nearClipPlane)*0.5f;
            Gizmos.color = new Color(0, 1, 0);
            Gizmos.DrawWireCube(new Vector3(transform.position.x,minHeight,transform.position.z), new Vector3(cam.orthographicSize*2*cam.aspect, cam.orthographicSize*2, spread));
            Gizmos.color = new Color(1, 0, 0);
            Gizmos.DrawWireCube(new Vector3(transform.position.x,maxHeight,transform.position.z), new Vector3(cam.orthographicSize*2*cam.aspect, cam.orthographicSize*2, spread));
        } else {
            Gizmos.DrawFrustum(new Vector3(transform.position.x,minHeight,transform.position.z), cam.fieldOfView, cam.farClipPlane, cam.nearClipPlane, cam.aspect);
            Gizmos.DrawFrustum(new Vector3(transform.position.x,maxHeight,transform.position.z), cam.fieldOfView, cam.farClipPlane, cam.nearClipPlane, cam.aspect);
        }
        Gizmos.matrix = temp;

    }
}
