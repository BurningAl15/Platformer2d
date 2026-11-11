using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

/// <summary>
/// Componente para el botón "Comprar NoAds" en cualquier panel (Settings, Pausa, MainMenu).
///
/// SETUP EN UNITY EDITOR:
/// - Adjuntar este script a un GameObject con un Button.
/// - Asignar los campos en el Inspector.
/// - Oculta la compra si Remove Ads ya está activo; mantiene Restaurar disponible.
///
/// USO:
///   El botón llama a RevenueCatManager.Instance.PurchaseNoAds().
///   Si la compra fue exitosa, el botón se oculta y se muestra feedback al usuario.
/// </summary>
public class NoAdsPurchaseButton : MonoBehaviour
{
    [Header("Panel de Tienda (Opcional)")]
    [Tooltip("CanvasGroup del panel completo de tienda para abrir/cerrar")]
    [SerializeField] private CanvasGroup storePanelGroup;
    [Tooltip("Botón que abre la tienda desde el menú principal o pausa")]
    [SerializeField] private Button openStoreButton;
    [Tooltip("Botón que cierra el panel de tienda")]
    [SerializeField] private Button closeStoreButton;

    [Header("Referencias UI")]
    [SerializeField] private Button purchaseButton;
    [SerializeField] private Button restoreButton;

    [Tooltip("Texto del botón de compra (ej: 'Eliminar Anuncios')")]
    [SerializeField] private TextMeshProUGUI purchaseButtonLabel;

    [Tooltip("Panel o icono que se muestra cuando NoAds ya está activo")]
    [SerializeField] private GameObject noAdsActiveIndicator;

    [Tooltip("Mensaje visible de resultado para compra o restauración")]
    [SerializeField] private TextMeshProUGUI statusText;

    [Header("Feedback")]
    [SerializeField] private GameObject loadingOverlay;

    private void Awake()
    {
        BindButtonListener(openStoreButton, OpenPanel);
        BindButtonListener(closeStoreButton, ClosePanel);
        BindButtonListener(purchaseButton, OnPurchasePressed);
        BindButtonListener(restoreButton, OnRestorePressed);
    }

    private void BindButtonListener(Button button, UnityAction listener)
    {
        if (button == null) return;

        button.onClick.RemoveListener(listener);
        for (int i = 0; i < button.onClick.GetPersistentEventCount(); i++)
        {
            if (button.onClick.GetPersistentTarget(i) == this &&
                button.onClick.GetPersistentMethodName(i) == listener.Method.Name)
                return;
        }

        button.onClick.AddListener(listener);
    }

    private void Start()
    {
        ConfigureAllTextsAutoResize();
        RefreshUI();
        RefreshPriceWhenReady();

        // Keep this object active: it owns the listeners that open the store.
        // Its CanvasGroup controls visibility and input while the panel is closed.
    }

    private void ConfigureAllTextsAutoResize()
    {
        ConfigureAutoResize(purchaseButtonLabel, 14f, 28f);
        ConfigureAutoResize(statusText, 12f, 22f);
        ConfigureButtonAutoResize(purchaseButton, 14f, 28f);
        ConfigureButtonAutoResize(restoreButton, 14f, 28f);
        ConfigureButtonAutoResize(closeStoreButton, 14f, 28f);
        ConfigureButtonAutoResize(openStoreButton, 14f, 28f);
    }

    private static void ConfigureAutoResize(TextMeshProUGUI tmp, float minSize, float maxSize)
    {
        if (tmp == null) return;
        tmp.enableAutoSizing = true;
        tmp.fontSizeMin = minSize;
        tmp.fontSizeMax = maxSize;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;
        tmp.overflowMode = TextOverflowModes.Ellipsis;
    }

    private static void ConfigureButtonAutoResize(Button btn, float minSize, float maxSize)
    {
        if (btn == null) return;
        var tmp = btn.GetComponentInChildren<TextMeshProUGUI>();
        ConfigureAutoResize(tmp, minSize, maxSize);
    }

    public void OpenPanel()
    {
        transform.SetAsLastSibling();
        if (storePanelGroup != null)
        {
            storePanelGroup.gameObject.SetActive(true);
            storePanelGroup.transform.SetAsLastSibling();
            storePanelGroup.alpha = 1f;
            storePanelGroup.interactable = true;
            storePanelGroup.blocksRaycasts = true;
        }
        SetStatus(string.Empty);
        RefreshUI();
        RefreshPriceWhenReady();
    }

