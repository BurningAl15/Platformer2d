# Fox's Tale — Handoff: Fuente de Verdad

> **FastTask:** tareas activas en [`../FastTask/tasks.json`](../FastTask/tasks.json). Manifest en [`../FastTask/project.json`](../FastTask/project.json).
>
> Este archivo es la fuente de verdad del proyecto. Si hay duda sobre qué falta, leer esto primero.
>
> **Última actualización:** 12/11/2026 — Kiro. ConsentManager, feedback visual del dado, .gitignore, ironSource sunset documentado. Build activo: 29 (Android) / 22+ (iOS).

---

## ⚠️ ACCIÓN URGENTE — ironSource Exchange cierra el 31/12/2026

**Qué cambia:** La red "ironSource Exchange" (iSX) deja de servir ads el 31/12/2026.
**Qué NO cambia:** LevelPlay, el SDK, el dashboard, las App Keys y los datos.

**Acción en el dashboard (no requiere código):**
1. Ir a [dashboard.is.com](https://dashboard.is.com) → Fox's Tale
2. En **Networks** de cada Ad Unit (Rewarded e Interstitial): activar **Unity Ads**
3. Hacerlo antes del **31 de diciembre de 2026**

---

> [!CAUTION]
> **CERO EMOJIS / ICONOS UNICODE EN TEXTMESHPRO.**
> Los atlas de fuentes SDF no contienen glifos emoji. Usar `Image` con sprite para cualquier ícono visual (gema, trébol, corazón, carrito). Texto = alfanumérico limpio en inglés.

---

## Estado actual del proyecto (12/11/2026)

### Branch activo
`codex/fix-horizontal-platform-riding` — 15+ commits por delante de `refactor/trying-refactor`.
Último commit: `a470991` — feedback visual del dado (Clover upgrade).

### Builds en tiendas
| Plataforma | Build | Estado |
|---|---|---|
| Android | versionCode 29 | ✅ Disponible en Play Internal Testing |
| iOS | build 22+ | ⚠️ Subido — procesamiento/disponibilidad en TestFlight sin confirmar |

---

## Lo que se hizo en esta sesión (12/11/2026 — Kiro)

### 1. ConsentManager.cs — NUEVO
`Assets/Scripts/Managers/ConsentManager.cs`

El bloqueador principal de los ads era que `AdsManager.SetPrivacyEligibilityResolved()` nunca tenía un caller. Ahora lo tiene.

- **iOS:** lanza el prompt ATT (`Unity.Advertisement.IosSupport.ATTrackingStatusBinding`)
- **Android:** usa LevelPlay UMP. Para audiencia 18+ el formulario se salta automáticamente en no-EEA
- **Editor/Standalone:** skip automático, resuelve elegible
- Cache en PlayerPrefs: no muestra el prompt de nuevo en arranques siguientes
- Se agrega automáticamente via `[RequireComponent]` en `AppBootstrap`
- **Commit:** `4f64925`

### 2. DiceRollController — feedback visual exagerado al gastar Clover
`Assets/Scripts/Systems/DiceRollController.cs`

`OnUpgradeWithTrebolPressed` reescrito con `PlayUpgradeSequence()` (coroutine):
1. Token del trébol vuela en arco desde el ícono HUD hasta la cara del dado
2. Flash blanco + `DOPunchScale` en la cara del dado al impactar
3. `DOPunchScale` sutil en el panel completo
4. Status text y contador de tréboles punchean
5. `ScreenShake` suave (0.2s, 0.04) como remate
6. Botón Upgrade se deshabilita inmediatamente (anti-doble-tap)
- **Commit:** `a470991`

### 3. .gitignore actualizado
Agregadas entradas para `.codex-attachments/` y `Assets/AddressableAssetsData/<platform>/` — ya no aparecen como untracked.
- **Commit:** `14eb623`

### 4. LevelManager + SaveManager — fixes
- `IsCurrentWorldComplete()`: ahora verifica que el nivel actual sea el último del mundo antes de mostrar WorldComplete
- `SaveManager.LoadRating()`: retrocompatibilidad — si había tiempo guardado pero sin rating, retorna 1 estrella
- **Commit:** `e1d0074`

### 5. AdsManager.cs — documentación ironSource sunset
Nota agregada en el docstring con la acción requerida en el dashboard.
- **Commit:** `67a4e1b`

---

## Pendientes P0 (bloquean producción)

### A. QA en dispositivo físico
Ningún build ha sido validado con QA completo en mano. Checklist:

| Área | Qué verificar |
|---|---|
| Controles táctiles | Sin mando y con GameSir X5s (conectar/desconectar en caliente) |
| Settings | Música, SFX, Haptics, Idioma, Reset Progress |
| GameOver | Ad / Trébol / Gemas / Volver al mapa — cada opción |
| DiceRoll | Animación de vuelo del trébol al dado, flash, nuevo rango visible antes de lanzar |
| Pickup animations | Clover vuela al HUD, HeartFragment vuela al slot de corazón |
| Ads | Consentimiento aceptado → rewarded disponible; denegado → ads bloqueados |
| Remove Ads | Compra y restauración en sandbox (Google License Tester / Apple Sandbox) |
| Safe areas | iPhone SE (16:9) y dispositivos con notch |
| Orientación | Landscape en ambas rotaciones |

### B. IAP Apple — captura para App Review
El producto `com.fatcatgamestudio.foxstale.noads` está en "Prepare for Submission" en App Store Connect. Falta una captura real del StorePanel en dispositivo para adjuntar en Review Information.

### C. ironSource Exchange → Unity Ads en dashboard
Ver sección urgente arriba. Fecha límite: 31/12/2026.

---

## Pendientes P1

### Consentimiento CMP completo (GDPR)
`ConsentManager.cs` maneja la elegibilidad, pero si se necesita un diálogo CMP explícito para EEA/UK, hay que integrar Google UMP SDK directamente. Para audiencia 18+ LevelPlay lo salta automáticamente en no-EEA — validar comportamiento en dispositivos europeos.

### Firebase Analytics y Crashlytics (P2 diferido)
Integración diferida hasta cerrar los P0 de monetización y QA. Ver FastTask `local-8ebeec1f`.

---

## Reglas de código vigentes (no negociables)

- **Cero `PlayerPrefs` directos** — todo a través de `SaveManager`
- **Cero `SceneManager.LoadScene` directos** — usar `SceneUtils` o `SceneTransitionManager`
- **Cero `DOTween.Init()` fuera de `AppBootstrap`**
- **Cero emojis en strings de TextMeshPro** — usar `Image` con sprite
- **Singletons** con `Destroy(gameObject)` en duplicados, nunca `_instance = this` directo
- **Texto de UI** alfanumérico limpio en inglés; strings de `GameLocalization.Get()` para todo lo visible al jugador

---

## Scripts nuevos en esta sesión

| Script | Ruta | Propósito |
|---|---|---|
| `ConsentManager` | `Assets/Scripts/Managers/` | CMP/ATT gateway para AdsManager |

## Scripts modificados en esta sesión

| Script | Cambio principal |
|---|---|
| `DiceRollController` | `PlayUpgradeSequence()` — feedback visual completo al gastar Clover |
| `LevelManager` | `IsCurrentWorldComplete()` más preciso |
| `SaveManager` | `LoadRating()` con retrocompatibilidad |
| `AdsManager` | Documentación del ironSource sunset |
| `AppBootstrap` | `[RequireComponent(typeof(ConsentManager))]` agregado |

---

## Referencia de documentación

| Archivo | Contenido |
|---|---|
| `.kiro/handoff.md` | **Este archivo** — estado del proyecto y pendientes |
| `.kiro/game-design-document.md` | GDD completo — diseño del juego, sistemas, decisiones |
| `FastTask/tasks.json` | Tareas activas con historial de cambios por fecha |
| `docs/ANTIGRAVITY_PRODUCTION_HANDOFF_2026-10-28.md` | Handoff de producción de Antigravity |
| `docs/MONETIZATION_RELEASE.md` | Estado de monetización y ficha de tiendas |
