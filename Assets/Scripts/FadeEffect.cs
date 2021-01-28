using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.UI;

public class FadeEffect : MonoBehaviour
{
    public static FadeEffect _instance;

    [SerializeField] private Image fadeScreen;
    [SerializeField] private float fadeSpeed;
    [SerializeField] private AnimationCurve animationCurve;
    private Coroutine currentCoroutine = null;

    [SerializeField]
    enum Fade_Effect
    {
        FadeIn,
        FadeOut,
        None
    }

    [SerializeField] private Fade_Effect _fadeEffect;

    public bool endFade = false;
    
    private void Awake()
    {
        _instance = this;
    }

    public void Initialize()
    {
        if (_fadeEffect == Fade_Effect.FadeIn)
            Fade_In();
        else if (_fadeEffect == Fade_Effect.FadeOut)
            Fade_Out();
        else
        {
            Color tempColor = fadeScreen.color;
            fadeScreen.color = new Color(tempColor.r, tempColor.g, tempColor.b, 0);
            fadeScreen.raycastTarget = false;
        }
    }
    
    public void Fade_In()
    {
        endFade = false;
        if (currentCoroutine == null)
            currentCoroutine = StartCoroutine(FadeIn());
    }
    public void Fade_Out()
    {
        endFade = false;
        if (currentCoroutine == null)
            currentCoroutine = StartCoroutine(FadeOut());
    }
    
    //Show 1-0
    IEnumerator FadeIn()
    {
        fadeScreen.raycastTarget = true;
        float initialValue = 1;
        Color tempColor = fadeScreen.color;
        while (initialValue >= 0)
        {
            initialValue -= Time.deltaTime * fadeSpeed;
            float animValue = animationCurve.Evaluate(initialValue);
            fadeScreen.color = new Color(tempColor.r, tempColor.g, tempColor.b, animValue);
            yield return null;
        }

        fadeScreen.raycastTarget = false;
        endFade = true;
        currentCoroutine = null;
    }
    
    //Show 0-1
    IEnumerator FadeOut()
    {
        fadeScreen.raycastTarget = false;
        float initialValue = 0;
        Color tempColor = fadeScreen.color;
        while (initialValue <= 1)
        {
            initialValue += Time.deltaTime * fadeSpeed;
            float animValue = animationCurve.Evaluate(initialValue);
            fadeScreen.color = new Color(tempColor.r, tempColor.g, tempColor.b, animValue);
            yield return null;
        }
        
        fadeScreen.raycastTarget = true;
        endFade = true;
        currentCoroutine = null;
    }

}
