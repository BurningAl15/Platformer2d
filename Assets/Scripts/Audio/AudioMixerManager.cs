using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using Random = UnityEngine.Random;

// ─────────────────────────────────────────────────────────────────────────────
//  Tipos de SFX disponibles en el juego
// ─────────────────────────────────────────────────────────────────────────────
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
    Warp_Jingle,
    Smash_Impact
}

// ─────────────────────────────────────────────────────────────────────────────
//  Elemento de SFX: clip + source + helpers de reproducción
// ─────────────────────────────────────────────────────────────────────────────
[Serializable]
public class SFXElement
{
    public AudioClip clip;
    public AudioSource source;

    public void CallSFX()
    {
        if (source == null || clip == null) return;
        source.Stop();
        source.clip = clip;
        source.Play();
    }

    public void SetPitch(float pitch)
    {
        if (source != null)
            source.pitch = pitch;
    }
}

// ─────────────────────────────────────────────────────────────────────────────
//  AudioMixerManager
// ─────────────────────────────────────────────────────────────────────────────
/// <summary>
/// Manager central de audio. Gestiona:
/// - Reproducción de SFX por tipo (SFXType).
/// - Música de fondo con crossfade entre pistas.
/// - Pitch creciente para el combo de gemas.
/// - Configuración de volumen (música / SFX) persistida en SaveManager.
/// </summary>
public class AudioMixerManager : MonoBehaviour
{
    public static AudioMixerManager _instance;

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer mixer;

    // ─────────────────────────────
    //  Música de fondo
    // ─────────────────────────────
    [Header("Pistas de música")]
    [SerializeField] private AudioClip background_MainLevel_Clip;
    [SerializeField] private AudioClip background_BossBattle_Clip;
    [SerializeField] private AudioClip background_GameComplete_Clip;
    [SerializeField] private AudioClip background_LevelVictory_Clip;
    [SerializeField] private AudioClip background_LevelSelect_Clip;
    [SerializeField] private AudioClip background_TitleScreen_Clip;

    [Header("Fuentes de audio de música")]
    [SerializeField] private AudioSource backgroundSource;    // pista principal
    [SerializeField] private AudioSource background2Source;   // victoria / fin de juego
    [SerializeField] private AudioSource background3Source;   // boss

    // ─────────────────────────────
    //  SFX — Jugador
    // ─────────────────────────────
    [Header("SFX — Jugador")]
    [SerializeField] private SFXElement jump_Clip;
    [SerializeField] private SFXElement hurt_Clip;
    [SerializeField] private SFXElement deathEffect_Player_Clip;
    [SerializeField] private SFXElement pickup_health_clip;
    [SerializeField] private SFXElement pickup_gem_clip;

    // ─────────────────────────────
    //  SFX — Enemigos / Boss
    // ─────────────────────────────
    [Header("SFX — Enemigos")]
    [SerializeField] private SFXElement deathEffect_Enemy_Clip;

    [Header("SFX — Boss")]
    [SerializeField] private SFXElement bossHit_Clip;
    [SerializeField] private SFXElement bossImpact_Clip;
    [SerializeField] private SFXElement bossShoot_Clip;

    // ─────────────────────────────
    //  SFX — Mundo / UI
    // ─────────────────────────────
    [Header("SFX — Mundo / UI")]
    [SerializeField] private SFXElement levelSelected_Clip;
    [SerializeField] private SFXElement mapMovement_Clip;
    [SerializeField] private SFXElement warpJingle_Clip;
    [SerializeField] private SFXElement smashImpact_Clip;

    // ─────────────────────────────
    //  UI de botones de volumen
    // ─────────────────────────────
    [Header("Botones de configuración de audio")]
    [SerializeField] private Image musicButton;
    [SerializeField] private Image sfxButton;
    [SerializeField] private Sprite musicOn;
    [SerializeField] private Sprite musicOff;
    [SerializeField] private Sprite sfxOn;
    [SerializeField] private Sprite sfxOff;

    // ─────────────────────────────
    //  Estado interno
    // ─────────────────────────────
    private bool isMusicOn = true;
    private bool isSFXOn   = true;

    // Pitch de combo de gemas
    private float gemPitch    = 0.5f;
    private const float GemPitchStart = 0.5f;
    private const float GemPitchStep  = 0.05f;
    private const float GemPitchMax   = 1.5f;

    private Coroutine currentCoroutine = null;

