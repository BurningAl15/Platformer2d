using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [SerializeField] private List<Transform> movingPoints = new List<Transform>();
    [SerializeField] private float moveSpeed;
    [SerializeField] private float maxTimer;
    [SerializeField] private Rigidbody2D rgb;

    private int currentPoint;
    private float timer;

    private Vector2 lastPosition;
    private Vector2 currentPosition;
    private Vector2 platformVelocity;

    void Start()
    {
        rgb.position = movingPoints[currentPoint].position;
        currentPosition = rgb.position;
        lastPosition = currentPosition;
        timer = maxTimer;
    }

    void FixedUpdate()
    {
        Vector2 targetPos = movingPoints[currentPoint].position;
        Vector2 newPos = Vector2.MoveTowards(currentPosition, targetPos, moveSpeed * Time.fixedDeltaTime);
        rgb.MovePosition(newPos);

        // Compute platform velocity
        currentPosition = newPos;
        platformVelocity = (currentPosition - lastPosition) / Time.fixedDeltaTime;
        lastPosition = currentPosition;

        // Check if reached the waypoint
        if (Vector2.Distance(currentPosition, targetPos) <= Mathf.Epsilon)
        {
            timer -= Time.fixedDeltaTime;
            if (timer < 0f)
            {
                currentPoint = (currentPoint + 1) % movingPoints.Count;
                timer = maxTimer;
            }
        }
    }

    public Vector2 GetVelocity()
    {
        return platformVelocity;
    }
}