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

    [SerializeField] private int worldNumber;
    
    void Start()
    {
        if (PlayerPrefs.HasKey("Level_1"))
        {
            for (int i = 1; i < _mapPoints.Count; i++)
            {
                int j = i + 1;
                _mapPoints[i].isLocked = PlayerPrefs.GetInt(StringUtils.Get_Level(j)) != 1;
            }

            for (int i = 0; i < _mapPoints.Count; i++)
            {
                int j = i + 1;
                _mapPoints[i].gemsCollected = PlayerPrefs.GetInt(StringUtils.Get_GemsInLevel(j));
                _mapPoints[i].CheckGems();
            }
            for (int i = 0; i < _mapPoints.Count; i++)
            {
                int j = i + 1;
                _mapPoints[i].bestTime = PlayerPrefs.GetFloat(StringUtils.Get_TimeInLevel(j));
                _mapPoints[i].CheckTime();
            }
            
            mapIndex = PlayerPrefs.GetInt(StringUtils.playerPref_mapIndex);
            currentMapPoint = _mapPoints[mapIndex];
        }
        else
        {
            for (int i = 0; i < _mapPoints.Count; i++)
            {
                int j = i + 1;
                // string _name = "World_" + worldNumber + "Level_" + j;
                PlayerPrefs.SetInt(StringUtils.Get_Level(j), 0);
            }
            for (int i = 0; i < _mapPoints.Count; i++)
            {
                int j = i + 1;
                PlayerPrefs.SetInt(StringUtils.Get_GemsInLevel(j), 0);
            }
            for (int i = 0; i < _mapPoints.Count; i++)
            {
                int j = i + 1;
                PlayerPrefs.SetInt(StringUtils.Get_TimeInLevel(j), 0);
            }

            PlayerPrefs.Save();
            
            mapIndex = 0;
            currentMapPoint = _mapPoints[mapIndex];
        }
        player.SetCurrentPoint(currentMapPoint);
    }
    
    //When finish level then check current level and unlock next
}
