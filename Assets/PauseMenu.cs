using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public static PauseMenu _instance;
    
    public GameObject pauseScreen;
    public bool isPaused;

    private void Awake()
    {
        _instance = this;
    }

    private void Start()
    {
        pauseScreen.SetActive(isPaused);
    }

    public void Pause()
    {
        isPaused = !isPaused;
        pauseScreen.SetActive(isPaused);
        Time.timeScale = isPaused ? 0 : 1;
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
            Pause();
    }

    public void LevelSelection()
    {
        SceneUtils.ToSelectionScene();
        Time.timeScale = 1;
    }

    public void MainMenu()
    {
        SceneUtils.ToMainScene();
        Time.timeScale = 1;
    }
}
