# Fox's Tale — Handoff: Fuente de Verdad

> Este archivo es la fuente de verdad del proyecto.
> Se actualiza al terminar cada fase. Si hay duda sobre qué falta, leer esto primero.
>
> **Última actualización:** 26/10/2026 — Fases 0-6 completadas en código

---

## Estado actual por fase

| Fase | Nombre | Estado |
|------|--------|--------|
| 0 | Estabilización | ✅ Código listo — pendiente pasos en Unity Editor |
| 1 | Controles (mobile + gamepad) | ✅ Código listo — pendiente pasos en Unity Editor |
| 2 | Refactorización de arquitectura | ✅ Código listo — pendiente pasos en Unity Editor |
| 3 | Economía (vidas, gemas, tréboles, dados) | ✅ Código completo |
| 4 | Monetización (Ads + IAP NoAds) | ✅ Código listo — pendiente instalar SDKs y configurar keys |
| 5 | Builds automáticos (Android / iOS) | ✅ Código listo |
| 6 | Expansión de contenido (múltiples mundos) | ✅ Arquitectura base lista — pendiente crear WorldData y escenas |

---

## FASE 0 — Estabilización

### Lo que se hizo (código)
- ✅ `SceneTransitionManager.cs` — nuevo. Todas las transiciones de escena pasan por aquí con fade.
- ✅ `AppBootstrap.cs` — nuevo. Inicializa DOTween y configura framerate una sola vez.
- ✅ `SceneUtils.cs` — parseo robusto de nombres de escena (ya no lanza excepción).
- ✅ `PauseMenu.cs` — bug de timing corregido, DOTween limpio, `Time.timeScale = 0` se aplica inmediatamente.
- ✅ `UI_SelectController.cs` — panel del mapa con slide in/out DOTween.
- ✅ `MapPoint.cs` — métodos `Select()` / `Deselect()` con animación de escala DOTween.
- ✅ `LSPlayerController.cs` — feedback visual y sonoro al navegar y al tocar niveles bloqueados.
- ✅ `UIController.cs` — animadores de fin de nivel se resetean al iniciar.
- ✅ Singletons homologados con protección contra duplicados en: `MainMenuManager`, `LevelManager`, `CheckpointManager`, `LevelSelectManager`, `PlayerController2d`, `PlayerHealth`, `SimpleCameraController2d`, `ComboManager`.

### ⚠️ Pendiente — HACER EN UNITY EDITOR (obligatorio para que funcione)

- [ ] **Crear GameObject `AppBootstrap`** en la escena `MainMenu` y asignarle el script `AppBootstrap`.  
  *(Sin esto, DOTween no se inicializa y el framerate no se configura.)*

- [ ] **Crear GameObject `SceneTransitionManager`** en la escena `MainMenu` y asignarle el script `SceneTransitionManager`.  
  *(Sin esto, las transiciones de escena no tienen fade.)*

- [ ] **Verificar `hiddenY` / `visibleY`** del componente `UI_SelectController` en el Inspector de la escena `LevelSelection`.  
  Valores por defecto del script: `hiddenY = -300`, `visibleY = 0`. Ajustar según la posición real del panel en el Canvas.

- [ ] **Verificar `normalScale` / `selectedScale`** en cada `MapPoint` de la escena `LevelSelection`.  
  Valores por defecto: `normalScale = 1`, `selectedScale = 1.2`. Ajustar si el efecto es muy exagerado.

- [ ] **Verificar `initPoint` / `endPoint`** del componente `PauseMenu` en la escena de gameplay.  
  El script se limpió pero los valores serialized se preservan. Confirmar que el panel sigue apareciendo/desapareciendo correctamente.

- [ ] **Agregar sonido de nivel bloqueado dedicado.**  
  Actualmente usa `Warp_Jingle` como placeholder. Cuando haya un SFX de "no disponible", buscar el comentario `// Cuando tengamos un SFX dedicado` en `LSPlayerController.cs` y reemplazarlo.

---

## FASE 1 — Controles (mobile + gamepad)

