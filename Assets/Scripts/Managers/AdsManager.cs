using System;
using UnityEngine;
#if LEVELPLAY_DEPENDENCIES_INSTALLED
using Unity.Services.LevelPlay;
#endif

/// <summary>
/// Wrapper centralizado para el sistema de anuncios.
/// Aísla el SDK de ads del resto del juego — si cambias de SDK
/// solo modificas este archivo.
///
/// Wrapper para la API de ad units de LevelPlay 9.x. Los anuncios permanecen cerrados
/// hasta resolver elegibilidad/consentimiento y entitlement Remove Ads.
///
/// INTEGRACIÓN RevenueCat NoAds:
///   Si RevenueCatManager.Instance.IsNoAdsPurchased() == true,
///   ningún ad se muestra, incluido rewarded (según el GDD).
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

    [Header("App Keys (LevelPlay Dashboard — configurar localmente)")]
    [Tooltip("App Key de Android — obtenida en dashboard.is.com")]
    [SerializeField] private string androidAppKey = "290dc33f5";
    [Tooltip("App Key de iOS — obtenida en dashboard.is.com")]
    [SerializeField] private string iosAppKey = "290e4f7ad";

    [Header("Ad Unit IDs (LevelPlay Dashboard)")]
    [SerializeField] private string androidRewardedAdUnitId = "d9opyhliijhr3fb4";
    [SerializeField] private string androidInterstitialAdUnitId = "7xsq048b2uuvo8vj";
    [SerializeField] private string iosRewardedAdUnitId = "4sm02ryaqjxmk9pd";
    [SerializeField] private string iosInterstitialAdUnitId = "6wzvpnyl3oqj65qm";

    [Header("Configuración de sesión")]
    [Tooltip("Segundos desde el inicio de sesión antes de permitir el primer interstitial")]
    [SerializeField] private float interstitialCooldownOnStart = 120f;
    [Tooltip("Segundos mínimos entre interstitials consecutivos")]
    [SerializeField] private float interstitialMinInterval = 60f;

    [Header("Debug")]
    [SerializeField] private bool launchLevelPlayTestSuite = true;
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
    private bool privacyEligibilityResolved;
    private bool mayServeAds;
    private bool initializationInProgress;
    private bool rewardedRequestPending;
    private bool rewardedCompleted;
    private float timeScaleBeforeAd = 1f;
    private Coroutine rewardCloseTimeout;

#if LEVELPLAY_DEPENDENCIES_INSTALLED
    private LevelPlayRewardedAd rewardedAd;
    private LevelPlayInterstitialAd interstitialAd;
