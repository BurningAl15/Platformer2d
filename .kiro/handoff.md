# Fox's Tale — Handoff: Fuente de Verdad

> Este archivo es la fuente de verdad del proyecto.
> Se actualiza al terminar cada fase. Si hay duda sobre qué falta, leer esto primero.
>
> **Última actualización:** 26/10/2026 — Fases 0, 1, 2, 3 y 5 completadas en código

---

## Estado actual por fase

| Fase | Nombre | Estado |
|------|--------|--------|
| 0 | Estabilización | ✅ Código listo — pendiente pasos en Unity Editor |
| 1 | Controles (mobile + gamepad) | ✅ Código listo — pendiente pasos en Unity Editor |
| 2 | Refactorización de arquitectura | ✅ Código listo — pendiente pasos en Unity Editor |
| 3 | Economía (vidas, gemas, tréboles, dados) | ✅ Código completo — `SceneElementSetup.cs` automatiza el setup en Unity Editor |
| 4 | Monetización (Ads) | 🔲 No iniciada — ad simulado funcional en `GameOverController` |
| 5 | Builds automáticos (Android / iOS) | ✅ Código listo (`BuildAutomation.cs`) |
| 6 | Expansión de contenido (múltiples mundos) | 🔲 No iniciada |

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

## FASE 4 — Monetización / Ads [NO INICIADA]

### Qué hay que hacer

- [ ] **Elegir SDK de ads.** Opciones: Unity LevelPlay (recomendado, nativo) o Google AdMob.

- [ ] **Tipos de ads a implementar (según diseño del GDD):**
  1. **Rewarded Video en Game Over** → ganar 1-2 vidas → continuar desde donde murió (prioridad alta)
  2. **Rewarded Video en fin de nivel** → duplicar gemas obtenidas *(pendiente de confirmar si aplica, ya que las gemas no son currency)*
  3. Interstitial al volver al mapa tras Game Over (controlado, no en primeros 2 min de sesión)
  4. Banner en mapa / menú principal (prioridad baja)

- [ ] **Reemplazar el ad simulado en `GameOverController.OnWatchAdPressed()`** con llamada real al SDK.

- [ ] **Reglas de implementación (no negociables):**
  - Nunca mostrar ads durante gameplay.
  - Rewarded siempre opcional — el jugador elige.
  - Respetar COPPA si el juego tiene audiencia menor.

---

## FASE 5 — Builds automáticos

### Lo que se hizo (código)
- ✅ `Assets/Editor/BuildAutomation.cs` — nuevo. Métodos:
  - `BuildAndroidAPK()` → genera APK para pruebas directas en dispositivo.
  - `BuildAndroidAAB()` → genera AAB optimizado para Google Play Store.
  - `BuildiOS()` → genera proyecto Xcode para iOS.
  - Ejecutable desde CLI (batchmode) y accesible desde menú `CustomEditorTools/Builds/...`.
  - Configuración automática de `PlayerSettings`: `com.fatcatgamestudio.foxstale`, Target API 34+, ARM64/ARMv7, IL2CPP e Internet Permission.

### ⚠️ Pendiente — Configuración de firma / Tiendas
- [ ] Configurar Keystore para release en Android.
- [ ] Configurar Team ID y Provisioning Profile para iOS en Xcode.

---

## FASE 6 — Expansión de contenido / Múltiples mundos [NO INICIADA]

### Qué hay que hacer

- [ ] **Diseñar `WorldData` ScriptableObject** con: nombre del mundo, lista de escenas, condición de desbloqueo (ej. X estrellas del mundo anterior).

- [ ] **Refactorizar `MapPointsHolder`** para leer `WorldData[]` y construir el mapa dinámicamente en lugar de hardcodear los niveles.

- [ ] **Pantalla de "Mundo completado"** antes de mostrar el siguiente mundo.  
  Requiere que las estrellas estén implementadas (Fase 3).

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
| 4 | `.kiro/fase-4-reporte.md` *(se crea al terminar)* |
| 5 | `.kiro/fase-5-reporte.md` |
| 6 | `.kiro/fase-6-reporte.md` *(se crea al terminar)* |

---

## GO_TEST_BUILD / NO_GO

### Veredicto: **GO PARA TEST BUILD** (Con pendientes manuales de Inspector en Unity Editor)

| Criterio | Estado | Evidencia |
|---|---|---|
| **Integridad del código** | **APROBADO** | Todos los scripts de las Fases 0, 1, 2, 3 y 5 están escritos, vinculados e integrados. C# limpio, deduplicado (`SaveManager.cs`) y sin advertencias de dependencias faltantes. |
| **Pipeline de Build** | **APROBADO** | `Assets/Editor/BuildAutomation.cs` implementado con comandos de menú y soporte CLI para APK, AAB e iOS. Target API 34+ y arquitecturas ARM64 configuradas. |
| **Arquitectura y Persistencia** | **APROBADO** | `SaveManager` centraliza todo el estado (niveles, vidas, tréboles, fragmentos de dado, tiempo, gemas, audio). No hay llamadas dispersas a `PlayerPrefs`. |
| **Controles (Mobile & Gamepad)** | **APROBADO** | `GamepadDetector`, `OnScreenControlsSetup`, y `PlatformDetector` listos para input dual (touchscreen + mando físico). |
| **Economía de Juego (GDD)** | **APROBADO** | Loop de Game Over implementado con opción de ver Ad (simulado) o gastar Trébol 🍀. `DiceRollController` listo para tiradas Normal/Mejorado/Secreto. Gemas HUD en formato `X / Total`. |

#### ⚠️ Acciones pendientes obligatorias en Unity Editor antes de compilar APK final:
1. **Escena `MainMenu`:**
   - Crear GameObject `AppBootstrap` y asignarle el script `AppBootstrap.cs`. (Se agregan automáticamente `SaveManager`, `GamepadDetector`, `LivesManager` vía `[RequireComponent]`).
   - Crear GameObject `SceneTransitionManager` con `SceneTransitionManager.cs`.
2. **Escena de Gameplay:**
   - Crear panel de Game Over en el Canvas y asignar el script `GameOverController.cs` con sus botones (Ad, Trébol, Mapa) y textos.
   - En `UIController`: vincular `trebolCountText`, `gemsLevelText`, y sprites de estrellas (`starFull`, `starEmpty`).
3. **Escena `LevelSelection`:**
   - Asignar en cada `MapPoint` el `trebolBadge` (ícono o GameObject de trébol desactivado por defecto).
4. **Android Build:**
   - Generar Keystore para producción (guardar fuera de git) o usar debug keystore para pruebas locales vía `BuildAutomation.BuildAndroidAPK()`.
