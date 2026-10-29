using System;
using UnityEngine;

/// <summary>
/// Manager centralizado para compras in-app via RevenueCat.
/// Gestiona el producto NoAds (entitlement) con cache local en PlayerPrefs.
///
/// Prototipo de wrapper de compra. El SDK, los identificadores de tienda y el entitlement
/// deben verificarse antes de producción; ver la auditoría vigente en .kiro/handoff.md.
///
/// FLUJO:
///   RevenueCatManager.Instance.IsNoAdsPurchased()  → bool (sincrónico, usa cache)
///   RevenueCatManager.Instance.PurchaseNoAds(onSuccess, onError)
///   RevenueCatManager.Instance.RestorePurchases(onSuccess, onError)
/// </summary>
[RequireComponent(typeof(Purchases))]
public class RevenueCatManager : MonoBehaviour
{
    [Serializable]
    private sealed class LocalApiKeys
    {
        public string androidApiKey;
        public string iosApiKey;
    }

    public static RevenueCatManager Instance { get; private set; }
    public bool IsInitialized => _isInitialized;
    public bool IsEntitlementStatusResolved { get; private set; }

    public event Action<bool> EntitlementStatusResolved;
    public event Action PurchasesInitialized;

    // ─────────────────────────────────────────────────────────────────────────
    //  Configuración (asignar en Inspector)
    // ─────────────────────────────────────────────────────────────────────────

    [Header("RevenueCat API Keys — configurar localmente")]
    [Tooltip("API Key de Android — obtenida en app.revenuecat.com")]
    [SerializeField] private string androidApiKey = "goog_XXXXXXXXXX";
    [Tooltip("API Key de iOS — obtenida en app.revenuecat.com")]
    [SerializeField] private string iosApiKey = "appl_XXXXXXXXXX";
    [Tooltip("Test Store API key para pruebas aisladas cuando se active la opción siguiente")]
    [SerializeField] private string testStoreApiKey = "test_XXXXXXXXXX";
    [Tooltip("Solo para pruebas aisladas de RevenueCat Test Store; los builds Development usan la tienda real por defecto")]
    [SerializeField] private bool useTestStoreInDevelopmentBuild = false;

    [Header("Producto NoAds")]
    [Tooltip("Product ID en Google Play / App Store. Ej: com.fatcatgamestudio.foxstale.noads")]
    [SerializeField] private string noAdsProductId = "com.fatcatgamestudio.foxstale.noads";
    [Tooltip("Entitlement Remove Ads configurado en RevenueCat")]
    [SerializeField] private string noAdsEntitlementId = "remove_ads";

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
    private Purchases purchases;
    private LocalApiKeys localApiKeys;

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
        purchases = GetComponent<Purchases>();
        Initialize();
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Inicialización
    // ─────────────────────────────────────────────────────────────────────────

    private void Initialize()
    {
        if (Application.isEditor)
        {
            LogRC("RevenueCat no se ejecuta en Unity Editor. Ejecutar un build en dispositivo para probar compras.");
            return;
        }

        string apiKey = GetApiKey();
        if (string.IsNullOrEmpty(apiKey) || apiKey.Contains("XXXXXXXXXX"))
        {
            LogRC("API Key no configurada. RevenueCat no inicializado. " +
                  "Asignar la key en el Inspector de RevenueCatManager.");
            return;
        }

        Purchases.PurchasesConfiguration config = Purchases.PurchasesConfiguration.Builder
            .Init(apiKey)
            .Build();

        purchases.useRuntimeSetup = true;
        purchases.Configure(config);
        purchases.GetCustomerInfo(
            (customerInfo, error) =>
            {
                if (error != null || customerInfo == null)
                {
                    LogRC($"Error al obtener customer info al iniciar: {error}");
                    return; // Entitlement unresolved: ads remain blocked.
                }
                OnCustomerInfoReceived(customerInfo);
            }
        );

        _isInitialized = true;
        PurchasesInitialized?.Invoke();
        LogRC("SDK inicializado correctamente.");
    }

