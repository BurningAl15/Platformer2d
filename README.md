# Fox's Tale

Plataformero 2D de aventura por mundos desarrollado en Unity. El jugador explora niveles con mecánicas de salto, combate por pisoteo y coleccionables que alimentan un sistema de economía de suerte.

**Estudio:** Fatcat Game Studio  
**Engine:** Unity (2D)  
**Plataformas objetivo:** Android, iOS  
**Branch principal de desarrollo:** `refactor/trying-refactor`

---

## Índice

- [Cómo abrir el proyecto](#cómo-abrir-el-proyecto)
- [Arquitectura del proyecto](#arquitectura-del-proyecto)
- [Sistemas principales](#sistemas-principales)
- [Documentación interna](#documentación-interna)
- [Estado del desarrollo](#estado-del-desarrollo)
- [Pendiente para Antigravity](#pendiente-para-antigravity)

---

## Cómo abrir el proyecto

1. Clonar el repositorio
2. Abrir con **Unity Hub** — versión recomendada: la que ya tienes instalada (ver `ProjectSettings/ProjectVersion.txt`)
3. Abrir la escena `MainMenu` como escena de arranque
4. Para correr el juego correctamente, verificar que existan los GameObjects de bootstrap en la escena `MainMenu` (ver sección de pendientes abajo)

---

## Arquitectura del proyecto

```
Assets/
├── Scripts/
│   ├── Audio/          AudioMixerManager — SFX, música, crossfade, combo de pitch
│   ├── Camera/         SimpleCameraController2d, LSCameraController, ScreenShake
│   ├── Core/           AppBootstrap, LevelManager, CheckpointManager,
│   │                   LevelSelectManager, MainMenuManager, SceneTransitionManager
│   ├── Data/           SaveManager — capa única de persistencia (PlayerPrefs)
│   ├── Enemies/        EnemyParent, EnemyController, FlyingEnemyController,
│   │                   BossTankController, BossTankHitbox, BossTankMine, Dart
│   ├── Environment/
│   │   ├── Hazards/    DamagePlayer, DeathZone, Smasher, Dart, etc.
│   │   ├── Interactables/ Goal, Checkpoint, Pickup, StompBox, BossSwitch, Bouncer
│   │   ├── Platforms/  MovingPlatform y variantes
│   │   └── Spawners/   SpawnerParent, SpawnerTime y variantes
│   ├── LevelSelection/ LSPlayerController, MapPoint, MapPointsHolder
│   ├── Managers/       AnalyticsManager
│   ├── Mobile/         PlatformDetector, GamepadDetector, MobileInputProvider,
│   │                   OnScreenControlsSetup, SafeAreaAdjuster
│   ├── Player/         PlayerController2d, PlayerMovement, PlayerJump,
│   │                   PlayerGroundCheck, PlayerHealth, PlayerKnockback,
│   │                   PlayerAnimator, PlayerEffects, PlatformRider
│   ├── Systems/        LivesManager, LevelRatingSystem, ComboManager
│   ├── UI/             UIController, FadeEffect, PauseMenu, UI_SelectController,
│   │                   GameOverController
│   └── Utils/          InputManager, SceneUtils, StringUtils, TurnOff_OnTime
├── Editor/             CountGemsInLevel, BuildAutomation (pendiente)
└── PlayerInputSystem.inputactions   — bindings de teclado, gamepad y on-screen
```

---

## Sistemas principales

### Input (`InputManager`)
Singleton con `DontDestroyOnLoad`. Puente único entre el Unity Input System y el resto del juego. Soporta teclado, gamepad físico (Xbox, PS, GameSir X5s via USB-C/Bluetooth) y controles on-screen (mobile).

**Eventos:** `OnJumpPressed`, `OnJumpReleased`, `OnPausePressed`, `OnSubmitPressed`

### Persistencia (`SaveManager`)
Capa única de acceso a `PlayerPrefs`. Ninguna otra clase llama `PlayerPrefs` directamente.  
Persiste: progreso de niveles, gemas récord, tiempos récord, rating de estrellas, vidas, tréboles (pendiente), configuración de audio.

### Vidas (`LivesManager`)
Singleton con `DontDestroyOnLoad`. Gestiona las vidas globales del jugador entre niveles.  
**Eventos:** `OnGameOver` (disparado cuando vidas = 0), `OnLivesChanged(int)`

### Audio (`AudioMixerManager`)
SFX por tipo (`SFXType`), 3 pistas de música con crossfade, pitch creciente para combos de gemas. Configuración de volumen persistida en `SaveManager`.

### Rating (`LevelRatingSystem`)
Calcula estrellas (1-3) por nivel: completar → 1★, todas las gemas → 2★, dentro del tiempo objetivo → 3★.

### Transiciones (`SceneTransitionManager`)
Todas las cargas de escena pasan por aquí con fade out antes de cargar. Singleton con `DontDestroyOnLoad`.

### Arranque (`AppBootstrap`)
Primer script en ejecutarse. Inicializa DOTween, configura framerate, agrega `SaveManager`, `LivesManager` y `GamepadDetector` automáticamente vía `[RequireComponent]`.

---

## Documentación interna

Toda la documentación del proyecto vive en la carpeta `.kiro/`:

| Archivo | Contenido |
|---------|-----------|
| `.kiro/handoff.md` | **Fuente de verdad del proyecto.** Estado de cada fase, pendientes de código y pendientes de Unity Editor |
| `.kiro/game-design-document.md` | **Diseño del juego.** Loop, vidas, gemas, trébol, dado de vidas, estrellas, HUD, decisiones pendientes |
| `.kiro/fase-0-reporte.md` | Cambios de la Fase 0 — Estabilización |
| `.kiro/fase-1-reporte.md` | Cambios de la Fase 1 — Controles |
| `.kiro/fase-2-reporte.md` | Cambios de la Fase 2 — Refactorización |
| `.kiro/fase-3-reporte.md` | Cambios de la Fase 3 — Economía |

> **Si no sabes qué falta o qué tocar:** leer `.kiro/handoff.md` primero.  
> **Si no sabes cómo funciona algo del juego:** leer `.kiro/game-design-document.md`.

---

## Estado del desarrollo

| Fase | Descripción | Estado |
|------|-------------|--------|
| 0 | Estabilización (transiciones, singletons, DOTween, mapa) | ✅ Código listo |
| 1 | Controles (mobile on-screen + gamepad físico) | ✅ Código listo |
| 2 | Refactorización (SaveManager, limpieza God Classes) | ✅ Código listo |
| 3 | Economía (vidas, estrellas, trébol, dado de vidas, corazón extra) | ✅ Código completo |
| 4 | Monetización (Ads Unity LevelPlay + IAP NoAds RevenueCat) | ✅ Código listo — pendiente instalar SDKs |
| 5 | Builds automáticos (Android AAB + iOS Xcode) | ✅ Código listo |
| 6 | Múltiples mundos (WorldData, WorldProgressManager) | ✅ Arquitectura base lista |

### Herramientas de editor disponibles (`CustomEditorTools` en Unity)
- `Setup Missing Scene Elements & Placeholders` — configura automáticamente toda la UI, prefabs y escenas
- `Builds → Build Android APK` — genera APK para testing
- `Builds → Build Android AAB` — genera bundle para Google Play
- `Builds → Build iOS` — genera proyecto Xcode
- `Get Gems in Level` — cuenta gemas en la escena abierta

---

## Pendiente para Antigravity

Lo siguiente requiere **assets visuales** antes de poder implementarse en pantalla. La lógica de código está preparada o documentada, pero no se puede conectar sin los sprites/animaciones.

### Assets requeridos

| Asset | Descripción | Usado en |
|-------|-------------|----------|
| 🍀 Sprite del Trébol | Normal, ya recogido, efecto de recolección (distinto a gema) | Nivel (pickup), HUD, MapPoint |
| 🎲 Sprite del Fragmento de Dado Secreto | En el nivel y en el HUD/inventario | Nivel (pickup), HUD |
| 🎲 Sprites de los 3 dados | Dado Normal, Mejorado, Secreto — visualmente distintos | Pantalla del dado |
| Pantalla del dado | Fondo, animación de números girando, resultado, celebración de vidas | Nueva escena/overlay |
| Animación: cambio de dado | Al gastar un Trébol, el dado cambia visualmente | Pantalla del dado |
| Animación: ensamblado del Dado Secreto | Al reunir todos los fragmentos del mundo | Overlay/notificación |
| ❤️ Ícono de corazón extra | Efecto visual temporal en HUD al activar el Dado Secreto | HUD de gameplay |
| Pantalla de Game Over | Layout completo con 4 botones: Ad / Trébol / Gemas / Mapa | Overlay en gameplay |
| HUD actualizado | Contador de vidas (x3) + contador de tréboles (🍀 x5) | HUD de gameplay |
| Badge 🍀 en MapPoint | Indicador visual de trébol obtenido en el mapa de selección | Escena LevelSelection |
| Estrellas en MapPoint | Sprites de estrella llena y vacía para los 3 slots | Escena LevelSelection |

### Lógica lista esperando assets

- `UIController.cs` — campos `livesText`, `trebolCountText`, `starImages`, `starFull`, `starEmpty` declarados, solo falta asignarlos en el Inspector
- `GameOverController.cs` — lógica completa, le falta el layout visual de los 4 botones
- `DiceRollController.cs` — animación lista, falta el fondo visual y sprites de dados
- `LevelRatingSystem.cs` — calcula estrellas, `GetStarStates()` listo para alimentar UI

### Setup pendiente en Unity Editor

Ver `.kiro/handoff.md` para la lista completa. Los más críticos:

1. **Ejecutar setup automático:** En Unity, ir a `CustomEditorTools → Setup Missing Scene Elements & Placeholders`. Configura automáticamente: `AppBootstrap`, `SceneTransitionManager`, paneles de Game Over y Dado, HUD de vidas y tréboles, badges en `MapPoint`, y `TrebolPickup` en cada nivel.

2. **Controles on-screen:** Agregar `OnScreenStick` al joystick (Control Path: `<Gamepad>/leftStick`) y `OnScreenButton` al botón de salto (Control Path: `<Gamepad>/buttonSouth`).

3. **En `AudioMixerManager`:** Reasignar `pickup_health_clip` (renombrado desde `pickup_helth_clip`).

4. **IAP NoAds:** Agregar botón `NoAdsPurchaseButton.cs` en la pantalla de Settings/PauseMenu. Ver sección de pendientes de Fase 4 en el handoff.

5. **Assets pendientes de Antigravity:** Sprites de estrella, trébol, fragmento de dado y tipos de dado. Ver tabla arriba.

---

## Convenciones de código

- **Singletons:** todos usan el patrón con `Destroy(gameObject)` en duplicados. No usar `_instance = this` directo.
- **PlayerPrefs:** solo a través de `SaveManager`. No llamar `PlayerPrefs` desde otras clases.
- **Transiciones de escena:** solo a través de `SceneUtils` o `SceneTransitionManager`. No llamar `SceneManager.LoadScene` directo.
- **Input:** solo a través de `InputManager`. No usar `Input.GetKey` ni `InputSystem` directo en gameplay.
- **DOTween:** inicializado en `AppBootstrap`. No llamar `DOTween.Init()` en otras clases.