    public void ClosePanel()
    {
        if (storePanelGroup != null)
        {
            storePanelGroup.alpha = 0f;
            storePanelGroup.interactable = false;
            storePanelGroup.blocksRaycasts = false;
        }
    }

    private void OnDestroy()
    {
        if (RevenueCatManager.Instance != null)
            RevenueCatManager.Instance.PurchasesInitialized -= LoadLocalizedPrice;
    }

    private void RefreshPriceWhenReady()
    {
        RevenueCatManager manager = RevenueCatManager.Instance;
        if (manager == null || !manager.IsInitialized)
        {
            if (manager != null)
                manager.PurchasesInitialized += LoadLocalizedPrice;
            return;
        }

        LoadLocalizedPrice();
    }

    private void LoadLocalizedPrice()
    {
        RevenueCatManager manager = RevenueCatManager.Instance;
        if (manager == null || purchaseButtonLabel == null)
            return;

        manager.PurchasesInitialized -= LoadLocalizedPrice;
        manager.GetNoAdsPrice(
            price =>
            {
                if (purchaseButtonLabel != null)
                    purchaseButtonLabel.text = $"Remove Ads - {price}";
            },
            error =>
            {
                if (purchaseButtonLabel != null)
                    purchaseButtonLabel.text = "Remove Ads";
                SetStatus("Price unavailable. Please try again later.");
                Debug.LogWarning($"[NoAdsPurchaseButton] No se pudo cargar el precio: {error}");
            });
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Actualizar UI según estado actual
    // ─────────────────────────────────────────────────────────────────────────

    private void RefreshUI()
    {
        bool hasPurchased = RevenueCatManager.Instance != null
            ? RevenueCatManager.Instance.IsNoAdsPurchased()
            : PlayerPrefs.GetInt("IAP_NoAds_Purchased", 0) == 1;

        if (purchaseButton != null)
            purchaseButton.gameObject.SetActive(!hasPurchased);

        // La restauración debe seguir accesible para quien ya compró en otro dispositivo.
        if (restoreButton != null)
            restoreButton.gameObject.SetActive(true);

        if (noAdsActiveIndicator != null)
            noAdsActiveIndicator.SetActive(hasPurchased);

        if (purchaseButtonLabel != null && !hasPurchased)
            purchaseButtonLabel.text = "Remove Ads";
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Eventos de botones
    // ─────────────────────────────────────────────────────────────────────────

    private void OnPurchasePressed()
    {
        SetStatus(string.Empty);
        if (RevenueCatManager.Instance == null)
        {
            SetStatus("Purchases unavailable right now.");
            return;
        }

        SetLoading(true);

        RevenueCatManager.Instance.PurchaseNoAds(
            onSuccess: () =>
            {
                SetLoading(false);
                RefreshUI();
                SetStatus("Purchase complete. Ads removed.");
                Debug.Log("[NoAdsPurchaseButton] Purchase successful. Ads removed.");
            },
            onError: errorMsg =>
            {
                SetLoading(false);
                if (errorMsg != "cancelled")
                {
                    SetStatus("Purchase could not be completed. Please try again.");
                    Debug.LogWarning($"[NoAdsPurchaseButton] Purchase error: {errorMsg}");
                }
            }
        );
    }

    private void OnRestorePressed()
    {
        SetStatus(string.Empty);
        if (RevenueCatManager.Instance == null)
        {
            SetStatus("Restore unavailable right now.");
            return;
        }

        SetLoading(true);

        RevenueCatManager.Instance.RestorePurchases(
            onSuccess: () =>
            {
                SetLoading(false);
                RefreshUI();
                SetStatus("Remove Ads purchase restored.");
                Debug.Log("[NoAdsPurchaseButton] Purchases restored.");
            },
            onError: errorMsg =>
            {
                SetLoading(false);
                SetStatus("No purchase found to restore.");
                Debug.LogWarning($"[NoAdsPurchaseButton] Restore error: {errorMsg}");
            }
        );
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Helpers
    // ─────────────────────────────────────────────────────────────────────────

    private void SetLoading(bool loading)
    {
        if (loadingOverlay != null)
            loadingOverlay.SetActive(loading);

        if (purchaseButton != null)
            purchaseButton.interactable = !loading;

        if (restoreButton != null)
            restoreButton.interactable = !loading;
    }

    private void SetStatus(string message)
    {
        if (statusText != null)
            statusText.text = message;
    }
}
