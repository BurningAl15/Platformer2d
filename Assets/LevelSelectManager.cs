using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSelectManager : MonoBehaviour
{
    public static LevelSelectManager _instance;
    private Coroutine currentCoroutine = null;

    void Awake()
    {
        _instance = this;
    }

    public void Loading_GameplayScene(int _level)
    {
        if (currentCoroutine == null)
            currentCoroutine = StartCoroutine(LoadingGameplayScene(_level));
    }
    
    IEnumerator LoadingGameplayScene(int _level)
    {
        FadeEffect._instance.Fade_Out();
        yield return new WaitUntil(() => FadeEffect._instance.endFade);
        yield return new WaitForSeconds(.25f);
        SceneUtils.LoadGameplayScene(_level);
        currentCoroutine = null;
    }
}
