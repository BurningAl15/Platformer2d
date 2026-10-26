# Fase 4 — Monetización (Ads): Reporte de cambios

**Fecha:** 26/10/2026
**Depende de:** Fases 0-3 completadas.

---

## Resumen ejecutivo

Se implementó la capa de ads con arquitectura de wrapper que aísla el SDK del resto del juego. El código funciona en modo simulación sin el SDK instalado y está listo para conectar Unity LevelPlay cuando se active.

---

## Archivos creados

### `Assets/Scripts/Managers/AdsManager.cs` (NUEVO)
Singleton con DontDestroyOnLoad. Wrapper sobre Unity LevelPlay SDK.

**Modos:**
- `#if UNITY_LEVELPLAY` definido → usa el SDK real
- Sin SDK → modo simulación (rewarded siempre exitoso en 0.5s)

**API:**
```csharp
AdsManager.Instance.ShowRewarded(AdContext.GameOver, onSuccess, onFail)
AdsManager.Instance.ShowInterstitial(AdContext.ReturnToMap)
AdsManager.Instance.IsRewardedReady() → bool
```

**Contextos (AdContext):**
- `GameOver` — rewarded para ganar vidas y continuar
- `LevelComplete` — placeholder para futuras features
- `ReturnToMap` — interstitial al volver al mapa desde Game Over

**Configuración de cooldown:**
- `interstitialCooldownOnStart = 120s` — no muestra interstitial en los primeros 2 minutos
- `interstitialMinInterval = 60s` — mínimo 60s entre interstitials
- Precarga automática del siguiente interstitial al cerrar uno

---

## Archivos modificados

### `Assets/Scripts/UI/GameOverController.cs`
**`OnWatchAdPressed()`:**
- Ya no simula el ad directamente
- Llama `AdsManager.Instance.ShowRewarded()` con callbacks de éxito/fallo
- Si `AdsManager` no está disponible o el rewarded no está listo → fallback a simulación
- El botón "Ver Ad" se deshabilita durante el ad para evitar doble tap

**`OnReturnToMapPressed()`:**
- Agrega `AdsManager.Instance.ShowInterstitial(AdContext.ReturnToMap)`
- El AdsManager decide si mostrar o no según las reglas de cooldown

### `Assets/Scripts/Core/AppBootstrap.cs`
- `[RequireComponent(typeof(AdsManager))]` agregado

---

## Acciones requeridas para activar ads reales

1. **Instalar el SDK:** Unity → Window → Package Manager → buscar "LevelPlay" e instalar.
2. **Definir símbolo:** Project Settings → Player → Scripting Define Symbols → agregar `UNITY_LEVELPLAY`.
3. **Obtener App Keys** desde [dashboard.is.com](https://dashboard.is.com):
   - Crear una app en el dashboard
   - Copiar la App Key de Android y de iOS
   - Asignarlas en el Inspector del GameObject `AppBootstrap` (campos `androidAppKey` e `iosAppKey`)
4. **Configurar Ad Units** en el dashboard: crear placements de Rewarded Video e Interstitial.
5. **Test Mode:** Dejar `enableTestMode = true` durante desarrollo — cambiarlo a `false` para producción.
6. **COPPA:** Si el juego tiene audiencia menor de 13 años, agregar antes del init: `IronSource.Agent.setMetaData("is_coppa", "true")`.

---

## Pendiente para Antigravity

Ninguno — los ads no tienen UI custom, usan la UI nativa del SDK.

---

## Cómo revertir

```bash
git checkout HEAD -- Assets/Scripts/UI/GameOverController.cs
git checkout HEAD -- Assets/Scripts/Core/AppBootstrap.cs
# rm Assets/Scripts/Managers/AdsManager.cs
```
