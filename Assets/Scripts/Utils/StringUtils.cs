﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StringUtils : MonoBehaviour
{
    public static string tag_Player = "Player";
    public static string tag_Enemy = "Enemy";
    public static string tag_Bouncer = "Bouncer";

    public static string playerPref_mapIndex = "mapIndex";
    public static string Get_World_Level(int _worldNumber, int _levelNumber)
    {
        return "World_" + _worldNumber + "Level_" + _levelNumber;
    }

    public static string Get_Level(int _levelNumber)
    {
        return "Level_" + _levelNumber;
    }

    public static string Get_GemsInLevel(int _levelNumber)
    {
        return "Level_" + _levelNumber + "_Gems";
    }
    public static string Get_TimeInLevel(int _levelNumber)
    {
        return "Level_" + _levelNumber + "_Time";
    }

    public static string Get_GizmosIconNumbers(int _index)
    {
        string iconName = "GizmoIcon_128_";
        return iconName + _index.ToString() +".png";
    }
}
