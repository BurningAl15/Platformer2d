using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathZone : MonoBehaviour
{
  [SerializeField] private BoxCollider2D _boxCollider;
  [SerializeField] private CircleCollider2D _circleCollider;
  private Vector3 position_Offset;

  private void Awake()
  {
    // position_Offset = (new Vector3(_boxCollider.offset.x, _boxCollider.offset.y, 0) + transform.position);
  }

  private void OnTriggerEnter2D(Collider2D other)
  {
    // Player colliders can live on children such as StompBox. Resolve the
    // owning player instead of relying on the child collider's tag.
    PlayerHealth player = other.GetComponentInParent<PlayerHealth>();
    if (player != null)
    {
      // Falls use the same health/invulnerability rules as other hazards, then
      // return the player to the last waypoint without charging a global life.
      player.DealFallDamage();
      return;
    }

    if(other.CompareTag("EnemyContainer"))
      Destroy(other.gameObject);
    if(other.CompareTag("FallingLog"))
      other.GetComponent<FallingLog>().ResetLog();
  }

  private void OnDrawGizmos()
  {
    Gizmos.color = new Color(1, 0, 0,.25f);
    
    if(_boxCollider != null)
      Gizmos.DrawCube(transform.position,_boxCollider.size);
    if(_circleCollider != null)
      Gizmos.DrawSphere(transform.position,_circleCollider.radius);
  }
}
