using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveMovement : EnemyParent
{
    public float timePeriod_Height = 2;
    public float height = 1f;
    public float timePeriod_Width = 2;
    public float width = 2f;
    private float timeSinceStart_Height;
    private float timeSinceStart_Width;
    private Vector3 pivot;

    private void Start()
    {
        pivot = transform.position;
        height /= 2;
        width /= 2;
        timeSinceStart_Height = (3 * timePeriod_Height) / 4;
        timeSinceStart_Width = (3 * timePeriod_Width) / 4;
    }
    void Update()
    {
        Vector3 nextPos = transform.position;
        nextPos.x = pivot.x + width + width * Mathf.Sin(((Mathf.PI * 2) / timePeriod_Width) * timeSinceStart_Width);
        nextPos.y = pivot.y + height + height * Mathf.Sin(((Mathf.PI * 2) / timePeriod_Height) * timeSinceStart_Height);
       
        timeSinceStart_Height += Time.deltaTime;
        timeSinceStart_Width += Time.deltaTime;
        
        FlipSprite(nextPos);
        
        transform.position = nextPos;
    }
    
    private void FlipSprite(Vector2 _nextPos)
    {
        //By rotations
        // transform.eulerAngles = new Vector3(0, direction == 1 ? 0 : 180, 0);
        //By spriteRenderer
        _spriteRenderer.flipX =  _nextPos.x > transform.position.x;
    }
    
    // private void OnDrawGizmos()
    // {
    //     Gizmos.DrawIcon(transform.position + Vector3.right*width, StringUtils.Get_GizmosIconNumbers(0));
    //     Gizmos.DrawIcon(transform.position, StringUtils.Get_GizmosIconNumbers(1));
    // }
}
