using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Manager centralizado para compras in-app via RevenueCat.
/// Gestiona el producto NoAds (entitlement) con cache local en PlayerPrefs.
///
/// SDK TARGET: RevenueCat Unity SDK
/// Documentación: https://www.revenuecat.com/docs/unity
///
/// SETUP REQUERIDO:
/// 1. Descargar el .unitypackage desde: https://github.com/RevenueCat/purchases-unity/releases
/// 2. Importar en el proyecto (Assets → Import Package → Custom Package).
/// 3. En el Inspector de este componente, asignar:
///    - androidApiKey: "goog_XXXXXXXXXX" (desde RevenueCat Dashboard)
///    - iosApiKey:     "appl_XXXXXXXXXX" (desde RevenueCat Dashboard)
///    - noAdsProductId: "com.fatcatgamestudio.foxstale.noads" (SKU del producto)
///    - noAdsEntitlementId: "NoAds" (entitlement configurado en RevenueCat)
/// 4. En el Dashboard de RevenueCat:
///    - Crear entitlement "NoAds"
///    - Crear offering con el producto one-time purchase
///    - Configurar los SKUs en Google Play Console y App Store Connect
///
/// FLUJO:
///   RevenueCatManager.Instance.IsNoAdsPurchased()  → bool (sincrónico, usa cache)
///   RevenueCatManager.Instance.PurchaseNoAds(onSuccess, onError)
///   RevenueCatManager.Instance.RestorePurchases(onSuccess, onError)
/// </summary>
public class RevenueCatManager : MonoBehaviour
{
    public static RevenueCatManager Instance { get; private set; }

    // ─────────────────────────────────────────────────────────────────────────
    //  Configuración (asignar en Inspector)
    // ─────────────────────────────────────────────────────────────────────────

    [Header("RevenueCat API Keys")]
    [Tooltip("API Key de Android — obtenida en app.revenuecat.com")]
    [SerializeField] private string androidApiKey = "goog_XXXXXXXXXX";
    [Tooltip("API Key de iOS — obtenida en app.revenuecat.com")]
    [SerializeField] private string iosApiKey = "appl_XXXXXXXXXX";

    [Header("Producto NoAds")]
    [Tooltip("Product ID en Google Play / App Store. Ej: com.fatcatgamestudio.foxstale.noads")]
    [SerializeField] private string noAdsProductId = "com.fatcatgamestudio.foxstale.noads";
    [Tooltip("ID del entitlement en RevenueCat Dashboard. Ej: NoAds")]
    [SerializeField] private string noAdsEntitlementId = "NoAds";

    [Header("Debug")]
    [SerializeField] private bool logEvents = true;

    // ─────────────────────────────────────────────────────────────────────────
    //  Claves de cache (PlayerPrefs)
    // ─────────────────────────────────────────────────────────────────────────

    private const string KEY_NO_ADS = "IAP_NoAds_Purchased";

    // ─────────────────────────────────────────────────────────────────────────
    //  Estado
    // ─────────────────────────────────────────────────────────────────────────

    private bool _isInitialized = false;
    private bool _isPurchasing  = false;

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
        Initialize();
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Inicialización
    // ─────────────────────────────────────────────────────────────────────────

    private void Initialize()
    {
#if UNITY_PURCHASING && UNITY_REVENUECAT
        string apiKey = GetApiKey();
        if (string.IsNullOrEmpty(apiKey) || apiKey.Contains("XXXXXXXXXX"))
        {
            LogRC("API Key no configurada. RevenueCat no inicializado. " +
                  "Asignar la key en el Inspector de RevenueCatManager.");
            return;
        }

        Purchases.PurchasesConfiguration config = Purchases.PurchasesConfiguration
            .Builder(apiKey)
            .Build();

        Purchases.Configure(config);
        Purchases.GetSharedInstance().GetCustomerInfo(
            customerInfo => OnCustomerInfoReceived(customerInfo),
            error        => LogRC($"Error al obtener customer info al iniciar: {error.message}")
        );

        _isInitialized = true;
        LogRC("SDK inicializado correctamente.");
#else
        // Sin SDK instalado: usar cache de PlayerPrefs como fuente de verdad.
        _isInitialized = true;
        LogRC("RevenueCat SDK no instalado. Usando cache local (PlayerPrefs).");
#endif
    }

