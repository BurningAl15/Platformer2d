using System;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public enum SFXType
{
    Player_Jump,
    Player_Pickup_Gem,
    Player_Pickup_Health,
    Player_Hurt,
    Player_Death,
        
    Enemy_Death,
        
    Boss_Hit,
    Boss_Impact,
    Boss_Shoot,
     
    Level_Selected,
    Map_Movement,
    Warp_Jingle
}

public class AudioMixerManager : MonoBehaviour
{
    public static AudioMixerManager _instance;
    [SerializeField] private AudioMixer mixer;

    private bool isMusicOn = true;
    private bool isSFXOn = true;

    [Header("Music Sprites and Images")]
    
    [SerializeField] private Image musicButton, sfxButton;
    [SerializeField] private Sprite musicOn, musicOff;
    [SerializeField] private Sprite sfxOn, sfxOff;

    [Header("Background Music")]
    [SerializeField] private AudioClip background_Intro_Clip;
    [SerializeField] private AudioClip backgroundClip;
    [SerializeField] private AudioClip goodEndClip, badEndClip;
    [SerializeField] private AudioSource backgroundSource;
    [SerializeField] private AudioSource background2Source;

    [Header("SFX")]
    [SerializeField] private AudioClip click_1_Clip ;
    [SerializeField] private AudioClip click_2_Clip;
    [SerializeField] private AudioClip success_Clip ,fail_Clip;

    [SerializeField] private AudioClip levelSelected_Clip;
    [SerializeField] private AudioClip mapMovement_Clip;
    [SerializeField] private AudioClip warpJingle_Clip;

    [Header("Boss SFX")] 
    [SerializeField] private AudioClip bossHit_Clip;
    [SerializeField] private AudioClip bossImpact_Clip;
    [SerializeField] private AudioClip bossShoot_Clip;
    
    [Header("Death Effect SFX")]
    [SerializeField] private AudioClip deathEffect_Enemy_Clip;
    
    [Header("Player SFX")]
    [SerializeField] private AudioClip deathEffect_Player_Clip;
    [SerializeField] private AudioClip pickup_gem_clip;
    [SerializeField] private AudioClip pickup_helth_clip;
    [SerializeField] private AudioClip jump_Clip;
    [SerializeField] private AudioClip hurt_Clip;
    
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource gems_sfxSource;

    private Coroutine currentCoroutine = null;
    
    public void CallSFX(SFXType sfxType)
    {
        AudioClip clip = null;
        switch (sfxType)
        {
            case SFXType.Player_Pickup_Health:
                clip = pickup_helth_clip;
                sfxSource.pitch = 1;
                break;
            case SFXType.Player_Jump:
                clip = jump_Clip;
                sfxSource.pitch = Random.Range(.8f, 1.2f);
                break;          
            case SFXType.Player_Hurt:
                clip = hurt_Clip;
                sfxSource.pitch = Random.Range(.8f, 1.2f);
                break;
            case SFXType.Player_Death:
                clip = deathEffect_Player_Clip;
                sfxSource.pitch = Random.Range(.8f, 1.2f);
                break;
            case SFXType.Enemy_Death:
                clip = deathEffect_Enemy_Clip;
                sfxSource.pitch = Random.Range(.8f, 1.2f);
                break;
            case SFXType.Boss_Hit:
                clip = bossHit_Clip;
                sfxSource.pitch = 1;
                break;
            case SFXType.Boss_Impact:
                clip = bossImpact_Clip;
                sfxSource.pitch = 1;
                break;
            case SFXType.Boss_Shoot:
                clip = bossShoot_Clip;
                sfxSource.pitch = 1;
                break;
            case SFXType.Level_Selected:
                clip = levelSelected_Clip;
                sfxSource.pitch = Random.Range(.8f, 1.2f);
                break;
            case SFXType.Map_Movement:
                clip = mapMovement_Clip;
                sfxSource.pitch = 1;
                break;
            case SFXType.Warp_Jingle:
                clip = warpJingle_Clip;
                sfxSource.pitch = 1;
                break;
        }
        
        sfxSource.PlayOneShot(clip);
    }

    public void CallSFX_Gems()
    {
        gems_sfxSource.Stop();
        gems_sfxSource.pitch = Random.Range(.9f, 1.1f);
        gems_sfxSource.clip = pickup_gem_clip;
        gems_sfxSource.Play();
    }

    private void Awake()
    {
        if (_instance == null)
            _instance = this;
        else if(_instance!=null)
            Destroy(this.gameObject);
    }

    private void Start()
    {
        // if (!PlayerPrefs.HasKey("MusicLevel"))
        //     PlayerPrefs.SetInt("MusicLevel", 1);
        //
        // if (!PlayerPrefs.HasKey("SFXLevel"))
        //     PlayerPrefs.SetInt("SFXLevel", 1);
        //
        // isMusicOn = PlayerPrefs.GetInt("MusicLevel") == 1;
        // isSFXOn = PlayerPrefs.GetInt("SFXLevel") == 1;

        // isMusicOn = SaveDataManager._instance.gameData.musicLevel;
        // isSFXOn = SaveDataManager._instance.gameData.sfxLevel;
        
        // CheckSFXValue();
        // CheckBackgroundValue();
        //
        // CheckButton_Music();
        // CheckButton_SFX();

        // if (SceneUtils.IsInGameplay())
        // {
        //     if (currentCoroutine == null)
        //         currentCoroutine = StartCoroutine(InitBackground());
        // }
    }

