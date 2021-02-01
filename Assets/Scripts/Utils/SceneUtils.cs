using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneUtils : MonoBehaviour
{
   public static void NextScene()
   {
      SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
   }

   public static void LoadScene(string _sceneNumber)
   {
      SceneManager.LoadScene(_sceneNumber);
   }
   
   public static void LoadScene(int _sceneNumber)
   {
      SceneManager.LoadScene(_sceneNumber);
   }
   
   public static void LoadGameplayScene(int _level)
   {
      SceneManager.LoadScene("Level_" + _level);
   }
   
   public static void ToSelectionScene()
   {
      SceneManager.LoadScene("LevelSelection");
   }

   public static void ToMainScene()
   {
      SceneManager.LoadScene("MainMenu");
   }

   public static bool IsInGameplay()
   {
      return SceneManager.GetActiveScene().name.Contains("Level_");
   }

   public static int Get_NextLevelName()
   {
      int sceneNumber = 0;
      string tempSceneName = SceneManager.GetActiveScene().name;
      char[] separator = new char[] {'_'};
      
      string[] subs = tempSceneName.Split(separator);
      sceneNumber = Int32.Parse(subs[1]) + 1;
      
      return sceneNumber;
   }
   
   public static int Get_CurrentLevelName()
   {
      int sceneNumber = 0;
      string tempSceneName = SceneManager.GetActiveScene().name;
      char[] separator = new char[] {'_'};
      
      string[] subs = tempSceneName.Split(separator);
      sceneNumber = Int32.Parse(subs[1]);
      
      return sceneNumber;
   }

}