    private string GetApiKey()
    {
#if UNITY_ANDROID
        return androidApiKey;
#elif UNITY_IOS
        return iosApiKey;
#else
        return androidApiKey; // Editor: Android key para pruebas
#endif
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  API PÚBLICA — Consulta de estado
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Retorna true si el jugador tiene NoAds activo.
    /// Usa el cache local para respuesta sincrónica inmediata.
    /// </summary>
    public bool IsNoAdsPurchased()
        => PlayerPrefs.GetInt(KEY_NO_ADS, 0) == 1;

    // ─────────────────────────────────────────────────────────────────────────
    //  API PÚBLICA — Compra
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Inicia el flujo de compra del producto NoAds.
    /// onSuccess: llamado si la compra fue exitosa.
    /// onError:   llamado con el mensaje de error si falló o se canceló.
    /// </summary>
    public void PurchaseNoAds(Action onSuccess, Action<string> onError = null)
    {
        if (_isPurchasing)
        {
            LogRC("Compra ya en progreso, ignorando llamada duplicada.");
            return;
        }

        // Si ya tiene NoAds, notificar directamente.
        if (IsNoAdsPurchased())
        {
            LogRC("NoAds ya comprado. Notificando éxito sin iniciar flujo.");
            onSuccess?.Invoke();
            return;
        }

#if UNITY_PURCHASING && UNITY_REVENUECAT
        _isPurchasing = true;
        LogRC($"Iniciando compra de: {noAdsProductId}");

        Purchases.GetSharedInstance().GetOfferings(offerings =>
        {
            if (offerings?.Current == null)
            {
                _isPurchasing = false;
                string msg = "No se pudieron cargar los offerings de RevenueCat.";
                LogRC(msg);
                onError?.Invoke(msg);
                return;
            }

            Purchases.Package package = offerings.Current.GetPackage(noAdsProductId);
            if (package == null)
            {
                // Intentar con el offering actual por defecto
                package = offerings.Current.Lifetime;
            }

            if (package == null)
            {
                _isPurchasing = false;
                string msg = $"Producto '{noAdsProductId}' no encontrado en el offering actual.";
                LogRC(msg);
                onError?.Invoke(msg);
                return;
            }

            Purchases.GetSharedInstance().PurchasePackage(
                package,
                (transaction, customerInfo) =>
                {
                    _isPurchasing = false;
                    OnCustomerInfoReceived(customerInfo);

                    if (IsNoAdsPurchased())
                    {
                        LogRC("Compra exitosa. NoAds activado.");
                        onSuccess?.Invoke();
                    }
                    else
                    {
                        string msg = "Compra procesada pero el entitlement NoAds no se activó.";
                        LogRC(msg);
                        onError?.Invoke(msg);
                    }
                },
                (error, userCancelled) =>
                {
                    _isPurchasing = false;
                    if (userCancelled)
                    {
                        LogRC("Compra cancelada por el usuario.");
                        onError?.Invoke("cancelled");
                    }
                    else
                    {
                        LogRC($"Error en compra: {error.message}");
                        onError?.Invoke(error.message);
                    }
                }
            );
        },
        error =>
        {
            _isPurchasing = false;
            LogRC($"Error al cargar offerings: {error.message}");
            onError?.Invoke(error.message);
        });
#else
        // Simulación: activa NoAds localmente para pruebas de flujo.
        LogRC("[SIMULACIÓN] Compra NoAds exitosa (SDK no instalado).");
        SetNoAdsPurchasedLocally(true);
        onSuccess?.Invoke();
#endif
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  API PÚBLICA — Restaurar compras
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Restaura compras anteriores del usuario (requerido por App Store / Play Store).
    /// onSuccess: llamado si se restauró al menos NoAds, o si ya lo tenía.
    /// onError:   llamado si falló la restauración.
    /// </summary>
    public void RestorePurchases(Action onSuccess, Action<string> onError = null)
    {
        LogRC("Iniciando restauración de compras...");

#if UNITY_PURCHASING && UNITY_REVENUECAT
        Purchases.GetSharedInstance().RestorePurchases(
            customerInfo =>
            {
                OnCustomerInfoReceived(customerInfo);
                LogRC($"Restauración completada. NoAds: {IsNoAdsPurchased()}");
                onSuccess?.Invoke();
            },
            error =>
            {
                LogRC($"Error al restaurar compras: {error.message}");
                onError?.Invoke(error.message);
            }
        );
#else
        // Simulación: restaurar desde cache.
        LogRC("[SIMULACIÓN] Restauración completada (SDK no instalado). Cache sin cambios.");
        onSuccess?.Invoke();
#endif
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Helpers internos
    // ─────────────────────────────────────────────────────────────────────────

#if UNITY_PURCHASING && UNITY_REVENUECAT
    private void OnCustomerInfoReceived(Purchases.CustomerInfo customerInfo)
    {
        if (customerInfo == null) return;

        bool hasEntitlement =
            customerInfo.Entitlements.Active.ContainsKey(noAdsEntitlementId);

        SetNoAdsPurchasedLocally(hasEntitlement);
        LogRC($"CustomerInfo procesado. Entitlement '{noAdsEntitlementId}' activo: {hasEntitlement}");
    }
#endif

    /// <summary>
    /// Persiste el estado de NoAds en PlayerPrefs (cache offline).
    /// </summary>
    private void SetNoAdsPurchasedLocally(bool purchased)
    {
        PlayerPrefs.SetInt(KEY_NO_ADS, purchased ? 1 : 0);
        PlayerPrefs.Save();

        // Notificar al AdsManager para que actualice su comportamiento en tiempo real.
        if (AdsManager.Instance != null)
            AdsManager.Instance.OnNoAdsStatusChanged(purchased);
    }

    private void LogRC(string message)
    {
        if (logEvents)
            Debug.Log($"<color=cyan>[RevenueCat]</color> {message}");
    }
}
