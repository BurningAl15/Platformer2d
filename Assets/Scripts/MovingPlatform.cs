using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [Header("Movement Setup")]
    [SerializeField] private List<Transform> movingPoints = new List<Transform>();
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float waitTime = 0f;

    [Header("References")]
    [SerializeField] private Transform platformTransform;
    [SerializeField] private Rigidbody2D platformRb;

    private int currentPointIndex = 0;
    private float waitTimer = 0f;
    private Vector2 lastPosition;
    private Vector2 currentVelocity;

    private void Start()
    {
        if (platformTransform == null)
            platformTransform = transform;
            
        platformTransform.position = movingPoints[currentPointIndex].position;
        lastPosition = platformTransform.position;
        waitTimer = waitTime;
    }

    private void FixedUpdate()
    {
        lastPosition = platformTransform.position;

        if (movingPoints.Count == 0) return;

        Vector2 targetPos = movingPoints[currentPointIndex].position;
        Vector2 currentPos = platformTransform.position;
    
        float distance = Vector2.Distance(currentPos, targetPos);

        if (distance > 0.01f)
        {
            Vector2 newPos = Vector2.MoveTowards(currentPos, targetPos, moveSpeed * Time.fixedDeltaTime);
            platformRb.MovePosition(newPos);
        
            currentVelocity = (newPos - lastPosition) / Time.fixedDeltaTime;
        }
        else
        {
            currentVelocity = Vector2.zero;
        
            if (waitTimer > 0)
            {
                waitTimer -= Time.fixedDeltaTime;
            }
            else
            {
                currentPointIndex++;
                if (currentPointIndex >= movingPoints.Count)
                {
                    currentPointIndex = 0;
                }
                waitTimer = waitTime;
            }
        }
    }

    public Vector2 GetVelocity()
    {
        return currentVelocity;
    }

    private void OnDrawGizmos()
    {
        if (movingPoints == null || movingPoints.Count == 0) return;

        Gizmos.color = Color.cyan;
        for (int i = 0; i < movingPoints.Count; i++)
        {
            if (movingPoints[i] != null)
            {
                Gizmos.DrawWireSphere(movingPoints[i].position, 0.3f);
                
                if (i < movingPoints.Count - 1 && movingPoints[i + 1] != null)
                {
                    Gizmos.DrawLine(movingPoints[i].position, movingPoints[i + 1].position);
                }
                else if (i == movingPoints.Count - 1 && movingPoints[0] != null)
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawLine(movingPoints[i].position, movingPoints[0].position);
                }
            }
        }
        
        if (Application.isPlaying)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(transform.position, 0.2f);
        }
    }
}
