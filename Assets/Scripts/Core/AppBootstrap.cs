using DG.Tweening;
using UnityEngine;

/// <summary>
/// Script de arranque de la aplicación. Se ejecuta UNA sola vez gracias a
/// DontDestroyOnLoad y a la protección de Singleton.
///
/// Centraliza:
/// - Inicialización de DOTween.
/// - Configuración global de framerate.
/// - Requiere GamepadDetector y SaveManager en el mismo GameObject.
///
/// Agregar este GameObject a la primera escena del juego (MainMenu).
/// </summary>
[RequireComponent(typeof(GamepadDetector))]
[RequireComponent(typeof(SaveManager))]
[RequireComponent(typeof(LivesManager))]
[RequireComponent(typeof(AdsManager))]
[RequireComponent(typeof(RevenueCatManager))]
[RequireComponent(typeof(WorldProgressManager))]
public class AppBootstrap : MonoBehaviour
{
    public static AppBootstrap Instance { get; private set; }

    [Header("DOTween")]
    [SerializeField] private bool dotweenRecycleObjects = true;
    [SerializeField] private bool dotweenUseSafeMode = true;

    [Header("Framerate")]
    [SerializeField] private int targetFrameRate = 60;

    private bool isApplicationPaused;
    private bool isApplicationFocused = true;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeDOTween();
        ConfigureFramerate();
        ConfigureLandscapeOrientation();
        ApplyScreenAwakePreference();
    }

    private void OnApplicationPause(bool paused)
    {
        isApplicationPaused = paused;
        ApplyScreenAwakePreference();
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        isApplicationFocused = hasFocus;
        ApplyScreenAwakePreference();
    }

    private void OnApplicationQuit()
    {
        Screen.sleepTimeout = SleepTimeout.SystemSetting;
    }

    private void OnDestroy()
    {
        if (Instance != this) return;
        Screen.sleepTimeout = SleepTimeout.SystemSetting;
        Instance = null;
    }

    public static void TriggerHapticFeedback()
    {
        if (PlayerPrefs.GetInt("HapticsEnabled", 1) != 1 || !Application.isMobilePlatform)
            return;

#if UNITY_ANDROID || UNITY_IOS
        Handheld.Vibrate();
#endif
    }

    public static void ApplyScreenAwakePreference()
    {
        bool shouldKeepAwake = PlayerPrefs.GetInt("KeepScreenAwake", 0) == 1;
        bool appIsActive = Instance == null || (!Instance.isApplicationPaused && Instance.isApplicationFocused);
        Screen.sleepTimeout = shouldKeepAwake && appIsActive
            ? SleepTimeout.NeverSleep
            : SleepTimeout.SystemSetting;
    }

    private static void ConfigureLandscapeOrientation()
    {
#if UNITY_ANDROID || UNITY_IOS
        Screen.autorotateToPortrait = false;
        Screen.autorotateToPortraitUpsideDown = false;
        Screen.autorotateToLandscapeLeft = true;
        Screen.autorotateToLandscapeRight = true;
        Screen.orientation = ScreenOrientation.AutoRotation;
#endif
    }

    private void InitializeDOTween()
    {
        DOTween.Init(dotweenRecycleObjects, dotweenUseSafeMode, LogBehaviour.Default);
        Debug.Log("[AppBootstrap] DOTween inicializado.");
    }

    private void ConfigureFramerate()
    {
#if UNITY_STANDALONE_WIN || UNITY_STANDALONE
        QualitySettings.vSyncCount = 1;
#else
        QualitySettings.vSyncCount = 0;
#endif
        Application.targetFrameRate = targetFrameRate;
        Debug.Log($"[AppBootstrap] Target framerate: {targetFrameRate}");
    }
}
