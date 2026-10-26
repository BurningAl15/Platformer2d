using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Componente para el botón "Comprar NoAds" en cualquier panel (Settings, Pausa, MainMenu).
///
/// SETUP EN UNITY EDITOR:
/// - Adjuntar este script a un GameObject con un Button.
/// - Asignar los campos en el Inspector.
/// - El botón se oculta automáticamente si NoAds ya está comprado.
///
/// USO:
///   El botón llama a RevenueCatManager.Instance.PurchaseNoAds().
///   Si la compra fue exitosa, el botón se oculta y se muestra feedback al usuario.
/// </summary>
public class NoAdsPurchaseButton : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private Button purchaseButton;
    [SerializeField] private Button restoreButton;

    [Tooltip("Texto del botón de compra (ej: 'Eliminar Anuncios')")]
    [SerializeField] private TextMeshProUGUI purchaseButtonLabel;

    [Tooltip("Panel o icono que se muestra cuando NoAds ya está activo")]
    [SerializeField] private GameObject noAdsActiveIndicator;

    [Header("Precio (mostrado en el botón hasta que el SDK provea el real)")]
    [SerializeField] private string fallbackPriceText = "$1.99";

    [Header("Feedback")]
    [SerializeField] private GameObject loadingOverlay;

    private void Start()
    {
        if (purchaseButton != null)
            purchaseButton.onClick.AddListener(OnPurchasePressed);

        if (restoreButton != null)
            restoreButton.onClick.AddListener(OnRestorePressed);

        RefreshUI();
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

        if (restoreButton != null)
            restoreButton.gameObject.SetActive(!hasPurchased);

        if (noAdsActiveIndicator != null)
            noAdsActiveIndicator.SetActive(hasPurchased);

        if (purchaseButtonLabel != null && !hasPurchased)
            purchaseButtonLabel.text = $"Sin Anuncios — {fallbackPriceText}";
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Eventos de botones
    // ─────────────────────────────────────────────────────────────────────────

    private void OnPurchasePressed()
    {
        if (RevenueCatManager.Instance == null)
        {
            Debug.LogWarning("[NoAdsPurchaseButton] RevenueCatManager no encontrado.");
            return;
        }

        SetLoading(true);

        RevenueCatManager.Instance.PurchaseNoAds(
            onSuccess: () =>
            {
                SetLoading(false);
                RefreshUI();
                Debug.Log("[NoAdsPurchaseButton] Compra exitosa. Anuncios desactivados.");
            },
            onError: errorMsg =>
            {
                SetLoading(false);
                if (errorMsg != "cancelled")
                {
                    // No mostrar error si el usuario canceló voluntariamente
                    Debug.LogWarning($"[NoAdsPurchaseButton] Error en compra: {errorMsg}");
                }
            }
        );
    }

    private void OnRestorePressed()
    {
        if (RevenueCatManager.Instance == null)
        {
            Debug.LogWarning("[NoAdsPurchaseButton] RevenueCatManager no encontrado.");
            return;
        }

        SetLoading(true);

        RevenueCatManager.Instance.RestorePurchases(
            onSuccess: () =>
            {
                SetLoading(false);
                RefreshUI();
                Debug.Log("[NoAdsPurchaseButton] Restauración completada.");
            },
            onError: errorMsg =>
            {
                SetLoading(false);
                Debug.LogWarning($"[NoAdsPurchaseButton] Error al restaurar: {errorMsg}");
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
}
