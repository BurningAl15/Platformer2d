using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathZone : MonoBehaviour
{
  [SerializeField] private BoxCollider2D _boxCollider;
  private Vector3 position_Offset;

  private void Awake()
  {
    // position_Offset = (new Vector3(_boxCollider.offset.x, _boxCollider.offset.y, 0) + transform.position);
  }

  private void OnTriggerEnter2D(Collider2D other)
  {
    if(other.CompareTag("Player"))
      LevelManager._instance.RespawnPlayer();
    if(other.CompareTag("EnemyContainer"))
      Destroy(other.gameObject);
  }

  private void OnDrawGizmos()
  {
    Gizmos.color = new Color(1, 0, 0,.25f);
    
    Gizmos.DrawCube(transform.position,_boxCollider.size);
  }
}
