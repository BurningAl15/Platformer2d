using System;
using UnityEngine;

/// <summary>
/// Wrapper centralizado para el sistema de anuncios.
/// Aísla el SDK de ads del resto del juego — si cambias de SDK
/// solo modificas este archivo.
///
/// SDK TARGET: Unity LevelPlay (Unity Gaming Services)
/// Documentación: https://developers.is.com/ironsource-mobile/unity/levelplay-starter-kit/
///
/// SETUP REQUERIDO:
/// 1. En Unity: Window → Package Manager → buscar "LevelPlay" e instalar.
/// 2. En el Inspector del GameObject AdsManager: asignar App Keys de Android e iOS
///    (obtenidas desde el dashboard de LevelPlay / ironSource).
/// 3. En Dashboard: configurar Ad Units para Rewarded e Interstitial.
///
/// INTEGRACIÓN RevenueCat NoAds:
///   Si RevenueCatManager.Instance.IsNoAdsPurchased() == true,
///   NINGÚN ad se muestra (rewarded ni interstitial).
///   El método OnNoAdsStatusChanged() actualiza este estado en tiempo real.
///
/// USO:
///   AdsManager.Instance.ShowRewarded(AdsManager.AdContext.GameOver, onSuccess, onFail);
///   AdsManager.Instance.ShowInterstitial(AdsManager.AdContext.ReturnToMap);
///   AdsManager.Instance.IsRewardedReady() → bool
///   AdsManager.Instance.IsNoAdsActive()   → bool
/// </summary>
public class AdsManager : MonoBehaviour
{
    public static AdsManager Instance { get; private set; }

    // ─────────────────────────────────────────────────────────────────────────
    //  Configuración
    // ─────────────────────────────────────────────────────────────────────────

    [Header("App Keys (LevelPlay Dashboard)")]
    [Tooltip("App Key de Android — obtenida en dashboard.is.com")]
    [SerializeField] private string androidAppKey = "YOUR_ANDROID_APP_KEY";
    [Tooltip("App Key de iOS — obtenida en dashboard.is.com")]
    [SerializeField] private string iosAppKey = "YOUR_IOS_APP_KEY";

    [Header("Configuración de sesión")]
    [Tooltip("Segundos desde el inicio de sesión antes de permitir el primer interstitial")]
    [SerializeField] private float interstitialCooldownOnStart = 120f;
    [Tooltip("Segundos mínimos entre interstitials consecutivos")]
    [SerializeField] private float interstitialMinInterval = 60f;

    [Header("Debug")]
    [SerializeField] private bool enableTestMode = true;
    [SerializeField] private bool logAdEvents = true;

    // ─────────────────────────────────────────────────────────────────────────
    //  Contextos de ad (dónde se muestran)
    // ─────────────────────────────────────────────────────────────────────────

    public enum AdContext
    {
        GameOver,           // Rewarded: ganar 1-2 vidas y continuar
        LevelComplete,      // Rewarded: futura funcionalidad (placeholder)
        ReturnToMap         // Interstitial: al volver al mapa desde Game Over
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Estado interno
    // ─────────────────────────────────────────────────────────────────────────

    private bool isInitialized = false;
    private float sessionStartTime;
    private float lastInterstitialTime = -999f;

    // Estado NoAds (sincronizado desde RevenueCatManager)
    private bool _noAdsActive = false;

    // Callbacks activos
    private Action onRewardedSuccess;
    private Action onRewardedFail;
    private AdContext currentContext;

    // ─────────────────────────────────────────────────────────────────────────
    //  Ciclo de vida
    // ─────────────────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        sessionStartTime = Time.realtimeSinceStartup;

        // Leer el estado de NoAds desde el cache local (sincrónico, inmediato).
        // RevenueCatManager actualizará este valor vía OnNoAdsStatusChanged() al iniciar.
        _noAdsActive = RevenueCatManager.Instance != null
            ? RevenueCatManager.Instance.IsNoAdsPurchased()
            : PlayerPrefs.GetInt("IAP_NoAds_Purchased", 0) == 1;

        Initialize();
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Inicialización
    // ─────────────────────────────────────────────────────────────────────────

