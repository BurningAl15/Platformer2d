using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapPoint : MonoBehaviour
{
   public MapPoint up, down, right, left;
   public bool isLevel;
   public bool isLocked;

   public int currentLevel;

   public string levelName;

   public int gemsCollected, totalGems;
   public float bestTime, targetTime;
   
   private void OnDrawGizmos()
   {
      if (up)
      {
         Gizmos.color = new Color(1, 0, 0);
         Gizmos.DrawLine(transform.position,up.transform.position);
      }
      if (down)
      {
         Gizmos.color = new Color(0, 1, 0);
         Gizmos.DrawLine(transform.position,down.transform.position);
      }
      if (right)
      {
         Gizmos.color = new Color(0, 0, 1);
         Gizmos.DrawLine(transform.position,right.transform.position);
      }
      if (left)
      {
         Gizmos.color = new Color(1, 1, 1);
         Gizmos.DrawLine(transform.position,left.transform.position);
      }
   }
}