#endif

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

        if (RevenueCatManager.Instance != null)
            RevenueCatManager.Instance.EntitlementStatusResolved += OnEntitlementStatusResolved;

        SetPrivacyEligibilityResolved(true);
    }

    private void OnDestroy()
    {
#if LEVELPLAY_DEPENDENCIES_INSTALLED
        LevelPlay.OnInitSuccess -= OnLevelPlayInitialized;
        LevelPlay.OnInitFailed -= OnLevelPlayInitializationFailed;
        if (rewardedAd != null)
        {
            rewardedAd.OnAdLoaded -= OnRewardedLoaded;
            rewardedAd.OnAdLoadFailed -= OnRewardedLoadFailed;
            rewardedAd.OnAdDisplayed -= OnRewardedDisplayed;
            rewardedAd.OnAdDisplayFailed -= OnRewardedDisplayFailed;
            rewardedAd.OnAdRewarded -= OnRewardedEarned;
            rewardedAd.OnAdClosed -= OnRewardedClosed;
        }
        if (interstitialAd != null)
        {
            interstitialAd.OnAdLoaded -= OnInterstitialReady;
            interstitialAd.OnAdLoadFailed -= OnInterstitialLoadFailed;
            interstitialAd.OnAdDisplayed -= OnInterstitialOpened;
            interstitialAd.OnAdDisplayFailed -= OnInterstitialFailed;
            interstitialAd.OnAdClosed -= OnInterstitialClosed;
        }
#endif
        if (RevenueCatManager.Instance != null)
            RevenueCatManager.Instance.EntitlementStatusResolved -= OnEntitlementStatusResolved;
        if (Instance == this)
            Instance = null;
    }

    /// <summary>
    /// Debe llamarlo la capa de audiencia/consentimiento después de resolver las señales
    /// pertinentes y configurar el SDK/CMP. Sin caller, los anuncios quedan bloqueados.
    /// </summary>
    public void SetPrivacyEligibilityResolved(bool eligible)
    {
        privacyEligibilityResolved = true;
        mayServeAds = eligible;
        if (eligible)
            TryInitializeWhenEligible();
    }

    private void OnEntitlementStatusResolved(bool isPurchased)
    {
        _noAdsActive = isPurchased;
        if (!isPurchased)
            TryInitializeWhenEligible();
    }

    private void TryInitializeWhenEligible()
    {
        if (!privacyEligibilityResolved || !mayServeAds)
            return;

        RevenueCatManager manager = RevenueCatManager.Instance;
        if (manager != null && manager.IsEntitlementStatusResolved)
        {
            _noAdsActive = manager.IsNoAdsPurchased();
        }
        else
        {
            _noAdsActive = PlayerPrefs.GetInt("IAP_NoAds_Purchased", 0) == 1;
        }

        if (!_noAdsActive)
            Initialize();
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Inicialización
    // ─────────────────────────────────────────────────────────────────────────

    private void Initialize()
    {
        if (isInitialized || initializationInProgress || !privacyEligibilityResolved || !mayServeAds || IsNoAdsActive())
            return;

#if LEVELPLAY_DEPENDENCIES_INSTALLED
        string appKey = GetAppKey();
        if (string.IsNullOrEmpty(appKey) || appKey.StartsWith("YOUR_") ||
            string.IsNullOrEmpty(GetRewardedAdUnitId()) || string.IsNullOrEmpty(GetInterstitialAdUnitId()))
        {
            Debug.LogWarning("[AdsManager] App Key o Ad Unit ID no configurado para esta plataforma.");
            return;
        }

        if (launchLevelPlayTestSuite && (Application.isEditor || Debug.isDebugBuild))
            LevelPlay.SetMetaData("is_test_suite", "enable");

        LevelPlay.OnInitSuccess -= OnLevelPlayInitialized;
        LevelPlay.OnInitFailed -= OnLevelPlayInitializationFailed;
        LevelPlay.OnInitSuccess += OnLevelPlayInitialized;
        LevelPlay.OnInitFailed += OnLevelPlayInitializationFailed;
        initializationInProgress = true;
        LevelPlay.Init(appKey);
#else
        LogAd("LevelPlay SDK no instalado; no se simularán anuncios ni recompensas.");
#endif
    }

    private string GetAppKey()
    {
#if UNITY_ANDROID
        return androidAppKey;
#elif UNITY_IOS
        return iosAppKey;
#else
        return androidAppKey; // Editor: configuración Android solo para el test suite
#endif
    }

    private string GetRewardedAdUnitId()
    {
#if UNITY_IOS
        return iosRewardedAdUnitId;
#else
        return androidRewardedAdUnitId;
#endif
    }

    private string GetInterstitialAdUnitId()
    {
#if UNITY_IOS
        return iosInterstitialAdUnitId;
#else
        return androidInterstitialAdUnitId;
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
        if (!privacyEligibilityResolved || !mayServeAds || !isInitialized)
        {
            LogAd("Rewarded bloqueado: privacidad/audiencia o entitlement aún no habilitados.");
            onFail?.Invoke();
            return;
        }

        // Remove Ads elimina todos los anuncios; no convierte el botón rewarded en vidas gratis.
        if (IsNoAdsActive())
        {
            LogAd($"Rewarded bloqueado — Remove Ads activo. Contexto: {context}");
            onFail?.Invoke();
            return;
        }

#if LEVELPLAY_DEPENDENCIES_INSTALLED
        if (rewardedRequestPending)
        {
            LogAd("Rewarded ya solicitado; ignorando toque duplicado.");
            return;
        }
#endif

        currentContext = context;
        onRewardedSuccess = onSuccess;
        onRewardedFail = onFail;

#if LEVELPLAY_DEPENDENCIES_INSTALLED
        if (rewardedAd != null && rewardedAd.IsAdReady())
        {
            rewardedRequestPending = true;
            rewardedCompleted = false;
            rewardedAd.ShowAd();
            LogAd($"Rewarded solicitado. Contexto: {context}");
        }
        else
        {
            LogAd($"Rewarded no disponible. Contexto: {context}.");
            FailRewarded(onFail);
        }
#else
        LogAd("Rewarded no disponible: LevelPlay SDK no instalado.");
        FailRewarded(onFail);
#endif
    }

    /// <summary>
    /// Retorna true si hay un rewarded listo para mostrar.
    /// </summary>
    public bool IsRewardedReady()
    {
        if (!privacyEligibilityResolved || !mayServeAds || !isInitialized) return false;
        if (IsNoAdsActive()) return false;
#if LEVELPLAY_DEPENDENCIES_INSTALLED
        return isInitialized && rewardedAd != null && rewardedAd.IsAdReady();
#else
        return false;
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
        if (!privacyEligibilityResolved || !mayServeAds || !isInitialized)
        {
            LogAd("Interstitial bloqueado: privacidad/audiencia o entitlement aún no habilitados.");
            return;
        }

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

#if LEVELPLAY_DEPENDENCIES_INSTALLED
        if (interstitialAd != null && interstitialAd.IsAdReady())
        {
            interstitialAd.ShowAd();
            LogAd($"Interstitial mostrado. Contexto: {context}");
        }
        else
        {
            LogAd($"Interstitial no listo. Contexto: {context}. Recargando...");
            LoadInterstitial();
        }
#else
        LogAd("Interstitial no disponible: LevelPlay SDK no instalado.");
#endif
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Carga de ads
    // ─────────────────────────────────────────────────────────────────────────

    private void LoadInterstitial()
    {
#if LEVELPLAY_DEPENDENCIES_INSTALLED
        interstitialAd?.LoadAd();
#endif
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Eventos del SDK — Rewarded
    // ─────────────────────────────────────────────────────────────────────────

#if LEVELPLAY_DEPENDENCIES_INSTALLED
    private void OnLevelPlayInitialized(LevelPlayConfiguration configuration)
    {
        initializationInProgress = false;
        isInitialized = true;
        LevelPlay.OnInitSuccess -= OnLevelPlayInitialized;
        LevelPlay.OnInitFailed -= OnLevelPlayInitializationFailed;

        rewardedAd = new LevelPlayRewardedAd(GetRewardedAdUnitId());
        rewardedAd.OnAdLoaded += OnRewardedLoaded;
        rewardedAd.OnAdLoadFailed += OnRewardedLoadFailed;
        rewardedAd.OnAdDisplayed += OnRewardedDisplayed;
        rewardedAd.OnAdDisplayFailed += OnRewardedDisplayFailed;
        rewardedAd.OnAdRewarded += OnRewardedEarned;
        rewardedAd.OnAdClosed += OnRewardedClosed;
        rewardedAd.LoadAd();

        interstitialAd = new LevelPlayInterstitialAd(GetInterstitialAdUnitId());
        interstitialAd.OnAdLoaded += OnInterstitialReady;
        interstitialAd.OnAdLoadFailed += OnInterstitialLoadFailed;
        interstitialAd.OnAdDisplayed += OnInterstitialOpened;
        interstitialAd.OnAdDisplayFailed += OnInterstitialFailed;
        interstitialAd.OnAdClosed += OnInterstitialClosed;
        LoadInterstitial();

        LogAd("LevelPlay inicializado; formatos cargándose.");
        if (launchLevelPlayTestSuite && (Application.isEditor || Debug.isDebugBuild))
            LevelPlay.LaunchTestSuite();
    }

    private void OnLevelPlayInitializationFailed(LevelPlayInitError error)
    {
        initializationInProgress = false;
        LevelPlay.OnInitSuccess -= OnLevelPlayInitialized;
        LevelPlay.OnInitFailed -= OnLevelPlayInitializationFailed;
        LogAd($"Falló inicialización de LevelPlay: {error.ErrorMessage}");
    }

    private void OnRewardedLoaded(LevelPlayAdInfo info) => LogAd("Rewarded cargado.");
    private void OnRewardedLoadFailed(LevelPlayAdError error) => LogAd($"No cargó rewarded: {error}");

    private void OnRewardedDisplayed(LevelPlayAdInfo info)
    {
        timeScaleBeforeAd = Time.timeScale;
        Time.timeScale = 0f;
        LogAd("Rewarded mostrado.");
    }

    private void OnRewardedEarned(LevelPlayAdInfo info, LevelPlayReward reward)
    {
        if (!rewardedRequestPending || rewardedCompleted) return;
        rewardedCompleted = true;
        if (rewardCloseTimeout != null)
        {
            StopCoroutine(rewardCloseTimeout);
            rewardCloseTimeout = null;
        }

        LogAd($"Rewarded completado. Recompensa SDK: {reward.Name} x{reward.Amount}");
        onRewardedSuccess?.Invoke();
        ClearRewardedCallbacks();
    }

    private void OnRewardedClosed(LevelPlayAdInfo info)
    {
        Time.timeScale = timeScaleBeforeAd;
        if (rewardedRequestPending && !rewardedCompleted)
        {
            // LevelPlay documents reward and close as asynchronous; defer failure so
            // a reward callback arriving just after close can still grant exactly once.
            rewardCloseTimeout = StartCoroutine(FailRewardedAfterCloseGracePeriod());
        }
        else
        {
            ClearRewardedCallbacks();
        }

        rewardedAd?.LoadAd();
    }

    private void OnRewardedDisplayFailed(LevelPlayAdInfo info, LevelPlayAdError error)
    {
        Time.timeScale = timeScaleBeforeAd;
        LogAd($"No se pudo mostrar rewarded: {error}");
        FailRewarded(onRewardedFail);
        rewardedAd?.LoadAd();
    }

    private System.Collections.IEnumerator FailRewardedAfterCloseGracePeriod()
    {
        yield return new WaitForSecondsRealtime(5f);
        rewardCloseTimeout = null;
        if (rewardedRequestPending && !rewardedCompleted)
            FailRewarded(onRewardedFail);
    }

    private void OnInterstitialReady(LevelPlayAdInfo info) => LogAd("Interstitial listo.");
    private void OnInterstitialLoadFailed(LevelPlayAdError error) => LogAd($"No cargó interstitial: {error}");

    private void OnInterstitialOpened(LevelPlayAdInfo info)
    {
        timeScaleBeforeAd = Time.timeScale;
        Time.timeScale = 0f;
        lastInterstitialTime = Time.realtimeSinceStartup;
    }

    private void OnInterstitialClosed(LevelPlayAdInfo info)
    {
        Time.timeScale = timeScaleBeforeAd;
        LoadInterstitial();
    }

    private void OnInterstitialFailed(LevelPlayAdInfo info, LevelPlayAdError error)
    {
        Time.timeScale = timeScaleBeforeAd;
        LogAd($"Interstitial falló: {error}");
        LoadInterstitial();
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
        if (!isPurchased && privacyEligibilityResolved && mayServeAds)
            TryInitializeWhenEligible();
    }

    private void FailRewarded(Action onFail)
    {
        ClearRewardedCallbacks();
        onFail?.Invoke();
    }

    private void ClearRewardedCallbacks()
    {
        if (rewardCloseTimeout != null)
        {
            StopCoroutine(rewardCloseTimeout);
            rewardCloseTimeout = null;
        }
        onRewardedSuccess = null;
        onRewardedFail = null;
        rewardedRequestPending = false;
        rewardedCompleted = false;
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
