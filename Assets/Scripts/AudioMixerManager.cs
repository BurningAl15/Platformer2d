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

[Serializable]
public class SFXElement
{
    public AudioClip clip;
    public AudioSource source;

    public void CallSFX()
    {
        source.Stop();
        source.clip = clip;
        source.Play();
    }

    public void SetPitch(float pitch)
    {
        source.pitch = pitch;
    }
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
    [FormerlySerializedAs("background_Intro_Clip")]  [SerializeField] private AudioClip background_MainLevel_Clip;
    [FormerlySerializedAs("backgroundClip")] [SerializeField] private AudioClip background_BossBattle_Clip;
    [FormerlySerializedAs("goodEndClip")] [SerializeField] private AudioClip background_GameComplete_Clip;
    [FormerlySerializedAs("badEndClip")] [SerializeField] private AudioClip background_LevelVictory_Clip;
    [SerializeField] private AudioClip background_LevelSelect_Clip;
    [SerializeField] private AudioClip background_TitleScreen_Clip;
    [SerializeField] private AudioSource backgroundSource;
    [SerializeField] private AudioSource background2Source;

    [Header("SFX")]
    // [SerializeField] private AudioClip click_1_Clip ;
    // [SerializeField] private AudioClip click_2_Clip;
    [SerializeField] private AudioClip success_Clip;
    [SerializeField] private AudioClip fail_Clip;

    [SerializeField] private SFXElement levelSelected_Clip;
    [SerializeField] private SFXElement mapMovement_Clip;
    [SerializeField] private SFXElement warpJingle_Clip;

    [Header("Boss SFX")] 
    [SerializeField] private SFXElement bossHit_Clip;
    [SerializeField] private SFXElement bossImpact_Clip;
    [SerializeField] private SFXElement bossShoot_Clip;
    
    [Header("Death Effect SFX")]
    [SerializeField] private SFXElement deathEffect_Enemy_Clip;
    
    [Header("Player SFX")]
    [SerializeField] private SFXElement deathEffect_Player_Clip;
    [SerializeField] private SFXElement pickup_gem_clip;
    [SerializeField] private SFXElement pickup_helth_clip;
    [SerializeField] private SFXElement jump_Clip;
    [SerializeField] private SFXElement hurt_Clip;
    

    [Header("Pitch Growing Variables")]
    private float initialPitch=.5f;
    private float growPitchRate = .05f;
    private float maxPitch = 1.5f;

    private Coroutine currentCoroutine = null;
    
    public void CallSFX(SFXType sfxType)
    {
        AudioClip clip = null;
        switch (sfxType)
        {
            case SFXType.Player_Pickup_Health:
                pickup_helth_clip.CallSFX();
                pickup_helth_clip.SetPitch(1);
                break;
            case SFXType.Player_Jump:
                jump_Clip.CallSFX();
                jump_Clip.SetPitch(Random.Range(.8f, 1.2f));
                break;          
            case SFXType.Player_Hurt:
                hurt_Clip.CallSFX();
                hurt_Clip.SetPitch(Random.Range(.8f, 1.2f));
                break;
            case SFXType.Player_Death:
                deathEffect_Player_Clip.CallSFX();
                deathEffect_Player_Clip.SetPitch(Random.Range(.8f, 1.2f));
                break;
            case SFXType.Enemy_Death:
                deathEffect_Enemy_Clip.CallSFX();
                deathEffect_Enemy_Clip.SetPitch(Random.Range(.8f, 1.2f));
                break;
            case SFXType.Boss_Hit:
                bossHit_Clip.CallSFX();
                bossHit_Clip.SetPitch(1);
                break;
            case SFXType.Boss_Impact:
                bossImpact_Clip.CallSFX();
                bossImpact_Clip.SetPitch(1);
                break;
            case SFXType.Boss_Shoot:
                bossShoot_Clip.CallSFX();
                bossShoot_Clip.SetPitch(1);
                break;
            case SFXType.Level_Selected:
                levelSelected_Clip.CallSFX();
                levelSelected_Clip.SetPitch(1);
                break;
            case SFXType.Map_Movement:
                mapMovement_Clip.CallSFX();
                mapMovement_Clip.SetPitch(1);
                break;
            case SFXType.Warp_Jingle:
                warpJingle_Clip.CallSFX();
                warpJingle_Clip.SetPitch(1);
                break;
        }
    }

    public void CallSFX_Gems()
    {
        pickup_gem_clip.SetPitch(initialPitch);
        pickup_gem_clip.CallSFX();

        initialPitch += growPitchRate;
        
        if (initialPitch >= maxPitch)
            initialPitch = maxPitch;
    }

    public void ResetPitch()
    {
        initialPitch = .5f;
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
        if (currentCoroutine == null)
            currentCoroutine = StartCoroutine(InitBackground());
        // }
    }

    IEnumerator InitBackground()
    {
        // backgroundSource.loop = false;
        // backgroundSource.clip = background_MainLevel_Clip;
        // backgroundSource.Play();
        //
        // yield return new WaitUntil(() => !backgroundSource.isPlaying);
        yield return null;
        backgroundSource.clip = background_MainLevel_Clip;
        backgroundSource.Play();
        backgroundSource.loop = true;
        currentCoroutine = null;
    }

    public void PlayBackgroundSource(bool _end)
    {
        //Make a fade effect
        // if (currentCoroutine == null)
        //     currentCoroutine = StartCoroutine(BackgroundFadeEffect(_end));
            
        AudioClip tempClip = _end ? background_GameComplete_Clip : background_LevelVictory_Clip;

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
        AudioClip tempClip = _end ? background_GameComplete_Clip : background_LevelVictory_Clip;

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


    // public void CallButtonClick(int clickSound)
    // {
    //     AudioClip clip = null;
    //     switch (clickSound)
    //     {
    //         case 0:
    //             clip = click_1_Clip;
    //             break;
    //         case 1:
    //             clip = click_2_Clip;
    //             break;
    //     }
    //     
    //     sfxSource.PlayOneShot(clip);
    // }
    
    // public void CallButtonClick()
    // {
    //     AudioClip clip = null;
    //     int tempRand = Random.Range(0, 2);
    //     switch (tempRand)
    //     {
    //         case 0:
    //             clip = click_1_Clip;
    //             break;
    //         case 1:
    //             clip = click_2_Clip;
    //             break;
    //     }
    //     
    //     sfxSource.PlayOneShot(clip);
    // }

    // public void SuccessSound()
    // {
    //     sfxSource.PlayOneShot(success_Clip);
    // }
    //
    // public void FailSound()
    // {
    //     sfxSource.PlayOneShot(fail_Clip);
    // }
    
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