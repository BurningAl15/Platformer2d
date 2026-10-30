using UnityEngine;
using UnityEngine.UI;
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

    private void Start()
    {
        if (openStoreButton != null)
            openStoreButton.onClick.AddListener(OpenPanel);

        if (closeStoreButton != null)
            closeStoreButton.onClick.AddListener(ClosePanel);

        if (purchaseButton != null)
            purchaseButton.onClick.AddListener(OnPurchasePressed);

        if (restoreButton != null)
            restoreButton.onClick.AddListener(OnRestorePressed);

        RefreshUI();
        RefreshPriceWhenReady();
    }

    public void OpenPanel()
    {
        transform.SetAsLastSibling();
        if (storePanelGroup != null)
        {
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
