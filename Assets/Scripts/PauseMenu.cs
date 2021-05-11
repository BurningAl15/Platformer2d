using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PauseMenu : MonoBehaviour
{
    public static PauseMenu _instance;
    
    [SerializeField] private CanvasGroup pauseBG;
    public GameObject pauseScreen;
    public bool isPaused;

    [SerializeField] private float initPoint, endPoint;
    
    private void Awake()
    {
        _instance = this;
        DOTween.Init(true, true, LogBehaviour.Default);
    }

    private void Start()
    {
        TurnCanvasGroup(false);
    }

    /// <summary>
    /// The flow of how this works is:
    /// When we pause:
    /// Turn on the fade canvas
    /// We set the endPoint as endValue for the DOTween DOLocalMoveY function
    /// when we finish, set the TimeScale as 0 (freeze time)
    /// ----------------------------------------------------
    /// When we stop pause:
    /// We set the TimeScale as 1, this is because DoMove works with TimeScale, if is 0, it won't work
    /// We set the initPoint as endValue for the DOTween DOLocalMoveY function
    /// when we finish, set the TimeScale as 1 (freeze time)
    /// We turn off the canvasGroup and fade the alpha to 0
    /// </summary>
    public void Pause()
    {
        isPaused = !isPaused;
        print("Is Paused State: "+ isPaused);

        if (isPaused)
            TurnCanvasGroup(true);
        else
            Time.timeScale = 1;

        float tempEndValue = isPaused ? endPoint : initPoint;
        print(tempEndValue);
        pauseScreen.transform.DOLocalMoveY(tempEndValue, .25f).SetEase(Ease.InOutBounce).OnComplete(TurnOffPause);
    }

    public void PauseAuxiliar()
    {
        print("Calling auxiliar");
        isPaused = false;
        Time.timeScale = 1;
        pauseScreen.transform.DOLocalMoveY(initPoint, .25f).SetEase(Ease.InOutBounce).OnComplete(TurnOffPause);
    }

    void TurnOffPause()
    {
        Time.timeScale = isPaused ? 0 : 1;
        if (!isPaused)
            TurnCanvasGroup(false);
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

    void TurnCanvasGroup(bool _)
    {
        pauseBG.interactable = _;
        pauseBG.blocksRaycasts = _;
        pauseBG.alpha = _ ? 1 : 0;
    }
}