    private void Initialize()
    {
#if UNITY_LEVELPLAY
        string appKey = GetAppKey();
        if (string.IsNullOrEmpty(appKey) || appKey.StartsWith("YOUR_"))
        {
            Debug.LogWarning("[AdsManager] App Key no configurada. Los ads no cargarán. " +
                             "Asignar la key en el Inspector del AdsManager.");
            return;
        }

        IronSource.Agent.validateIntegration();

        if (enableTestMode)
            IronSource.Agent.setMetaData("is_test_suite", "enable");

        IronSource.Agent.init(appKey, IronSourceAdUnits.REWARDED_VIDEO, IronSourceAdUnits.INTERSTITIAL);

        // Suscribir eventos de Rewarded
        IronSourceRewardedVideoEvents.onAdOpenedEvent      += OnRewardedOpened;
        IronSourceRewardedVideoEvents.onAdClosedEvent      += OnRewardedClosed;
        IronSourceRewardedVideoEvents.onAdRewardedEvent    += OnRewardedEarned;
        IronSourceRewardedVideoEvents.onAdShowFailedEvent  += OnRewardedFailed;

        // Suscribir eventos de Interstitial
        IronSourceInterstitialEvents.onAdReadyEvent        += OnInterstitialReady;
        IronSourceInterstitialEvents.onAdOpenedEvent       += OnInterstitialOpened;
        IronSourceInterstitialEvents.onAdClosedEvent       += OnInterstitialClosed;
        IronSourceInterstitialEvents.onAdShowFailedEvent   += OnInterstitialFailed;

        isInitialized = true;
        LogAd("SDK inicializado correctamente.");

        // Precargar interstitial
        LoadInterstitial();
#else
        LogAd("LevelPlay SDK no instalado. Modo simulación activo.");
        isInitialized = true; // En modo simulación, siempre "listo"
#endif
    }

