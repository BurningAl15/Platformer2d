using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapPointsHolder : MonoBehaviour
{
    //Here we save the lock or unlock of levels
    [SerializeField] private List<MapPoint> _mapPoints = new List<MapPoint>();

    //Set the current position of the player too
    [SerializeField] private LSPlayerController player;

    private int mapIndex;
    [SerializeField] private MapPoint currentMapPoint;
    
    void Start()
    {
        if (PlayerPrefs.HasKey("map_index"))
        {
            mapIndex = PlayerPrefs.GetInt("map_index");
            currentMapPoint = _mapPoints[mapIndex];
        }
        else
        {
            mapIndex = 0;
            currentMapPoint = _mapPoints[mapIndex];
        }
        player.SetCurrentPoint(currentMapPoint);
    }
    
    //When finish level then check current level and unlock next
}