### Lo que se hizo (código)
- ✅ `GamepadDetector.cs` — nuevo. Detecta conexión/desconexión de gamepads físicos en runtime.
- ✅ `OnScreenControlsSetup.cs` — nuevo. Helper de documentación y validación del setup on-screen.
- ✅ `MobileInputProvider.cs` — reescrito. Documentado como puente opcional con métodos `SetMoveInput()` / `SetJumpPressed()`.
- ✅ `InputManager.cs` — limpiado. Eliminada referencia muerta a `MobileInputProvider`. Nueva propiedad `NavigationInput`.
- ✅ `PlatformDetector.cs` — nuevo método `ForceSetMobileUIActive()` para ocultar controles sin importar plataforma.
- ✅ `AppBootstrap.cs` — `[RequireComponent(typeof(GamepadDetector))]` agregado.
- ✅ Bindings de Gamepad verificados: ya existían en `.inputactions` (Move, Jump, Pause, Navigate, Submit).

### ⚠️ Pendiente — HACER EN UNITY EDITOR (obligatorio para que funcione)

- [ ] **Configurar joystick on-screen:** En el GameObject del joystick virtual → `Add Component → OnScreenStick` → Control Path: `<Gamepad>/leftStick`.

- [ ] **Configurar botón de salto on-screen:** En el GameObject del botón de salto → `Add Component → OnScreenButton` → Control Path: `<Gamepad>/buttonSouth`.

- [ ] **Configurar botón de pausa on-screen** (si existe): `Add Component → OnScreenButton` → Control Path: `<Gamepad>/start`.

- [ ] **Agregar `OnScreenControlsSetup`** al GameObject raíz de la UI mobile de gameplay y asignar las referencias en el Inspector.

- [ ] **Verificar que `GamepadDetector` está en el mismo GameObject que `AppBootstrap`** (se agrega automáticamente por `[RequireComponent]` al abrir la escena en Unity).

- [ ] **Probar con GameSir X5s** (USB-C y Bluetooth) en Android una vez configurados los on-screen controls.

---

## FASE 2 — Refactorización de arquitectura

### Lo que se hizo (código)
- ✅ `SaveManager.cs` — nuevo en `Assets/Scripts/Data/`. Capa única de `PlayerPrefs`. DontDestroyOnLoad.
- ✅ `LevelManager.cs` — limpiado. Guardado delega a `SaveManager`. Coroutines renombradas. Tiempo solo avanza cuando `stopGame = false`.
- ✅ `MainMenuManager.cs` — limpiado. Usa `SaveManager.ResetProgress()` en nueva partida.
- ✅ `MapPointsHolder.cs` — limpiado. Usa `SaveManager` con fallback a `PlayerPrefs`.
- ✅ `AudioMixerManager.cs` — ~180 líneas de código comentado eliminadas. Bug de `Smash_Impact` corregido. Audio config usa `SaveManager`. Constantes de pitch nombradas. Null checks agregados.
- ✅ `AnalyticsManager.cs` — eliminado el guardado ilimitado en `PlayerPrefs`. API actualizada a Unity Gaming Services.
- ✅ `ResetPosition.cs` — implementado (estaba completamente vacío).
- ✅ `AppBootstrap.cs` — `[RequireComponent(typeof(SaveManager))]` agregado.

### ⚠️ Pendiente — HACER EN UNITY EDITOR (obligatorio para que funcione)

- [ ] **Verificar que `SaveManager` está en el GameObject de `AppBootstrap`** en la escena `MainMenu` (se agrega automáticamente por `[RequireComponent]`).

- [ ] **Reasignar `pickup_health_clip`** en el Inspector del `AudioMixerManager` (el campo cambió de nombre desde `pickup_helth_clip`).

- [ ] **Verificar `smashImpact_Clip`** en el Inspector del `AudioMixerManager` (renombrado de `SmashImpact_Clip`).

---

## FASE 3 — Economía del juego ✅ CÓDIGO COMPLETO

