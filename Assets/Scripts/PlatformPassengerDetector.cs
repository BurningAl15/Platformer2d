using UnityEngine;

public class PlatformPassengerDetector : MonoBehaviour
{
    [SerializeField] private MovingPlatform movingPlatform;
    [SerializeField] private Vector2 detectionSize = new Vector2(5f, 4f);
    [SerializeField] private Vector2 detectionOffset = new Vector2(0f, 1.5f);
    [SerializeField] private LayerMask stompBoxLayer;
    
    private PlayerController2d currentPassenger = null;

    private void Start()
    {
        if (movingPlatform == null)
        {
            movingPlatform = GetComponentInParent<MovingPlatform>();
        }
        
        if (stompBoxLayer == 0)
        {
            stompBoxLayer = LayerMask.GetMask("StompBox");
        }
        
        Debug.Log($"[DETECTOR] {transform.parent.name} initialized. Layer mask: {stompBoxLayer.value}");
    }

    // private void FixedUpdate()
    // {
    //     Vector2 checkPosition = (Vector2)transform.position + detectionOffset;
    //     Collider2D hit = Physics2D.OverlapBox(checkPosition, detectionSize, 0f, stompBoxLayer);
    //     
    //     if (hit != null && hit.CompareTag("StompBox"))
    //     {
    //         if (currentPassenger == null)
    //         {
    //             PlayerController2d player = hit.GetComponentInParent<PlayerController2d>();
    //             if (player != null && movingPlatform != null)
    //             {
    //                 currentPassenger = player;
    //                 movingPlatform.AddPassenger(player);
    //                 Debug.Log($"[DETECTOR] ✅ CAPTURED on {transform.parent.name}!");
    //             }
    //         }
    //     }
    //     else
    //     {
    //         if (currentPassenger != null)
    //         {
    //             Debug.Log($"[DETECTOR] ❌ RELEASING from {transform.parent.name}");
    //             movingPlatform.RemovePassenger(currentPassenger);
    //             currentPassenger = null;
    //         }
    //     }
    // }

    private void OnDrawGizmos()
    {
        Vector2 pos = (Vector2)transform.position + detectionOffset;
        Gizmos.color = currentPassenger != null ? Color.green : Color.yellow;
        Gizmos.DrawWireCube(pos, detectionSize);
    }
}
