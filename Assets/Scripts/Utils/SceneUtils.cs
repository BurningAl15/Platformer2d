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
      return SceneManager.GetActiveScene().name.Contains("Level");
   }
}
