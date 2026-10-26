# Fase 4 — Monetización (Ads e In-App Purchases NoAds): Reporte de cambios

**Fecha:** 26/10/2026
**Depende de:** Fases 0-3 completadas.

---

## Resumen ejecutivo

Se implementó la capa completa de monetización:
1. **Ads (Unity LevelPlay):** Arquitectura de wrapper aislada con soporte de rewarded e interstitials, cooldowns configurables y modo simulación sin SDK.
2. **IAP NoAds (RevenueCat):** Gestor de compra única para eliminar anuncios permanente, sincronizado con cache offline (`PlayerPrefs`), persistencia ante `ResetProgress()`, y desactivación automática en tiempo real de todos los ads en `AdsManager`.

---

## Archivos creados

### `Assets/Scripts/Managers/AdsManager.cs` (NUEVO)
Singleton con DontDestroyOnLoad. Wrapper sobre Unity LevelPlay SDK.

**Modos:**
- `#if UNITY_LEVELPLAY` definido → usa el SDK real
- Sin SDK → modo simulación (rewarded siempre exitoso en 0.5s)

**Integración NoAds:**
- Consulta `RevenueCatManager.Instance.IsNoAdsPurchased()` / `IsNoAdsActive()`.
- Si NoAds está activo, el rewarded otorga recompensa directamente y el interstitial es bloqueado de inmediato.

**API:**
```csharp
AdsManager.Instance.ShowRewarded(AdContext.GameOver, onSuccess, onFail)
AdsManager.Instance.ShowInterstitial(AdContext.ReturnToMap)
AdsManager.Instance.IsRewardedReady() → bool
AdsManager.Instance.IsNoAdsActive()   → bool
AdsManager.Instance.OnNoAdsStatusChanged(bool isPurchased)
```

**Contextos (AdContext):**
- `GameOver` — rewarded para ganar vidas y continuar
- `LevelComplete` — placeholder para futuras features
- `ReturnToMap` — interstitial al volver al mapa desde Game Over

**Configuración de cooldown:**
- `interstitialCooldownOnStart = 120s` — no muestra interstitial en los primeros 2 minutos
- `interstitialMinInterval = 60s` — mínimo 60s entre interstitials
- Precarga automática del siguiente interstitial al cerrar uno

### `Assets/Scripts/IAP/RevenueCatManager.cs` (NUEVO)
Singleton con DontDestroyOnLoad. Administrador de compras in-app via RevenueCat SDK.

**Características:**
- Manejo de SDK real con `#if UNITY_PURCHASING && UNITY_REVENUECAT`.
- Modo simulación automático si el SDK no está instalado.
- Cache offline en `PlayerPrefs` (`IAP_NoAds_Purchased`) para consulta sincrónica instantánea sin delays de red.
- Notificación automática a `AdsManager.Instance.OnNoAdsStatusChanged()` al concretar compra o restaurar.
- API:
  - `IsNoAdsPurchased()` → bool
  - `PurchaseNoAds(onSuccess, onError)`
  - `RestorePurchases(onSuccess, onError)`

### `Assets/Scripts/IAP/NoAdsPurchaseButton.cs` (NUEVO)
Componente de UI para botones en MainMenu, Settings o PauseMenu:
- Botón de compra y botón de restaurar compras.
- Oculta automáticamente los botones si NoAds ya está comprado.
- Manejo de estado de carga / loading overlay.

---

## Archivos modificados

### `Assets/Scripts/Managers/AdsManager.cs`
- Integración directa con `RevenueCatManager` y `IsNoAdsActive()`.
- Bloqueo de anuncios si NoAds fue adquirido.

### `Assets/Scripts/Data/SaveManager.cs`
- `ResetProgress()` ahora preserva la clave `IAP_NoAds_Purchased`, impidiendo que un reseteo de niveles elimine compras reales del usuario.

### `Assets/Scripts/Core/AppBootstrap.cs`
- Agregado `[RequireComponent(typeof(RevenueCatManager))]` y `[RequireComponent(typeof(AdsManager))]`.

### `Assets/Scripts/UI/GameOverController.cs`
- Botón "Ver Ad" conectado a `AdsManager.Instance.ShowRewarded()`.
- Botón "Volver al Mapa" conectado a `AdsManager.Instance.ShowInterstitial(AdContext.ReturnToMap)`.

---

## Acciones requeridas para activar ads e IAP reales

1. **Unity LevelPlay:**
   - Instalar package de LevelPlay desde Package Manager.
   - Project Settings → Scripting Define Symbols → agregar `UNITY_LEVELPLAY`.
   - Asignar App Keys en el Inspector de `AdsManager` (en el GameObject `AppBootstrap`).
2. **RevenueCat:**
   - Descargar e importar `.unitypackage` de RevenueCat desde GitHub releases (`purchases-unity`).
   - Project Settings → Scripting Define Symbols → agregar `UNITY_REVENUECAT` y `UNITY_PURCHASING`.
   - Asignar Android API Key (`goog_...`) e iOS API Key (`appl_...`) en `RevenueCatManager`.
   - Configurar Entitlement `NoAds` y el SKU del producto (`com.fatcatgamestudio.foxstale.noads`) en RevenueCat Dashboard.

---

## Cómo revertir

```bash
git checkout HEAD -- Assets/Scripts/UI/GameOverController.cs
git checkout HEAD -- Assets/Scripts/Core/AppBootstrap.cs
git checkout HEAD -- Assets/Scripts/Data/SaveManager.cs
git checkout HEAD -- Assets/Scripts/Managers/AdsManager.cs
# rm -rf Assets/Scripts/IAP
```