    IEnumerator InitBackground()
    {
        backgroundSource.loop = false;
        backgroundSource.clip = background_Intro_Clip;
        backgroundSource.Play();

        yield return new WaitUntil(() => !backgroundSource.isPlaying);
        
        backgroundSource.clip = backgroundClip;
        backgroundSource.Play();
        backgroundSource.loop = true;
        currentCoroutine = null;
    }

    public void PlayBackgroundSource(bool _end)
    {
        //Make a fade effect
        // if (currentCoroutine == null)
        //     currentCoroutine = StartCoroutine(BackgroundFadeEffect(_end));
            
        AudioClip tempClip = _end ? goodEndClip : badEndClip;

        if (!_end)
            backgroundSource.loop = false;
        backgroundSource.clip = tempClip;
        backgroundSource.Play();
    }

    #region In case of changes

    IEnumerator BackgroundFadeEffect(bool _end)
    {
        float i = 0;
        // float maxValue = _end ? .5f : .4f;
        
        float maxValue = .4f;
        AudioClip tempClip = _end ? goodEndClip : badEndClip;

        background2Source.loop = _end;
        background2Source.clip = tempClip;
        background2Source.Play();
        
        while (i < maxValue)
        {
            backgroundSource.volume = .5f - i;
            i += Time.deltaTime;
            yield return null;
        }

        currentCoroutine = null;
    }

    public void PlayBackgroundSource2(bool _end)
    {
        if (currentCoroutine == null)
            currentCoroutine = StartCoroutine(BackgroundFadeEffect(_end));
    }

    #endregion
    
    
    private void CheckButton_Music()
    {
        if (PlayerPrefs.GetInt("MusicLevel") > .5f)
            musicButton.sprite = musicOn;
        else
            musicButton.sprite = musicOff;

        // musicButton.sprite = SaveDataManager._instance.gameData.musicLevel ? musicOn : musicOff;
    }

    private void CheckButton_SFX()
    {
        if (PlayerPrefs.GetInt("SFXLevel") > .5f)
            sfxButton.sprite = sfxOn;
        else
            sfxButton.sprite = sfxOff;

        // sfxButton.sprite = SaveDataManager._instance.gameData.sfxLevel ? sfxOn : sfxOff;
    }

    public void SetBackgroundMusicVolume()
    {
        isMusicOn = !isMusicOn;
        var _soundLevel = 0.0001f;
        if (isMusicOn)
            _soundLevel = 1;
        mixer.SetFloat("MusicVol", Mathf.Log10(_soundLevel) * 20);

        // SaveDataManager._instance.gameData.musicLevel = isMusicOn;
        // SaveDataManager._instance.SaveData();
        var value = isMusicOn ? 1 : 0;
        PlayerPrefs.SetInt("MusicLevel", value);
        CheckButton_Music();
    }

    public void SetBSFXMusicVolume()
    {
        isSFXOn = !isSFXOn;
        var _soundLevel = 0.0001f;
        if (isSFXOn)
            _soundLevel = 1;
        mixer.SetFloat("SFXVol", Mathf.Log10(_soundLevel) * 20);

        // SaveDataManager._instance.gameData.sfxLevel = isSFXOn;
        // SaveDataManager._instance.SaveData();
        var value = isSFXOn ? 1 : 0;
        PlayerPrefs.SetInt("SFXLevel", value);
        CheckButton_SFX();
    }

    private void CheckSFXValue()
    {
        var _soundLevel = 0.0001f;
        if (isSFXOn)
            _soundLevel = 1;
        mixer.SetFloat("SFXVol", Mathf.Log10(_soundLevel) * 20);
    }

    private void CheckBackgroundValue()
    {
        var _soundLevel = 0.0001f;
        if (isMusicOn)
            _soundLevel = 1;
        mixer.SetFloat("MusicVol", Mathf.Log10(_soundLevel) * 20);
    }


    public void CallButtonClick(int clickSound)
    {
        AudioClip clip = null;
        switch (clickSound)
        {
            case 0:
                clip = click_1_Clip;
                break;
            case 1:
                clip = click_2_Clip;
                break;
        }
        
        sfxSource.PlayOneShot(clip);
    }
    
    public void CallButtonClick()
    {
        AudioClip clip = null;
        int tempRand = Random.Range(0, 2);
        switch (tempRand)
        {
            case 0:
                clip = click_1_Clip;
                break;
            case 1:
                clip = click_2_Clip;
                break;
        }
        
        sfxSource.PlayOneShot(clip);
    }

    public void SuccessSound()
    {
        sfxSource.PlayOneShot(success_Clip);
    }

    public void FailSound()
    {
        sfxSource.PlayOneShot(fail_Clip);
    }
    #region Not Used

    public void SetBackgroundMusicVolume(float _soundLevel)
    {
        print(_soundLevel);
        mixer.SetFloat("MusicVol", Mathf.Log10(_soundLevel) * 20);
        CheckButton_Music();
    }

    public void SetBSFXMusicVolume(float _soundLevel)
    {
        print(_soundLevel);
        mixer.SetFloat("SFXVol", Mathf.Log10(_soundLevel) * 20);
        CheckButton_SFX();
    }

    #endregion
}