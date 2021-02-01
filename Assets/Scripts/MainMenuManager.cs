using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
   public GameObject continueButton;
   
   private void Start()
   {
      if(PlayerPrefs.HasKey("Level_1"))
         continueButton.SetActive(true);
      else
         continueButton.SetActive(false);
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
