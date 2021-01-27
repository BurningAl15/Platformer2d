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

   public static void LoadScene(int _sceneNumber)
   {
      SceneManager.LoadScene("Level" + _sceneNumber);
   }
   
   public static void ToSelectionScene()
   {
      SceneManager.LoadScene("SelectionScreen");
   }

   public static void ToMainScene()
   {
      SceneManager.LoadScene("MainScreen");
   }

   public static bool IsInGameplay()
   {
      return SceneManager.GetActiveScene().name.Contains("Level");
   }
}