    // ─────────────────────────────────────────────────────────────────────────
    //  Ciclo de vida
    // ─────────────────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
    }

    private void Start()
    {
        // Cargar configuración de audio desde SaveManager
        if (SaveManager.Instance != null)
        {
            isMusicOn = SaveManager.Instance.LoadMusicEnabled();
            isSFXOn   = SaveManager.Instance.LoadSFXEnabled();
        }

        ApplyMusicVolume();
        ApplySFXVolume();
        RefreshAudioButtons();

        if (currentCoroutine == null)
            currentCoroutine = StartCoroutine(InitBackgroundMusic());
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  SFX
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Reproduce un SFX por tipo.</summary>
    public void CallSFX(SFXType sfxType)
    {
        switch (sfxType)
        {
            case SFXType.Player_Jump:
                jump_Clip.CallSFX();
                jump_Clip.SetPitch(Random.Range(0.8f, 1.2f));
                break;
            case SFXType.Player_Hurt:
                hurt_Clip.CallSFX();
                hurt_Clip.SetPitch(Random.Range(0.8f, 1.2f));
                break;
            case SFXType.Player_Death:
                deathEffect_Player_Clip.CallSFX();
                deathEffect_Player_Clip.SetPitch(Random.Range(0.8f, 1.2f));
                break;
            case SFXType.Player_Pickup_Health:
                pickup_health_clip.CallSFX();
                pickup_health_clip.SetPitch(1f);
                break;
            case SFXType.Enemy_Death:
                deathEffect_Enemy_Clip.CallSFX();
                deathEffect_Enemy_Clip.SetPitch(Random.Range(0.8f, 1.2f));
                break;
            case SFXType.Boss_Hit:
                bossHit_Clip.CallSFX();
                bossHit_Clip.SetPitch(1f);
                break;
            case SFXType.Boss_Impact:
                bossImpact_Clip.CallSFX();
                bossImpact_Clip.SetPitch(1f);
                break;
            case SFXType.Boss_Shoot:
                bossShoot_Clip.CallSFX();
                bossShoot_Clip.SetPitch(1f);
                break;
            case SFXType.Level_Selected:
                levelSelected_Clip.CallSFX();
                levelSelected_Clip.SetPitch(1f);
                break;
            case SFXType.Map_Movement:
                mapMovement_Clip.CallSFX();
                mapMovement_Clip.SetPitch(1f);
                break;
            case SFXType.Warp_Jingle:
                warpJingle_Clip.CallSFX();
                warpJingle_Clip.SetPitch(1f);
                break;
            case SFXType.Smash_Impact:
                smashImpact_Clip.CallSFX();
                smashImpact_Clip.SetPitch(1f);
                break;
        }
    }

    /// <summary>Reproduce el SFX de gema con pitch creciente (combo).</summary>
    public void CallSFX_Gems()
    {
        pickup_gem_clip.SetPitch(gemPitch);
        pickup_gem_clip.CallSFX();

        gemPitch = Mathf.Min(gemPitch + GemPitchStep, GemPitchMax);
        ComboManager._instance?.RunTimer();
    }

    /// <summary>Reinicia el pitch del combo de gemas al valor inicial.</summary>
    public void ResetPitch()
    {
        gemPitch = GemPitchStart;
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Música de fondo
    // ─────────────────────────────────────────────────────────────────────────

    private IEnumerator InitBackgroundMusic()
    {
        yield return null; // esperar un frame para que la escena termine de cargar

        if (SceneUtils.IsInGameplay())
            backgroundSource.clip = background_MainLevel_Clip;
        else if (SceneUtils.IsInTitleScreen())
            backgroundSource.clip = background_TitleScreen_Clip;
        else
            backgroundSource.clip = background_LevelSelect_Clip;

        backgroundSource.loop = true;
        backgroundSource.Play();
        currentCoroutine = null;
    }

    /// <summary>Cambia a la música de victoria o fin de juego con fade.</summary>
    public void PlayBackgroundSource(bool isGameComplete)
    {
        if (currentCoroutine == null)
            currentCoroutine = StartCoroutine(FadeToVictoryMusic(isGameComplete));
    }

    private IEnumerator FadeToVictoryMusic(bool isGameComplete)
    {
        AudioClip clip = isGameComplete ? background_GameComplete_Clip : background_LevelVictory_Clip;
        background2Source.loop = isGameComplete;
        background2Source.clip = clip;
        background2Source.Play();

        float t = 0f;
        while (t < 0.4f)
        {
            backgroundSource.volume = 0.5f - t;
            t += Time.deltaTime;
            yield return null;
        }
        currentCoroutine = null;
    }

    /// <summary>Transiciona a la música del boss con fade.</summary>
    public void PlayBossBackground()
    {
        if (currentCoroutine == null)
            currentCoroutine = StartCoroutine(FadeToBossMusic());
    }

    private IEnumerator FadeToBossMusic()
    {
        background3Source.loop = true;
        background3Source.clip = background_BossBattle_Clip;
        background3Source.Play();

        float t = 0f;
        while (t < 0.5f)
        {
            backgroundSource.volume = 0.5f - t;
            t += Time.deltaTime;
            yield return null;
        }
        currentCoroutine = null;
    }

    /// <summary>Regresa a la música del nivel principal con fade.</summary>
    public void StopBossBackground()
    {
        if (currentCoroutine == null)
            currentCoroutine = StartCoroutine(FadeFromBossMusic());
    }

    private IEnumerator FadeFromBossMusic()
    {
        backgroundSource.loop = true;
        backgroundSource.clip = background_MainLevel_Clip;
        backgroundSource.Play();

        float t = 0f;
        while (t < 0.5f)
        {
            background3Source.volume = 0.5f - t;
            backgroundSource.volume  = t * 2f;
            t += Time.deltaTime;
            yield return null;
        }
        currentCoroutine = null;
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Configuración de volumen (toggling)
    // ─────────────────────────────────────────────────────────────────────────

    public void SetBackgroundMusicVolume()
    {
        isMusicOn = !isMusicOn;
        ApplyMusicVolume();
        SaveManager.Instance?.SaveMusicEnabled(isMusicOn);
        RefreshAudioButtons();
    }

    public void SetBSFXMusicVolume()
    {
        isSFXOn = !isSFXOn;
        ApplySFXVolume();
        SaveManager.Instance?.SaveSFXEnabled(isSFXOn);
        RefreshAudioButtons();
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Helpers privados
    // ─────────────────────────────────────────────────────────────────────────

    private void ApplyMusicVolume()
    {
        float level = isMusicOn ? 1f : 0.0001f;
        mixer.SetFloat("MusicVol", Mathf.Log10(level) * 20f);
    }

    private void ApplySFXVolume()
    {
        float level = isSFXOn ? 1f : 0.0001f;
        mixer.SetFloat("SFXVol", Mathf.Log10(level) * 20f);
    }

    private void RefreshAudioButtons()
    {
        if (musicButton != null)
            musicButton.sprite = isMusicOn ? musicOn : musicOff;
        if (sfxButton != null)
            sfxButton.sprite = isSFXOn ? sfxOn : sfxOff;
    }
}