    private string GetApiKey()
    {
        if (localApiKeys == null)
        {
            TextAsset localConfig = Resources.Load<TextAsset>("RevenueCatPublicKeys.local");
            if (localConfig != null)
            {
                try
                {
                    localApiKeys = JsonUtility.FromJson<LocalApiKeys>(localConfig.text);
                }
                catch (Exception exception)
                {
                    LogRC($"No se pudo leer Assets/Resources/RevenueCatPublicKeys.local.json: {exception.GetType().Name}");
                    localApiKeys = new LocalApiKeys();
                }
            }
            else
                localApiKeys = new LocalApiKeys();
        }

#if DEVELOPMENT_BUILD
        if (useTestStoreInDevelopmentBuild)
            return testStoreApiKey;
#endif
#if UNITY_ANDROID
        return !string.IsNullOrEmpty(localApiKeys.androidApiKey)
            ? localApiKeys.androidApiKey
            : androidApiKey;
#elif UNITY_IOS
        return !string.IsNullOrEmpty(localApiKeys.iosApiKey)
            ? localApiKeys.iosApiKey
            : iosApiKey;
#else
        return androidApiKey;
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

    /// <summary>Returns the store-localized price for the exact configured Remove Ads product.</summary>
    public void GetNoAdsPrice(Action<string> onPrice, Action<string> onError = null)
    {
        if (!_isInitialized)
        {
            onError?.Invoke("Compras no inicializadas.");
            return;
        }

        purchases.GetOfferings((offerings, error) =>
        {
            if (error != null)
            {
                onError?.Invoke(error.Message);
                return;
            }

            Purchases.Package package = offerings?.Current?.AvailablePackages.Find(
                candidate => candidate.StoreProduct != null &&
                             candidate.StoreProduct.Identifier == noAdsProductId);
            if (package?.StoreProduct == null || string.IsNullOrEmpty(package.StoreProduct.PriceString))
            {
                onError?.Invoke("El producto Remove Ads aún no está configurado en RevenueCat.");
                return;
            }

            onPrice?.Invoke(package.StoreProduct.PriceString);
        });
    }

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
        if (!_isInitialized)
        {
            onError?.Invoke("Compras no inicializadas. Revisa la configuración de RevenueCat.");
            return;
        }

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

        _isPurchasing = true;
        LogRC($"Iniciando compra de: {noAdsProductId}");

        purchases.GetOfferings((offerings, offeringsError) =>
        {
            if (offeringsError != null)
            {
                _isPurchasing = false;
                LogRC($"Error al cargar offerings: {offeringsError.Message}");
                onError?.Invoke(offeringsError.Message);
                return;
            }

            if (offerings?.Current == null)
            {
                _isPurchasing = false;
                string msg = "No se pudieron cargar los offerings de RevenueCat.";
                LogRC(msg);
                onError?.Invoke(msg);
                return;
            }

            Purchases.Package package = offerings.Current.AvailablePackages.Find(
                candidate => candidate.StoreProduct != null &&
                             candidate.StoreProduct.Identifier == noAdsProductId);

            if (package == null)
            {
                _isPurchasing = false;
                string msg = $"Producto '{noAdsProductId}' no encontrado en el offering actual.";
                LogRC(msg);
                onError?.Invoke(msg);
                return;
            }

            purchases.PurchasePackage(package, result =>
                {
                    _isPurchasing = false;
                    if (result == null || result.UserCancelled)
                    {
                        onError?.Invoke("cancelled");
                        return;
                    }
                    if (result.Error != null)
                    {
                        LogRC($"Error en compra: {result.Error.Message}");
                        onError?.Invoke(result.Error.Message);
                        return;
                    }

                    OnCustomerInfoReceived(result.CustomerInfo);

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
                }
            );
        });
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
        if (!_isInitialized)
        {
            onError?.Invoke("Compras no inicializadas. Revisa la configuración de RevenueCat.");
            return;
        }

        purchases.RestorePurchases(
            (customerInfo, error) =>
            {
                if (error != null)
                {
                    LogRC($"Error al restaurar compras: {error.Message}");
                    onError?.Invoke(error.Message);
                    return;
                }
                if (customerInfo == null)
                {
                    const string message = "La tienda no devolvió información de compras.";
                    LogRC(message);
                    onError?.Invoke(message);
                    return;
                }

                OnCustomerInfoReceived(customerInfo);
                if (IsNoAdsPurchased())
                {
                    LogRC("Restauración completada. Remove Ads activo.");
                    onSuccess?.Invoke();
                }
                else
                {
                    const string message = "No se encontró una compra de Remove Ads para restaurar.";
                    LogRC(message);
                    onError?.Invoke(message);
                }
            }
        );
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Helpers internos
    // ─────────────────────────────────────────────────────────────────────────

    private void OnCustomerInfoReceived(Purchases.CustomerInfo customerInfo)
    {
        if (customerInfo == null) return;

        bool hasEntitlement =
            customerInfo.Entitlements.Active.ContainsKey(noAdsEntitlementId);

        SetNoAdsPurchasedLocally(hasEntitlement);
        LogRC($"CustomerInfo procesado. Entitlement '{noAdsEntitlementId}' activo: {hasEntitlement}");
    }

    /// <summary>
    /// Persiste el estado de NoAds en PlayerPrefs (cache offline).
    /// </summary>
    private void SetNoAdsPurchasedLocally(bool purchased)
    {
        PlayerPrefs.SetInt(KEY_NO_ADS, purchased ? 1 : 0);
        PlayerPrefs.Save();
        ResolveEntitlementStatus(purchased);

        // Notificar al AdsManager para que actualice su comportamiento en tiempo real.
        if (AdsManager.Instance != null)
            AdsManager.Instance.OnNoAdsStatusChanged(purchased);
    }

    private void ResolveEntitlementStatus(bool isPurchased)
    {
        IsEntitlementStatusResolved = true;
        EntitlementStatusResolved?.Invoke(isPurchased);
    }

    private void LogRC(string message)
    {
        if (logEvents)
            Debug.Log($"<color=cyan>[RevenueCat]</color> {message}");
    }
}