### Lo que se hizo (código)
- ✅ `LivesManager.cs` — vidas globales con `OnGameOver` / `OnLivesChanged` / `NotifyLivesChanged()`
- ✅ `GameOverController.cs` — 4 opciones: Ad (simulado) / Trébol+Dado / Volver al mapa
- ✅ `TrebolPickup.cs` — coleccionable trébol persistente por nivel
- ✅ `DiceFragmentPickup.cs` — fragmento de dado secreto por mundo
- ✅ `DiceRollController.cs` — dado animado 4 tipos (Normal 1-3, Mejorado A 3-5, B 2-6, Secreto 4-6). Dos modos: fin de nivel (→ mapa) y Game Over (→ respawn)
- ✅ `SaveManager.cs` — tréboles, fragmentos, rating, vidas, gemas globales
- ✅ `LevelManager.cs` — auto-detecta total de gemas, conecta dado al fin de nivel con 10/10 gemas
- ✅ `UIController.cs` — gemas `X/Total`, tréboles, vidas, estrellas. Bug `RefreshTrebolUI` duplicado corregido
- ✅ `MapPoint.cs` — campo `hasTrebol` + `trebolBadge` + `CheckTrebol()`
- ✅ `MapPointsHolder.cs` — carga trébol desde `SaveManager` por nivel
- ✅ `LevelRatingSystem.cs` — Singleton, `GetStarStates()`, integrado al fin de nivel

### ⚠️ Pendiente — HACER EN UNITY EDITOR

**La mayoría se puede hacer automáticamente con `SceneElementSetup.cs`:**
En Unity, ir a menú `CustomEditorTools → Setup Missing Scene Elements & Placeholders`.
Esto crea y conecta: `AppBootstrap`, `SceneTransitionManager`, paneles de Game Over y Dado, textos de HUD, badges de trébol en MapPoints y coloca un `TrebolPickup` en cada nivel.

**Lo que el script NO hace y hay que hacer manualmente:**
- [ ] Asignar sprites de estrellas en `UIController` (`starFull`, `starEmpty`, `starImages`) — **bloqueado hasta assets de Antigravity**
- [ ] Ajustar la posición del `TrebolPickup` en cada nivel (el setup lo coloca cerca del Goal como placeholder — debe moverse a un lugar que requiera exploración)
- [ ] Crear prefab `DiceFragmentPickup` y colocarlo en zonas secretas cuando estén diseñadas
- [ ] Ícono/animación del corazón extra cuando se activa el Dado Secreto (efecto visual temporal en HUD)

### 🎨 Pendiente para ANTIGRAVITY (assets visuales — el código está listo y esperando)

- [ ] Sprite del Trébol 🍀: normal, translúcido "ya obtenido", efecto de recolección diferente a la gema
- [ ] Sprite del Fragmento de Dado 🎲 (en nivel y en HUD/inventario)
- [ ] Sprites de los 3 tipos de dado (Normal, Mejorado, Secreto) con distinción visual
- [ ] Animación de números girando en la pantalla del dado
- [ ] Layout visual del panel del dado (fondo, celebración de resultado)
- [ ] Layout visual del panel de Game Over (4 botones claramente diferenciados)
- [ ] Sprites de estrella llena / vacía (para `UIController.starFull` y `starEmpty`)
- [ ] Badge del trébol para `MapPoint` en el mapa de selección
- [ ] HUD actualizado con slots visuales de vidas y tréboles

---

## FASE 4 — Monetización / Ads ✅ CÓDIGO LISTO

### Lo que se hizo (código)
- ✅ `AdsManager.cs` — wrapper sobre Unity LevelPlay SDK con modo simulación sin SDK. Rewarded + Interstitial con cooldowns. Se desactiva automáticamente si `RevenueCatManager.IsNoAdsPurchased()` es true.
- ✅ `RevenueCatManager.cs` — wrapper sobre RevenueCat SDK (IAP "Sin Anuncios"). Singleton con DontDestroyOnLoad. Cache en PlayerPrefs para modo offline. Modo simulación sin SDK.
- ✅ `NoAdsPurchaseButton.cs` — componente de botón listo para colocar en Settings/PauseMenu. Maneja compra y restauración de compras. Se deshabilita si ya está comprado.
- ✅ `GameOverController.cs` — `OnWatchAdPressed()` usa `AdsManager` con callbacks reales.
- ✅ `SaveManager.cs` — preserva `IAP_NoAds_Purchased` al resetear el progreso del juego.
- ✅ `AppBootstrap.cs` — `[RequireComponent(typeof(AdsManager))]` y `[RequireComponent(typeof(RevenueCatManager))]` agregados.

