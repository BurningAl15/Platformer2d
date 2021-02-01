using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager _instance;
    [SerializeField] private float respawnMaxTime;

    private Coroutine currentCoroutine = null;

    int gemsCollected = 0;

    public bool stopGame = false;

    private string levelname;

    public float timeInLevel;
    
    private void Awake()
    {
        _instance = this;
    }

    private void Start()
    {
        UIController._instance.UpdateGems_UI(gemsCollected);
        timeInLevel = 0f;
    }

    private void Update()
    {
        timeInLevel += Time.deltaTime;
    }

    public void UpdateGemsCollected()
    {
        gemsCollected++;
        UIController._instance.UpdateGems_UI(gemsCollected);
    }
    
    public void RespawnPlayer()
    {
        if (currentCoroutine == null)
            currentCoroutine = StartCoroutine(Respawning());
    }

    IEnumerator Respawning()
    {
        PlayerHealth._instance.DeathEffect();
        PlayerHealth._instance.TurnOffPlayer();
        yield return new WaitUntil(() => PlayerHealth._instance.isDeathEffectEnd());
        FadeEffect._instance.Fade_Out();
        yield return new WaitUntil(() => FadeEffect._instance.endFade);
        yield return new WaitForSeconds(respawnMaxTime);
        PlayerHealth._instance.ResetHealth();
        UIController._instance.ResetHearts();
        PlayerHealth._instance.ResetPlayer();
        PlayerHealth._instance.ResetDeathEffect();
        FadeEffect._instance.Fade_In();
        currentCoroutine = null;
    }

    void SetPlayerPrefsLevelToUnlock()
    {
        // if (SceneUtils.Get_CurrentLevelName() == 1)
        //     PlayerPrefs.SetInt(StringUtils.Get_Level(SceneUtils.Get_CurrentLevelName()), 1);
        PlayerPrefs.SetInt(StringUtils.Get_Level(SceneUtils.Get_NextLevelName()), 1);
        PlayerPrefs.SetInt(StringUtils.playerPref_mapIndex, SceneUtils.Get_NextLevelName() - 2);
        PlayerPrefs.Save();
    }

    void SetPlayerPrefsGems()
    {
        if (PlayerPrefs.HasKey(StringUtils.Get_GemsInLevel(SceneUtils.Get_CurrentLevelName())))
        {
            if(gemsCollected>PlayerPrefs.GetInt(StringUtils.Get_GemsInLevel(SceneUtils.Get_CurrentLevelName())))
            {
                print("Gems");
                PlayerPrefs.SetInt(StringUtils.Get_GemsInLevel(SceneUtils.Get_CurrentLevelName()), gemsCollected);
                print("Saved Gems: " + gemsCollected);
            }
        }
        else
        {
            PlayerPrefs.SetInt(StringUtils.Get_GemsInLevel(SceneUtils.Get_CurrentLevelName()), gemsCollected);
        }
        
        PlayerPrefs.Save();
    }

    void SetPlayerPrefsTime()
    {
        if (PlayerPrefs.HasKey(StringUtils.Get_TimeInLevel(SceneUtils.Get_CurrentLevelName())))
        {
            print("Time 1 - Test");
            if(timeInLevel < PlayerPrefs.GetFloat(StringUtils.Get_TimeInLevel(SceneUtils.Get_CurrentLevelName())))
            {
                print("Time 2 - Test");
                PlayerPrefs.SetFloat(StringUtils.Get_TimeInLevel(SceneUtils.Get_CurrentLevelName()), timeInLevel);
                print("Saved Time: " + timeInLevel);
            }
        }
        else
        {
            print("Time 3 - Test");
            PlayerPrefs.SetFloat(StringUtils.Get_TimeInLevel(SceneUtils.Get_CurrentLevelName()), timeInLevel);
        }

        PlayerPrefs.Save();
    }

    public void EndLevel()
    {
        if (currentCoroutine == null)
            currentCoroutine = StartCoroutine(End_Level());
    }

    IEnumerator End_Level()
    {
        stopGame = true;
        AudioMixerManager._instance.PlayBackgroundSource(false);
        SimpleCameraController2d._instance.StopFollow();
        yield return new WaitForSeconds(.5f);
        UIController._instance.Run_EndLevelAnimation();
        yield return new WaitForSeconds(1.5f);
        FadeEffect._instance.Fade_Out();
        SetPlayerPrefsLevelToUnlock();
        if (SceneUtils.Get_CurrentLevelName() != 0)
        {
            SetPlayerPrefsGems();
            SetPlayerPrefsTime();
        }
        yield return new WaitUntil(() => FadeEffect._instance.endFade);
        yield return new WaitForSeconds(.25f);
        SceneUtils.ToSelectionScene();
    }
}
