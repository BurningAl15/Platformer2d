using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
   public GameObject continueButton;

   [SerializeField] private Image textImg;
   [SerializeField] private CanvasGroup panelBG;
   [SerializeField] private GameObject panelOptions;

   [SerializeField] private float initPoint, endPoint;
   [SerializeField] private AnimationCurve initCurve;
   
   private void Awake()
   {
      DOTween.Init(true, true, LogBehaviour.Default);
   }

   private void Start()
   {
      if(PlayerPrefs.HasKey("Level_1"))
         continueButton.SetActive(true);
      else
         continueButton.SetActive(false);

      panelBG.alpha = 0;
      panelBG.interactable = false;
      panelBG.blocksRaycasts = false;
   }

   public void DeactivatePanel()
   {
      panelOptions.transform.DOLocalMoveY(initPoint, .5f).SetEase(initCurve).OnComplete(()=>TurnOffPanel());
   }

   void TurnOffPanel()
   {
      panelBG.alpha = 0;
      panelBG.interactable = false;
      panelBG.blocksRaycasts = false;
   }
   
   public void ActivatePanel()
   {
      if (PlayerPrefs.HasKey("Level_1"))
      {
         panelBG.alpha = 1;
         panelBG.interactable = true;
         panelBG.blocksRaycasts = true;
         panelOptions.transform.DOLocalMoveY(endPoint, .5f).SetEase(initCurve);
      }
      else
      {
         ToNextScene();
      }
   }
   
   public void ToNextScene()
   {
      PlayerPrefs.DeleteAll();
      //We create 3 playerprefs per level:
      //Level_Number
      //Level_Number_Gems
      //Level_Number_Time
      for (int i = 0; i < 10; i++)
      {
         int j = i + 1;
         // string _name = "World_" + worldNumber + "Level_" + j;
         PlayerPrefs.SetInt(StringUtils.Get_Level(j), 0);
         PlayerPrefs.SetInt(StringUtils.Get_GemsInLevel(j), 0);
         PlayerPrefs.SetFloat(StringUtils.Get_TimeInLevel(j), 99999);
      }

      PlayerPrefs.Save();
      SceneUtils.LoadGameplayScene(0);
   }

   public void ContinueScene()
   {
       SceneUtils.ToSelectionScene();
   }

   public void QuitGame()
   {
      Application.Quit();
   }
}