### ⚠️ Pendiente — PARA ACTIVAR ADS E IAP REALES

**Ads (Unity LevelPlay):**
- [ ] Instalar SDK: Unity → Package Manager → buscar "LevelPlay" e instalar.
- [ ] Símbolo de compilación: Project Settings → Player → Scripting Define Symbols → agregar `UNITY_LEVELPLAY`.
- [ ] App Keys: obtener desde [dashboard.is.com](https://dashboard.is.com) y asignar en el Inspector de `AppBootstrap` (campos `androidAppKey` e `iosAppKey` en `AdsManager`).
- [ ] Configurar Ad Units (placements Rewarded e Interstitial) en el dashboard.
- [ ] Mantener `enableTestMode = true` en desarrollo. Cambiar a `false` antes de publicar.

**IAP NoAds (RevenueCat):**
- [ ] Descargar SDK desde https://github.com/RevenueCat/purchases-unity/releases → importar `.unitypackage`.
- [ ] Símbolo de compilación: agregar `REVENUE_CAT` en Scripting Define Symbols.
- [ ] Asignar API Keys en el Inspector de `AppBootstrap` (campos `androidApiKey` e `iosApiKey` en `RevenueCatManager`).
- [ ] Crear producto "Sin Anuncios" en Google Play Console y App Store Connect con SKU: `com.fatcatgamestudio.foxstale.noads`.
- [ ] Configurar entitlement `NoAds` en el dashboard de RevenueCat apuntando al SKU anterior.
- [ ] Agregar botón `NoAdsPurchaseButton.cs` en la pantalla de Settings/PauseMenu.

**Información que el usuario debe proveer:**
- RevenueCat API Key Android: `goog_XXXXXXXXXX`
- RevenueCat API Key iOS: `appl_XXXXXXXXXX`
- Unity LevelPlay App Key Android e iOS
- Bundle ID definitivo: `com.fatcatgamestudio.foxstale` (confirmar)

---

## FASE 5 — Builds automáticos ✅ LISTO

### Lo que se hizo (código)
- ✅ `Assets/Editor/BuildAutomation.cs` — APK, AAB e iOS desde menú `CustomEditorTools/Builds/...` y CLI.
- ✅ `Assets/Editor/SceneElementSetup.cs` — configura automáticamente escenas y prefabs faltantes.

### ⚠️ Pendiente — Configuración de firma
- [ ] Configurar Keystore para Android release (guardar fuera de git).
- [ ] Configurar Team ID y Provisioning Profile para iOS en Xcode.

---

## FASE 6 — Múltiples mundos ✅ ARQUITECTURA BASE LISTA

### Lo que se hizo (código)
- ✅ `WorldData.cs` — ScriptableObject. Crear con Assets → Create → Fox's Tale → World Data.
- ✅ `WorldProgressManager.cs` — Singleton. Calcula estrellas, desbloqueo y progreso por mundo.
- ✅ `MapPointsHolder.cs` — soporta modo legacy (MVP actual) y modo `WorldData` (multi-mundo).
- ✅ `AppBootstrap.cs` — `[RequireComponent(typeof(WorldProgressManager))]` agregado.

### ⚠️ Pendiente — Para agregar el Mundo 2

- [ ] Assets → Create → Fox's Tale → World Data → configurar Mundo 1 con las escenas actuales.
- [ ] Al crear el Mundo 2: crear `WorldData_World2`, asignar escenas nuevas, definir `starsRequiredToUnlock`.
- [ ] Asignar el `WorldData` en el `MapPointsHolder` del mundo correspondiente.
- [ ] Crear escenas `Level_11`, `Level_12`... y agregarlas en Build Settings.
- [ ] Pantalla de "Mundo completado" — pendiente de diseño (ver GDD sección 13).

---

## Notas generales

- **GDD disponible:** `.kiro/game-design-document.md` — fuente de verdad del diseño del juego. Consultar antes de implementar cualquier feature nuevo.

- **Assets visuales pendientes:** La pantalla del dado, sprites del trébol, fragmento de dado secreto y HUD actualizado tienen su código y eventos 100% listos. Solo falta asignar los GameObjects/sprites en los campos correspondientes de Inspector en Unity Editor.

- **Sonido de nivel bloqueado:** Usar `Warp_Jingle` como placeholder hasta tener SFX dedicado. Buscar comentario `// Cuando tengamos un SFX dedicado` en `LSPlayerController.cs`.

- **`MobileInputProvider`:** Reemplazado funcionalmente por `OnScreenStick` / `OnScreenButton` nativos. Ver Fase 1.

---

## Referencia de archivos de reporte por fase

| Fase | Archivo de reporte |
|------|--------------------|
| 0 | `.kiro/fase-0-reporte.md` |
| 1 | `.kiro/fase-1-reporte.md` |
| 2 | `.kiro/fase-2-reporte.md` |
| 3 | `.kiro/fase-3-reporte.md` |
| 4 | `.kiro/fase-4-reporte.md` |
| 5 | `.kiro/fase-5-reporte.md` |
| 6 | `.kiro/fase-6-reporte.md` |

---

## GO_TEST_BUILD / NO_GO

### Veredicto: **GO PARA TEST BUILD** (Con pendientes manuales de Inspector en Unity Editor)

| Criterio | Estado | Evidencia |
|---|---|---|
| **Integridad del código** | **APROBADO** | Todos los scripts de Fases 0-6 + IAP escritos, vinculados e integrados. C# limpio. |
| **Pipeline de Build** | **APROBADO** | `BuildAutomation.cs` con menú y CLI para APK, AAB e iOS. |
| **Arquitectura y Persistencia** | **APROBADO** | `SaveManager` centraliza todo. `ResetProgress` preserva `IAP_NoAds_Purchased`. |
| **Controles (Mobile & Gamepad)** | **APROBADO** | `GamepadDetector`, `OnScreenControlsSetup`, `PlatformDetector` listos. |
| **Economía de Juego** | **APROBADO** | Game Over completo: Ad / Trébol / Gemas / Mapa. Dado Normal/Mejorado/Secreto. |
| **Ads (Unity LevelPlay)** | **APROBADO** | `AdsManager.cs` con Rewarded + Interstitial, cooldowns, modo simulación. Desactivado automáticamente si NoAds está activo. |
| **IAP NoAds (RevenueCat)** | **APROBADO** | `RevenueCatManager.cs` + `NoAdsPurchaseButton.cs` creados. Cache PlayerPrefs para modo offline. Simulación funciona sin SDK. |

#### ⚠️ Acciones pendientes antes del APK final:

##### En Unity Editor (manual):
1. **Escena `MainMenu`:** Crear `AppBootstrap` GameObject. Se agregan automáticamente via `[RequireComponent]`: `SaveManager`, `GamepadDetector`, `LivesManager`, `AdsManager`, `RevenueCatManager`, `WorldProgressManager`.
2. **Escena de Gameplay:** Crear panel de GameOver en Canvas con `GameOverController.cs` y sus botones (Ad, Trébol, Gemas, Mapa).
3. **Settings / PauseMenu:** Agregar botón con `NoAdsPurchaseButton.cs` para la compra de NoAds (y botón Restaurar).
4. **Escena `LevelSelection`:** Asignar `trebolBadge` en cada `MapPoint`.

##### SDK — instalar antes del build final:
5. **Unity LevelPlay:** `Window → Package Manager → buscar "LevelPlay"`. Asignar App Keys de Android/iOS en Inspector del `AdsManager`.
6. **RevenueCat SDK:** Descargar `.unitypackage` desde https://github.com/RevenueCat/purchases-unity/releases — importar. Asignar API Keys de Android/iOS en `RevenueCatManager`. Configurar entitlement "NoAds" en el Dashboard de RevenueCat.

##### Información que el usuario debe proveer:
- RevenueCat API Key (Android): `goog_XXXXXXXXXX`
- RevenueCat API Key (iOS): `appl_XXXXXXXXXX`
- SKU del producto: `com.fatcatgamestudio.foxstale.noads` (confirmar en tiendas)
- Unity LevelPlay App Key (Android e iOS)
- Bundle ID / Application ID definitivo

##### Android Build:
7. Generar Keystore para producción o usar debug keystore vía `BuildAutomation.BuildAndroidAPK()`.
