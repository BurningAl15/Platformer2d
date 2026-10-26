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
public class AppBootstrap : MonoBehaviour
{
    public static AppBootstrap Instance { get; private set; }

    [Header("DOTween")]
    [SerializeField] private bool dotweenRecycleObjects = true;
    [SerializeField] private bool dotweenUseSafeMode = true;

    [Header("Framerate")]
    [SerializeField] private int targetFrameRate = 60;

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
