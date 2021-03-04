using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingEdge : MonoBehaviour
{
    [SerializeField] private List<Transform> movingPoints = new List<Transform>();
    [SerializeField] private float moveSpeed;

    private int currentPoint;

    [SerializeField] private SpriteRenderer _spriteRenderer;
    
    void Start()
    {
        for (var i = 0; i < movingPoints.Count; i++)
            movingPoints[i].parent = null;
        transform.position = movingPoints[currentPoint].position;
        // FlipSprite(currentPoint);
    }

    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position,
            movingPoints[currentPoint].position, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, movingPoints[currentPoint].position) <= Mathf.Epsilon)
        {
            currentPoint++;
            if (currentPoint >= movingPoints.Count)
                currentPoint = 0;
            FlipSprite();
        }
    }
    
    private void FlipSprite()
    {
        //By rotations
        // transform.eulerAngles = new Vector3(0, direction == 1 ? 0 : 180, 0);
        //By spriteRenderer
        _spriteRenderer.flipX = movingPoints[currentPoint].position.x > transform.position.x;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 0, 0, .25f);
        for (var i = 0; i < movingPoints.Count; i++)
            Gizmos.DrawIcon(movingPoints[i].transform.position, StringUtils.Get_GizmosIconNumbers(i));
    }
}
