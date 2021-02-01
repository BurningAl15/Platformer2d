using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class UI_SelectController : MonoBehaviour
{
    public static UI_SelectController _instance;
    [FormerlySerializedAs("levelInfo")] [SerializeField] private GameObject levelInfoContainer;

    [FormerlySerializedAs("levelName")] [SerializeField] private TextMeshProUGUI levelNameText;

    [Header("Gems")]
    [FormerlySerializedAs("gemsFound")] [SerializeField] private TextMeshProUGUI gemsFoundText;
    [FormerlySerializedAs("gemsTarget")] [SerializeField] private TextMeshProUGUI gemsTargetText;
    
    [Header("Time")]
    [FormerlySerializedAs("bestTime")] [SerializeField] private TextMeshProUGUI bestTimeText;
    [FormerlySerializedAs("timeTarget")] [SerializeField] private TextMeshProUGUI timeTargetText;
    
    private void Awake()
    {
        _instance = this;
        Turn_On_Off(false);
    }

    // public void RenderLevelData(int levelNumber)
    // {
    //     levelName.text = "Level " + levelNumber;
    // }
    
    public void RenderLevelData(MapPoint levelInfo)
    {
        levelNameText.text = levelInfo.levelName;
        gemsFoundText.text = "FOUND: "+ levelInfo.gemsCollected;
        gemsTargetText.text = "IN LEVEL: "+ levelInfo.totalGems;
        timeTargetText.text = "TARGET: " + levelInfo.targetTime.ToString("F1") + "s";

        if (levelInfo.bestTime == 99999)
        {
            bestTimeText.text = "Best: ----";
        }
        else
        {
            bestTimeText.text = "Best: " + levelInfo.bestTime.ToString("F1") + "s";
        }        
        
    }

    public void Turn_On_Off(bool _turn)
    {
        levelInfoContainer.SetActive(_turn);
    }
}