    private string GetAppKey()
    {
#if UNITY_ANDROID
        return androidAppKey;
#elif UNITY_IOS
        return iosAppKey;
#else
        return androidAppKey; // Editor/PC: usar Android key para pruebas
#endif
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  API PÚBLICA — Rewarded
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Muestra un rewarded ad.
    /// onSuccess: se llama si el jugador vio el ad completo y ganó la recompensa.
    /// onFail: se llama si el ad no estaba disponible o el jugador lo cerró antes.
    /// </summary>
    public void ShowRewarded(AdContext context, Action onSuccess, Action onFail = null)
    {
        // Si el jugador tiene NoAds, dar la recompensa directamente sin ad.
        if (IsNoAdsActive())
        {
            LogAd($"NoAds activo — recompensa otorgada directamente. Contexto: {context}");
            onSuccess?.Invoke();
            return;
        }

        currentContext = context;
        onRewardedSuccess = onSuccess;
        onRewardedFail = onFail;

#if UNITY_LEVELPLAY
        if (IronSource.Agent.isRewardedVideoAvailable())
        {
            IronSource.Agent.showRewardedVideo();
            LogAd($"Rewarded mostrado. Contexto: {context}");
        }
        else
        {
            LogAd($"Rewarded no disponible. Contexto: {context}. Ejecutando fallback.");
            onFail?.Invoke();
        }
#else
        // Modo simulación — simula un ad exitoso directamente
        LogAd($"[SIMULACIÓN] Rewarded ad exitoso. Contexto: {context}");
        SimulateRewardedAd(context, onSuccess, onFail);
#endif
    }

    /// <summary>
    /// Retorna true si hay un rewarded listo para mostrar.
    /// </summary>
    public bool IsRewardedReady()
    {
#if UNITY_LEVELPLAY
        return isInitialized && IronSource.Agent.isRewardedVideoAvailable();
#else
        return true; // Modo simulación: siempre disponible
#endif
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  API PÚBLICA — Interstitial
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Muestra un interstitial si las condiciones se cumplen:
    /// - El jugador NO tiene NoAds activo.
    /// - Han pasado más de interstitialCooldownOnStart segundos desde que inició la sesión.
    /// - Han pasado más de interstitialMinInterval desde el último interstitial.
    /// </summary>
    public void ShowInterstitial(AdContext context)
    {
        // Bloquear si tiene NoAds
        if (IsNoAdsActive())
        {
            LogAd($"Interstitial bloqueado — NoAds activo. Contexto: {context}");
            return;
        }

        float now = Time.realtimeSinceStartup;

        if (now - sessionStartTime < interstitialCooldownOnStart)
        {
            LogAd($"Interstitial bloqueado — cooldown de inicio ({interstitialCooldownOnStart}s). Contexto: {context}");
            return;
        }

        if (now - lastInterstitialTime < interstitialMinInterval)
        {
            LogAd($"Interstitial bloqueado — muy pronto después del anterior. Contexto: {context}");
            return;
        }

#if UNITY_LEVELPLAY
        if (IronSource.Agent.isInterstitialReady())
        {
            IronSource.Agent.showInterstitial();
            lastInterstitialTime = now;
            LogAd($"Interstitial mostrado. Contexto: {context}");
        }
        else
        {
            LogAd($"Interstitial no listo. Contexto: {context}. Recargando...");
            LoadInterstitial();
        }
#else
        LogAd($"[SIMULACIÓN] Interstitial. Contexto: {context}");
        lastInterstitialTime = now;
#endif
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Carga de ads
    // ─────────────────────────────────────────────────────────────────────────

    private void LoadInterstitial()
    {
#if UNITY_LEVELPLAY
        IronSource.Agent.loadInterstitial();
#endif
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Eventos del SDK — Rewarded
    // ─────────────────────────────────────────────────────────────────────────

#if UNITY_LEVELPLAY
    private void OnRewardedOpened(IronSourceAdInfo info)
    {
        LogAd("Rewarded abierto.");
        Time.timeScale = 0f; // Pausar el juego mientras corre el ad
    }

    private void OnRewardedEarned(IronSourcePlacement placement, IronSourceAdInfo info)
    {
        LogAd($"Rewarded completado. Recompensa: {placement.getRewardName()} x{placement.getRewardAmount()}");
        Time.timeScale = 1f;
        onRewardedSuccess?.Invoke();
        onRewardedSuccess = null;
        onRewardedFail = null;
    }

    private void OnRewardedClosed(IronSourceAdInfo info)
    {
        LogAd("Rewarded cerrado (sin completar).");
        Time.timeScale = 1f;
        // Si no se llamó OnRewardedEarned, el jugador cerró antes
        if (onRewardedFail != null)
        {
            onRewardedFail?.Invoke();
            onRewardedSuccess = null;
            onRewardedFail = null;
        }
    }

    private void OnRewardedFailed(IronSourceError error, IronSourceAdInfo info)
    {
        LogAd($"Rewarded falló: {error.getDescription()}");
        Time.timeScale = 1f;
        onRewardedFail?.Invoke();
        onRewardedSuccess = null;
        onRewardedFail = null;
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Eventos del SDK — Interstitial
    // ─────────────────────────────────────────────────────────────────────────

    private void OnInterstitialReady(IronSourceAdInfo info)
        => LogAd("Interstitial listo.");

    private void OnInterstitialOpened(IronSourceAdInfo info)
        => Time.timeScale = 0f;

    private void OnInterstitialClosed(IronSourceAdInfo info)
    {
        Time.timeScale = 1f;
        LoadInterstitial(); // Precargar el siguiente
    }

    private void OnInterstitialFailed(IronSourceError error, IronSourceAdInfo info)
    {
        LogAd($"Interstitial falló: {error.getDescription()}");
        Time.timeScale = 1f;
    }

    private void OnApplicationPause(bool isPaused)
    {
        IronSource.Agent.onApplicationPause(isPaused);
    }
#endif

    // ─────────────────────────────────────────────────────────────────────────
    //  API PÚBLICA — NoAds (integración con RevenueCat)
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Retorna true si el jugador tiene NoAds activo.
    /// Los ads quedan completamente desactivados cuando esto es true.
    /// </summary>
    public bool IsNoAdsActive() => _noAdsActive;

    /// <summary>
    /// Llamado por RevenueCatManager cuando el estado de NoAds cambia
    /// (compra exitosa o restauración). Actualiza el estado en tiempo real.
    /// </summary>
    public void OnNoAdsStatusChanged(bool isPurchased)
    {
        _noAdsActive = isPurchased;
        LogAd($"Estado NoAds actualizado: {(isPurchased ? "ACTIVO — ads desactivados" : "INACTIVO — ads activos")}");
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Simulación (sin SDK instalado)
    // ─────────────────────────────────────────────────────────────────────────

    private void SimulateRewardedAd(AdContext context, Action onSuccess, Action onFail)
    {
        // En simulación siempre tiene éxito — para pruebas reales del flujo
        // sin depender del SDK instalado.
        StartCoroutine(SimulateAdDelay(onSuccess));
    }

    private System.Collections.IEnumerator SimulateAdDelay(Action onSuccess)
    {
        yield return new UnityEngine.WaitForSecondsRealtime(0.5f);
        onSuccess?.Invoke();
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Helpers
    // ─────────────────────────────────────────────────────────────────────────

    private void LogAd(string message)
    {
        if (logAdEvents)
            Debug.Log($"<color=yellow>[AdsManager]</color> {message}");
    }
}
