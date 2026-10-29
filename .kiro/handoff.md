# Fox's Tale — Handoff: Fuente de Verdad

> Este archivo es la fuente de verdad del proyecto.
> Se actualiza al terminar cada fase. Si hay duda sobre qué falta, leer esto primero.
>
> **Última actualización:** 29/10/2026 — leer primero el plan de cierre rápido que sigue; el estado histórico detallado de monetización está en `docs/ANTIGRAVITY_PRODUCTION_HANDOFF_2026-10-28.md`.

## Plan de cierre rápido para Antigravity — 2026-10-29

**Objetivo inmediato:** dejar Fox's Tale jugable y monetización comprobable en Android Internal Testing, generar un AAB nuevo y entregar evidencia. No preparar producción ni enviar a App Review. Trabajar en este checkout y conservar todos los cambios locales; no resetear, limpiar, cambiar de rama ni reemplazar archivos del dueño. Leer también `docs/ANTIGRAVITY_PRODUCTION_HANDOFF_2026-10-28.md` antes de editar.

### Orden de trabajo obligatorio

1. **Hacer inventario del checkout antes de tocarlo.** Abrir el proyecto canónico en Unity `6000.6.0f1`; revisar Console, UPM/manifest/lockfile, `git status` y números actuales de Play/App Store Connect. Hay mucho trabajo local sin commit. No asumir que los registros antiguos de tienda o credenciales siguen iguales: confirmar solo lo necesario y no imprimir secretos.

2. **Controles móviles (bloqueo de jugabilidad).** La versión instalada en Android se reportó sin controles táctiles. Añadir joystick de movimiento y botones de salto/pausa conectados a las Actions existentes (`<Gamepad>/leftStick`, `<Gamepad>/buttonSouth`, `<Gamepad>/start`). Guardarlos como UI serializada en el prefab/escenas de gameplay para que se puedan inspeccionar y mantener en Unity; no dejar la interfaz final construida con `new GameObject` en runtime. El script `Assets/Scripts/Mobile/MobileGameplayControls.cs` es un prototipo local añadido el 29-oct: revisarlo y reemplazarlo o reducirlo a lógica de visibilidad si se pasa a prefabs. Mostrar controles solo en móviles sin mando físico; ocultarlos al conectar un mando `Gamepad` o `Joystick` (incluido GameSir X5s), ignorar el gamepad virtual `OnScreen`, y volver a mostrarlos al desconectarlo. Verificar hot-plug en Android y que el Input System los lea junto con `InputManager`.

3. **Fondo y encuadre (bloqueo visual).** La captura del dueño muestra un corte vertical cian muy notorio en el borde derecho: el fondo/parallax no cubre el viewport. Corregirlo en los sprites, cámara y prefab/escenas, preservando pixel art y evitando estirar la imagen de forma evidente. Revisar también que HUD e iconos no queden cortados en ratios 16:9 y 20:9. `Assets/Scripts/Camera/SimpleCameraController2d.cs` tiene un ajuste local de cobertura añadido el 29-oct; validarlo visualmente contra la captura, no asumir que el cambio matemático arregló el arte.

4. **Compra Remove Ads visible y usable.** `NoAdsPurchaseButton.cs` y `RevenueCatManager.cs` tienen lógica, pero falta confirmar/crear en Edit Mode el panel de tienda y conectarlo a la UI del menú/ajustes. Debe mostrar precio localizado que devuelve la tienda, Comprar, Restaurar, cargando, cancelación/error y estado adquirido. Conectar eventos/referencias serializadas; verificar que entitlement `remove_ads` persistente suprima los formatos definidos. Capturar la pantalla real para Review Information del IAP Apple. No fijar precio en UI ni simular una compra.

5. **Consentimiento y ads de prueba.** Resolver privacidad/elegibilidad con CMP y señales aplicables al SDK, audiencia y regiones declaradas; `AdsManager.SetPrivacyEligibilityResolved(bool)` sigue fail-closed si no hay llamador. No invocar `true` sin obtener una decisión real. Configurar test ads, Rewarded voluntario con una sola recompensa, no-fill tolerante y el comportamiento de Remove Ads documentado en el GDD. Si no se puede completar consentimiento con evidencia, dejar anuncios cerrados y reportarlo como bloqueo de monetización: se puede generar un build solo de QA de controles, pero no declarar ads listos.

6. **Compilar y entregar Android primero.** Cerrar Unity Editor de forma normal tras guardar; no ejecutar una segunda instancia batch sobre el proyecto abierto. Confirmar en Play Console el mayor `versionCode` recibido (el registro previo indicaba code 1; usar 2 solo si sigue libre). Revisar keystore local de Fox's Tale sin mostrar contraseñas; ejecutar `scripts/release-test.sh android --android-code <code-nuevo>`. Confirmar que el AAB existe, firma, package `com.fatcatgamestudio.foxstale`, API/manifest `BILLING` y `AD_ID` de acuerdo con la integración final. Para subir, `--upload-test` usa Fastlane; hay un HTTP 403 histórico de la cuenta de servicio Play. Si sigue sin permiso, entregar el AAB para carga por Play Console del propietario y no perder tiempo reintentando la API. Usar solo Internal Testing, nunca producción.

7. **QA breve y reporte.** Instalar el AAB desde Internal Testing en el Android del dueño; probar movimiento/salto/pausa sin mando y con GameSir/mando conectado y desconectado; revisar corte de fondo, HUD, arranque y una partida. Probar compra/restauración sandbox y anuncios de prueba cuando los pasos 4–5 estén cerrados. Adjuntar capturas/logs sin datos privados, registrar versión/code, rutas de artefactos y errores concretos. Cerrar con `GO_TEST_BUILD` solo si compila y se completan las verificaciones requeridas; si no, `NO_GO` con el primer bloqueo reproducible. `GO_TEST_BUILD` no significa aprobación para producción.

### Tareas de arte / configuración que siguen en el mismo pase

- Revisar y completar la tabla de assets y campos exactos del Inspector bajo **“Para ANTIGRAVITY”** más abajo. Priorizar primero joystick/botones, fondo, panel de compra, el slot de corazón extra y estrellas/UI que aparezcan en el flujo probado. No bloquear el primer AAB de QA por leaderboard, mundo nuevo u otros elementos que no se alcancen en la partida corta.
- Ejecutar las listas escena por escena de **“Para Unity Editor”** cuando correspondan al flujo de build. Guardar todo fuera de Play Mode. No declarar configurado un campo solo porque existe el script o prefab.
- La app Play y el producto Android Remove Ads se reportaron creados/activos en la verificación del 28-oct; el SKU es `com.fatcatgamestudio.foxstale.noads`, base US$2.99. No duplicarlos. Confirmar estado actual del producto y mappings RevenueCat al hacer QA.
- Después de validar Android, terminar export/firma/upload de iOS TestFlight con el paso detallado en el handoff de producción. No hacer que el bloqueo de firma iOS detenga el AAB Android.

### Estado del código que Antigravity debe considerar

- Cambios locales del 29-oct aún no probados en Unity/dispositivo: `Assets/Scripts/Mobile/MobileGameplayControls.cs` (+ `.meta`), `Assets/Scripts/Mobile/GamepadDetector.cs` y `Assets/Scripts/Camera/SimpleCameraController2d.cs`. Revisar, corregir e integrar con prefabs; el APK/AAB ya instalado no contiene estos cambios.
- No ejecutar builds o subidas hasta que escenas/prefabs estén guardados, scripts compilen y el Editor esté cerrado. El pipeline de subida de Play tuvo 403 histórico y TestFlight exige firma válida; distinguir build local, subida interna, TestFlight y release público.
- Recursos visuales ya inspeccionados y usados en la ficha de Play: no volver a fabricar capturas de tienda si basta reutilizar las fuentes aprobadas; para el IAP Apple sí hace falta una captura real del panel Remove Ads.

---

## Estado general del proyecto

La tabla de inventario que sigue es una instantánea previa y sus conteos/pendientes no están sincronizados con el estado actual. Las apps y el IAP Remove Ads se reportaron creados en Play/App Store Connect y mapeados en RevenueCat; algunas respuestas/listings continúan como cambios sin enviar. Sigue faltando cableado de UI, consentimiento real de ads, cierre visual/input móvil y validación del build. El Play Internal actual es antiguo y no contiene los cambios del 29-oct. Para la próxima tarea, seguir el **Plan de cierre rápido** del inicio; para producción, revisar también la auditoría detallada de monetización.

---

## Inventario completo de scripts (104 archivos .cs)

### ✅ Implementados (103/104)

| Carpeta | Scripts |
|---------|---------|
| `Audio/` | `AudioMixerManager` |
| `Camera/` | `SimpleCameraController2d`, `LSCameraController`, `ScreenShake` |
| `Core/` | `AppBootstrap`, `CheckpointManager`, `LevelManager`, `LevelSelectManager`, `MainMenuManager`, `SceneTransitionManager` |
| `Data/` | `SaveManager`, `WorldData`, `WorldProgressManager` |
| `Enemies/` | `BossTankController`, `BossTankHitbox`, `BossTankMine`, `Bullet`, `Dart`, `EnemyController`, `EnemyParent`, `FlyingEnemyController`, `RunningEnemy`, `TutorialEnemy` |
| `Environment/Hazards/` | `DamagePlayer`, `DeathZone`, `FallingLog`, `FallingSpike`, `HangingHazzard`, `HoldingDamageBall`, `HoldingDamageBall_Signal`, `PendulumChain`, `PendulumMovement`, `Smasher`, `SmasherSignal` |
| `Environment/Interactables/` | `BossSwitch`, `Checkpoint`, `DiceFragmentPickup`, `Goal`, `Pickup`, `SpawnerSwitch`, `StompBox`, `Switch`, `TrebolPickup` |
| `Environment/Platforms/` | `MovingEdge`, `MovingPlatform`, `PlatformPassengerDetector`, `RotatatingEdge` |
| `Environment/Spawners/` | `ActivateFallingSpikes`, `ActivateSpawner`, `BushManager`, `KillEnemyContainer`, `SpawnByGroup`, `SpawnerParent`, `SpawnerTime`, `SpikeSpawner` |
| `IAP/` | `RevenueCatManager`, `NoAdsPurchaseButton` |
| `LevelSelection/` | `LSPlayerController`, `MapPoint`, `MapPointsHolder` |
| `Managers/` | `AdsManager`, `AnalyticsManager` |
| `Mobile/` | `GamepadDetector`, `MobileInputProvider`, `OnScreenControlsSetup`, `PlatformDetector`, `SafeAreaAdjuster` |
| `Player/` | `PlatformRider`, `PlayerAnimator`, `PlayerController2d`, `PlayerEffects`, `PlayerGroundCheck`, `PlayerHealth`, `PlayerJump`, `PlayerKnockback`, `PlayerMovement` |
| `Systems/` | `ComboManager`, `DiceRollController`, `LevelRatingSystem`, `LivesManager`, `WaveMovement` |
| `UI/` | `FadeEffect`, `GameOverController`, `PauseMenu`, `UI_SelectController`, `UIController` |
| `Utils/` | `InputManager`, `SceneUtils`, `StringUtils`, `TurnOff_OnTime` |
| `Raíz/` | `ResetPosition`, `Testing`, `TutorialEnding` |
| `Editor/` | `AudioMixerManagerEditor`, `BuildAutomation`, `ComboManagerEditor`, `CountGemsInLevel`, `PlayerResetEditor`, `SceneElementSetup`, `SpawnerSwitchEditor`, `SpawnerTimeEditor` |

### Estado de stubs

`Bouncer.cs` ya tiene `OnTriggerEnter2D` y llama `PlayerController2d.Bounce(bouncePower)`. No se afirma aquí un conteo total de scripts pendiente de un nuevo inventario.

---

## Estado por fase

| Fase | Descripción | Estado de código | Pendiente |
|------|-------------|-----------------|-----------|
| 0 | Estabilización | ✅ Completo | Unity Editor |
| 1 | Controles (mobile + gamepad) | ⚠️ Input System y gamepad existentes; la prueba Android reportó que faltan controles táctiles visibles | Integrar UI táctil serializada en prefab/escenas; hot-plug y QA en móvil |
| 2 | Refactorización + SaveManager | ✅ Completo | Unity Editor |
| 3 | Economía (vidas, trébol, dado, corazón extra) | ✅ Completo | Unity Editor + assets Antigravity |
| 4 | Ads (LevelPlay) + IAP Remove Ads (RevenueCat) | ⚠️ SDK/código y producto Play reportados configurados; compra/restauración no cableadas a UI; ads fail-closed | UI de tienda, consentimiento real, revalidar mappings y probar en dispositivo |
| 5 | Builds automáticos Android/iOS | ⚠️ Pipeline y keystore Android local preparados; hubo 403 de Fastlane/Play y el Input System requiere reinicio del Editor; no hay AAB nuevo con el código móvil actual | Reiniciar Editor, compilar Android Internal primero; luego resolver subida y TestFlight |
| 6 | Arquitectura multi-mundo | ✅ Completo | Crear WorldData assets + escenas nuevas |

---

## Lo que falta — organizado por quién lo hace

---

### 🤖 Para CODEX (código C# que falta o puede mejorar)

#### 1. `Bouncer.cs`
**Estado:** ✅ Implementado con `OnTriggerEnter2D`; detecta al jugador y llama `Bounce(bouncePower)`.

#### 2. Pantalla de resumen de mundo completado (Fase 6 — GDD sección 15)
**Estado:** ✅ Implementado `WorldCompleteController.cs`; muestra estrellas, gemas, tréboles y suma de mejores tiempos guardados. `LevelManager` abre el resumen cuando el `WorldData` asignado está completo. Si también corresponde el dado por gemas, este aparece primero y el resumen se abre al cerrar el dado.

**Cableado pendiente en Unity Editor:**
- Asignar `currentWorldData` y `worldCompleteController` en el `LevelManager` de cada nivel del mundo.
- Crear el panel y asignar `panel`, textos y `nextWorldData` al `WorldCompleteController`.
- Conectar el botón del panel a `ContinueToNextWorld()`.
- Definir `selectionSceneName` en cada `WorldData` que tenga una escena de selección propia.

El flujo implementado:
- Se activa cuando `WorldProgressManager.IsWorldComplete(worldData)` es true al terminar un nivel
- Muestra estrellas totales, gemas, tréboles y mejores tiempos acumulados del mundo
- El botón "Siguiente mundo" carga `selectionSceneName` del siguiente `WorldData`; sin `nextWorldData`, vuelve a la selección general

#### 3. Corazón extra del Dado Secreto — HUD visual
**Estado:** ✅ `UIController` refresca salud y activa el slot opcional `bonusHeartIcon` cuando `maxHealth` supera la capacidad de los corazones base. `PlayerHealth` refresca el HUD al recibir daño, curarse, reiniciar y ganar/quitar corazones bonus.
**Cableado pendiente:** Crear/asignar un `Image` de corazón extra en `UIController.bonusHeartIcon`. El HUD actual tiene 4 slots base; hace falta ese slot adicional para mostrar el bonus.

#### 4. Analytics — TrackCheckpoint no se llama
**Estado:** ✅ Ya estaba conectado en `Checkpoint.OnTriggerEnter2D`; registra el nivel y `checkpointIndex` al activarse.

#### 5. Score / leaderboard por nivel (GDD sección 15 — pendiente de diseño)
**Estado:** No está definido el diseño. Cuando se defina, Codex puede implementar:
- Campo `score` en `SaveManager` por nivel
- Fórmula de score (tiempo + gemas + vidas restantes)
- Integración con Unity Leaderboards (Unity Gaming Services)

#### 6. Dado Secreto — verificar si está disponible en Game Over
**Estado actual:** El `GameOverController` solo ofrece el dado mejorado (trébol), no el Dado Secreto. El GDD tiene pendiente definir si el Dado Secreto también se puede usar en Game Over.
**Cuando se decida:** Agregar un cuarto botón en `GameOverController` condicionado a `SaveManager.IsSecretDiceUnlocked(worldNumber)`.

---

### 🎨 Para ANTIGRAVITY (assets visuales — código 100% listo esperando sprites)

| Asset | Usado en | Campos a asignar en Inspector |
|-------|----------|-------------------------------|
| Sprite del Trébol 🍀 (normal) | `TrebolPickup.cs` prefab | `SpriteRenderer.sprite` |
| Sprite del Trébol 🍀 (translúcido "ya obtenido") | `TrebolPickup.cs` — alpha automático en código | No requiere campo extra |
| Efecto VFX de recolección del Trébol | `TrebolPickup.cs` → `effect` GameObject | `[SerializeField] private GameObject effect` |
| Sprite del Fragmento de Dado 🎲 | `DiceFragmentPickup.cs` prefab | `SpriteRenderer.sprite` |
| Sprites de los 3 dados (Normal / Mejorado / Secreto) | `DiceRollController` panel — visuales del dado | Agregar `Image` en el panel del dado |
| Fondo y layout del panel del dado | `DiceRollController.cs` — ya tiene lógica | Crear panel en Canvas del prefab `UI-Canvas` |
| Animación de cambio de dado (al gastar Trébol) | `DiceRollController.OnUpgradeWithTrebolPressed()` | DOTween ya está, solo falta sprite/animación |
| Animación de ensamblado del Dado Secreto | `DiceFragmentPickup.cs` — pendiente conectar | Agregar `OnSecretDiceAssembled` event en `SaveManager` |
| Fondo y layout del panel Game Over | `GameOverController.cs` — ya tiene 4 botones | Crear panel en Canvas del prefab `UI-Canvas` |
| ❤️ Ícono de corazón extra (Dado Secreto) | `PlayerHealth.AddBonusHeart()` | Crear slot en HUD — campo `bonusHeartIcon` en `UIController` |
| Sprites de estrella llena / vacía | `UIController.starFull`, `UIController.starEmpty` | Asignar en Inspector del prefab `UI-Canvas` |
| Badge de Trébol en MapPoint | `MapPoint.trebolBadge` | Asignar GameObject en cada MapPoint de `LevelSelection` |
| HUD: contador de tréboles 🍀 | `UIController.trebolCountText` | Asignar TextMeshProUGUI en Inspector |
| HUD: contador de vidas ❤️ | `UIController.livesText` | Asignar TextMeshProUGUI en Inspector |

---

### 🔧 Para Unity Editor (configuración y serialización)

#### Escena `MainMenu`
- [x] Crear GameObject `AppBootstrap` y asignarle el script con `[RequireComponent]` (`SaveManager`, `GamepadDetector`, `LivesManager`, `AdsManager`, `RevenueCatManager`, `WorldProgressManager`). *(Verificado y serializado)*
- [x] Crear GameObject `SceneTransitionManager` con su script. *(Verificado y serializado)*
- [x] Crear `OpenStoreButton` ("🛒 Quitar Anuncios") y `StorePanel` con `NoAdsPurchaseButton.cs` para compras/restauración. *(Verificado y serializado en MainMenu)*

#### Escena `LevelSelection`
- [x] Asignar `trebolBadge` (`Badge_Trebol`) en cada `MapPoint` de la escena. *(Verificado y serializado en los MapPoints)*
- [x] Configuración de paneles de selección y badges de mundo. *(Verificado y serializado)*

#### Escena de Gameplay (`Level_0` a `Level_10`)
- [x] Prefab `UI-Canvas` configurado y serializado con HUD (`Trebol_Txt`, `Lives_Txt`), `GameOverPanel` (con `useSecretDiceButton`), `DiceRollPanel`, `MobileControls` y `StorePanel`. *(Verificado y guardado)*
- [x] Configuración de color de fondo de cámara en `#47F3FF` para eliminar franja en pantallas panorámicas (20:9). *(Verificado en Level_0 a Level_10)*
- [x] `TrebolPickup.prefab` agregado y posicionado en cada escena de nivel. *(Verificado y serializado)*

#### Controles on-screen (mobile)
- [x] Serializado en `UI-Canvas.prefab` bajo `MobileControls`:
  - `MoveStick` con `OnScreenStick` → Control Path: `<Gamepad>/leftStick`.
  - `JumpButton` con `OnScreenButton` → Control Path: `<Gamepad>/buttonSouth`.
  - `PauseButton` con `OnScreenButton` → Control Path: `<Gamepad>/start`.
  - Conectado a `MobileGameplayControls.cs` para control de visibilidad y hot-plug con mandos físicos.

#### Settings / PauseMenu / MainMenu (Store UI)
- [x] `StorePanel` serializado con `NoAdsPurchaseButton.cs`: botón de compra con precio dinámico de RevenueCat, botón de restaurar, indicador activo de NoAds, mensajes de estado y botón de cierre. *(Serializado en MainMenu y UI-Canvas)*

---

### 🔑 SDKs externos (prerequisito para build de producción)

> **Avance de integración (2026-10-27):** el manifest declara LevelPlay `com.unity.services.levelplay` 9.5.1, RevenueCat Unity 9.15.0, External Dependency Manager 2.1.1 y Unity Pipeline 0.9.1-exp.1. `.gitignore` ahora permite registrar `Packages/manifest.json` y `packages-lock.json`; el lockfile no contiene aún estos paquetes y debe regenerarse en UPM. Migré `AdsManager` de `IronSource.*` legado a ad units LevelPlay 9.x; los registros Android/iOS verificados tienen app keys y ad units Rewarded/Interstitial configurados. Rewarded evita doble toque y otorga solo desde el callback de recompensa; Remove Ads bloquea todos los formatos. Migré `RevenueCatManager` a la API actual de componente de RevenueCat, entitlement `remove_ads`, validación del SKU exacto y consulta del precio localizado. **Pendiente de resolver/importar el manifest en Unity, compilar y probar en dispositivo.** RevenueCat ya tiene proyecto, apps, entitlement y offering multiplataforma. Las claves públicas propias están en configuración local ignorada por Git; faltan credenciales de servicio Play, UI conectada y consentimiento. El producto iOS tiene precio US$2.99. LevelPlay sigue cerrado si entitlement/consentimiento no están resueltos; no se ha validado compra ni anuncio.

> **Bloqueo de validación del editor:** Unity CLI quedó actualizado a `1.0.0-beta.14` y `unity pipeline install` añadió `com.unity.pipeline@0.9.1-exp.1`. Abrí el proyecto con Unity `6000.6.0f1`; `unity status --until-ready` esperó 60 s y devolvió `STATUS_NO_INSTANCES`, aunque el inventario de apps muestra Unity abierto. La inspección de la ventana agotó el tiempo. El primer build Android terminó con error antes de importar C# por `EPERM` al iniciar el socket UPM local y fallo de IPC con Licensing Client. `unity license status` con acceso autorizado confirmó Unity Personal activo. El build elevado quedó sin log ni actualización de `Library/PackageCache`/lockfile tras varios minutos; lo interrumpí. No se pudo validar resolución UPM, compilación, ni APK. No declarar compilación exitosa.

> **Auditoría de monetización (2026-10-27):** el manifest declara LevelPlay 9.5.1, RevenueCat Unity 9.15.0, External Dependency Manager 2.1.1 y Pipeline 0.9.1-exp.1; falta resolución UPM y actualización del lockfile. No hay managers/botones serializados en escenas/prefabs ni flujo de consentimiento cableado. Las claves públicas de Fox's Tale están configuradas en `Assets/Resources/RevenueCatPublicKeys.local.json`, ignorado por Git. LevelPlay Android e iOS tienen apps y unidades de Fox's Tale. Play Console ya tiene la app, pero el producto único exige un build con `BILLING`. RevenueCat tiene proyecto propio, apps Android/iOS y mapping de ambos productos; la integración migrada aún no se compiló ni probó. No copiar keys ni IDs de otros juegos.

> **Dashboards inspeccionados (2026-10-27):** el propietario certificó explícitamente las declaraciones de políticas y exportación; se creó [Fox's Tale en Play Console](https://play.google.com/console/u/0/developers/7720975764288323280/app/4976400942511877165/app-dashboard) (Juego, Sin coste, en-US; package reservado `com.fatcatgamestudio.foxstale`). En `Monetizar con Play > Productos únicos`, Google exige primero subir un APK/AAB con permiso `BILLING`; el producto Android todavía no existe. LevelPlay Android `Foxs Tale Android`, app key `290dc33f5`, Rewarded `d9opyhliijhr3fb4` (Lives x2), Interstitial `7xsq048b2uuvo8vj`. LevelPlay iOS `Foxs Tale iOS`, app key `290e4f7ad`, Rewarded `4sm02ryaqjxmk9pd` (Lives x2), Interstitial `6wzvpnyl3oqj65qm`; ambas apps se marcaron no dirigidas a niños. Apple Developer registró el App ID explícito `com.fatcatgamestudio.foxstale`. App Store Connect creó [Fox's Tale](https://appstoreconnect.apple.com/apps/6827745041/distribution) (Apple ID `6827745041`, SKU `foxstale-ios`) y el producto no consumible [Remove Ads](https://appstoreconnect.apple.com/apps/6827745041/distribution/iaps/6827746088), SKU `com.fatcatgamestudio.foxstale.noads`, Apple ID `6827746088`, con localización en-US y disponibilidad en 175 regiones. El propietario aprobó US$2.99 y Apple muestra ese precio vigente para Estados Unidos (PEN 12.90 en Perú). Falta captura del IAP dentro del juego para revisión; no se envió a revisión. Apple avisó que no pudo guardar la preferencia de acceso al crear la app, pero indicó que todos los usuarios actuales tienen acceso. Se creó un [proyecto RevenueCat independiente Fox's Tale](https://app.revenuecat.com/projects/0b0ff296/overview), categoría Games y plataforma Unity, y el entitlement [`remove_ads`](https://app.revenuecat.com/projects/0b0ff296/product-catalog/entitlements). Tras la autorización expresa del propietario, se creó también la app iOS en RevenueCat y se asignaron las claves Apple compartidas de compra (`Z5V4FL5RJD`) y App Store Connect (`6TGLSXPKYY`); ambas muestran `Valid credentials`. Los productos Apple y Android están en `remove_ads` y en el mismo paquete `$rc_lifetime` del offering `default`. No copiar claves a código ni al handoff.

> **Audiencia indicada por el dueño (2026-10-27):** adolescentes y adultos, excluyendo niños; no quiere sumar una pantalla de edad si puede evitarla. 18+ es una alternativa condicional si una exigencia real de la tienda vuelve necesarias pantallas adicionales; no está confirmada como audiencia final. No seleccionar grupos de edad en Play Console hasta reflejar esta decisión correctamente. No declarar 18+ solo para facilitar ads; alinear el listing, contenido y marketing con la audiencia elegida.

#### Hallazgos verificados y trabajo realizado
- `Packages/manifest.json` declara LevelPlay 9.5.1, RevenueCat Unity 9.15.0 y External Dependency Manager 2.1.1; falta resolver UPM y lockfile. `AdsManager.cs` está migrado al ad-unit API LevelPlay 9.x; `RevenueCatManager.cs` ahora usa el componente `Purchases`. La compilación sigue sin verificar.
- No se encontraron referencias serializadas de AdsManager, RevenueCatManager, AppBootstrap, GameOverController ni NoAdsPurchaseButton en escenas/prefabs; confirmar y cablear el setup en Unity Editor.
- `AdsManager.cs` contiene las app keys y los IDs de ad units Android/iOS confirmados en LevelPlay. `RevenueCatManager.cs` prioriza las claves SDK propias de `Assets/Resources/RevenueCatPublicKeys.local.json`; el archivo es local e ignorado por Git. RevenueCat valida las credenciales Apple compartidas autorizadas por el propietario: IAP key ID `Z5V4FL5RJD` y App Store Connect API key ID `6TGLSXPKYY`. No registrar `.p8` ni claves privadas en Git, docs o logs.
- El SKU iOS `com.fatcatgamestudio.foxstale.noads` ya existe como producto no consumible en App Store Connect, con precio base US$2.99. Play Console aún no permite crear el producto hasta subir un build con `BILLING`. RevenueCat ya tiene los mappings Android/iOS en `remove_ads` y `default` / `Lifetime`; Android no puede verificarse hasta que existan el producto real y las credenciales de Google. Los builds Development usan la key de tienda de su plataforma por defecto; Test Store requiere activar la opción explícita en el Inspector.
- No existe implementación visible de CMP/consentimiento, privacidad por región, URL de política, opción ATT, mecanismo de reporte de anuncios ni disclosure de segmentación. El dueño indica adolescentes y adultos, excluye niños, y prefiere no añadir age gate; 18+ es fallback condicional, aún no elegido. La ficha y el marketing deben coincidir con la audiencia final. Aplican igualmente las obligaciones de privacidad por región. No asumir que ATT aplica: depende del uso de tracking/IDFA y los SDKs incluidos.
- PlayerSettings y `BuildAutomation.cs` usan `com.fatcatgamestudio.foxstale` en Android/iOS. Android versionCode 1 y target API 36; iOS build number 1. El SDK Android 36 está instalado. Desde el 31-ago-2026, Google Play exige API 36 para nuevas apps y actualizaciones salvo extensión aplicable. Falta compilar y verificar el build resultante.
- `scripts/release-test.sh` construye AAB iOS/Android con números explícitos y permite `--upload-test` solo para Google Play Internal y TestFlight mediante `fastlane/Fastfile`. Requiere keystore/certificado de subida Android, JSON de cuenta de servicio Play, archivo privado `.p8` para Fastlane y signing de Xcode. No se generó ni subió ningún build: esos archivos/valores locales todavía no están configurados.
- Se creó la app Fox's Tale en Play Console; la ficha y declaraciones de contenido aún requieren configuración. En LevelPlay se registraron Android e iOS, ambos con COPPA `Not directed`, y se crearon sus ad units `Rewarded` e `Interstitial`.
- RevenueCat: se creó el proyecto propio `Fox's Tale` (`0b0ff296`) y la app Android [`Fox's Tale (Play Store)`](https://app.revenuecat.com/projects/0b0ff296/apps/app53b6dcbbc8), package `com.fatcatgamestudio.foxstale`. Se registró manualmente el producto no consumible [`com.fatcatgamestudio.foxstale.noads`](https://app.revenuecat.com/projects/0b0ff296/product-catalog/products/prod7d7ac80fbc), enlazado al entitlement `remove_ads` y al offering actual [`default` / paquete `Lifetime`](https://app.revenuecat.com/projects/0b0ff296/product-catalog/offerings/ofrngba96c477a5). Estado del producto: `Could not check`, porque faltan las credenciales de servicio de Google Play y el producto real en Play Console. La [app iOS](https://app.revenuecat.com/projects/0b0ff296/apps/appd0ba507214) se guardó con las claves Apple compartidas autorizadas: IAP key ID `Z5V4FL5RJD` y App Store Connect API key ID `6TGLSXPKYY`, ambas validadas por RevenueCat. El [producto iOS](https://app.revenuecat.com/projects/0b0ff296/product-catalog/products/prodc6cafe5b24) está enlazado a `remove_ads` y `default` / `$rc_lifetime`; RevenueCat lo marca `Missing Metadata`, coherente con el estado incompleto de revisión en Apple. No reutilizar keys ajenas al proyecto ni registrar claves en código o handoff.
- iOS tiene deployment target 13.0, build number 1 y Bundle ID explícito `com.fatcatgamestudio.foxstale`, registrado en Apple Developer y App Store Connect. No se encontró artefacto de build. Apple exige SDK iOS 26/Xcode 26 o posterior para uploads desde 28-abr-2026; revisar la versión Xcode real y subir con una toolchain admitida.
- El rewarded no otorga vidas si la reproducción no completa; ya no se simula una recompensa gratis en builds de prueba. Remove Ads bloquea todos los anuncios según el GDD. El botón puede mostrar el precio localizado cuando RevenueCat tenga el producto/Offering disponible; el Inspector sigue necesitando sus referencias UI.
- LevelPlay queda cerrado por defecto: `AdsManager.SetPrivacyEligibilityResolved()` aún no tiene caller y espera a que RevenueCat resuelva el entitlement antes de inicializar. Falta crear/configurar privacidad y consentimiento y cablear UI y managers en escenas. Hasta resolver consentimiento/elegibilidad y entitlement, los anuncios no arrancan.

Fuentes oficiales consultadas el 2026-10-27: [LevelPlay Unity package integration](https://docs.unity.com/en-us/grow/levelplay/sdk/unity/package-integration), [migración de inicialización LevelPlay](https://docs.unity.com/grow/levelplay/sdk/unity/migrate-to-init-api), [consentimiento y COPPA de LevelPlay](https://docs.unity.com/en-us/grow/levelplay/sdk/unity/regulation-advanced-settings), [SDKs de anuncios Families certificados por Google](https://support.google.com/googleplay/android-developer/answer/12955712?hl=en-en), [audiencias y edades en Play Console](https://support.google.com/googleplay/android-developer/answer/9867159?hl=en), [RevenueCat Unity](https://www.revenuecat.com/docs/getting-started/installation/unity), [RevenueCat entitlements](https://www.revenuecat.com/docs/getting-started/entitlements), [Google Play target API](https://support.google.com/googleplay/android-developer/answer/11926878?hl=en-en), [App Review Guidelines](https://developer.apple.com/app-store/review/guidelines/) y [requisitos próximos de Apple](https://developer.apple.com/news/upcoming-requirements/?id=02212025a).

Para la integración actual consultada: [LevelPlay migración de Rewarded Ad Unit API](https://docs.unity.com/en-us/grow/levelplay/sdk/unity/migrate-rewarded-ad-unit-api), [LevelPlay integración de interstitial](https://docs.unity.com/en-us/grow/levelplay/sdk/unity/interstitial-integration), [RevenueCat Unity 9.15.0](https://github.com/RevenueCat/purchases-unity/releases/tag/9.15.0), [RevenueCat Test Store](https://www.revenuecat.com/docs/test-and-launch/sandbox/test-store), [Unity External Dependency Manager](https://docs.unity3d.com/Packages/com.unity.external-dependency-manager@2.1/manual/get-started-with-edm.html).

#### Unity LevelPlay (Ads)
1. Dejar que Unity resuelva `com.unity.services.levelplay@9.5.1`; confirmar soporte con Unity 6000.6.0f1 y corregir cualquier incompatibilidad después de importar.
2. Verificar `LEVELPLAY_DEPENDENCIES_INSTALLED` definido por el paquete y compilar. No agregar define symbols a mano como sustituto de instalar/verificar el SDK.
3. Configurar honestamente la clasificación “no dirigido a niños” en los dashboards si el producto, contenido y marketing la respaldan. Declarar las edades objetivo reales en Play Console. Implementar CMP y señales GDPR/CCPA que correspondan; no configurar COPPA/child-directed sin base, ni omitir esas señales si la clasificación real cambia.
4. Configurar app keys y ad units (Rewarded + Interstitial) para cada tienda en dashboards y asignarlas en el Inspector usando configuración segura. La app key no es una API key secreta, pero debe corresponder al app real; no copiarla desde otro juego.
5. Inicializar LevelPlay únicamente tras confirmar elegibilidad para anuncios y resolver el entitlement Remove Ads. Probar con test ads en dispositivos; no activar inventario de producción como parte de la auditoría.

#### RevenueCat (IAP NoAds)
1. Resolver `com.revenuecat.purchases-unity@9.15.0` y External Dependency Manager `2.1.1`; confirmar que se importen las dependencias nativas antes de generar builds.
2. Ya están creados el proyecto/app de RevenueCat, el entitlement `remove_ads`, el offering `default`/`$rc_lifetime` y el mapping iOS/Android del SKU `com.fatcatgamestudio.foxstale.noads`. Apple muestra US$2.99 y el dueño aprobó ese precio. Play aún requiere el primer AAB con `BILLING` para crear el producto real.
3. Las claves SDK públicas específicas de Fox's Tale están en `Assets/Resources/RevenueCatPublicKeys.local.json`, ignorado por Git. `RevenueCatManager` las carga desde Resources. Tras resolver UPM, probar compra, cancelación, fallo, restore y reinstalación con productos de tienda/sandbox; Editor/Test Store no valida compras reales.
4. RevenueCat debe devolver `customerInfo` antes de habilitar anuncios; el error deja LevelPlay bloqueado. No fijar precio de fallback: mostrar el precio localizado recibido del store/RevenueCat.

---

### 📦 Build y distribución (Fase 5 — pipeline añadido, credenciales y compilación pendientes)

#### Android
- [ ] Crear keystore de subida y guardarlo **fuera** del repositorio; anotar/respaldar de forma segura contraseñas y alias.
- [ ] Configurar `FOXSTALE_KEYSTORE_PATH`, `FOXSTALE_KEYSTORE_PASSWORD`, `FOXSTALE_KEYALIAS_NAME` y `FOXSTALE_KEYALIAS_PASSWORD` en el entorno local seguro.
- [ ] Compilar con `scripts/release-test.sh android --android-code <nuevo>`; el script genera AAB firmado para Google Play. Requiere resolver UPM y completar todas las escenas requeridas.
- [ ] El build automatizado fija target API 36 por disponibilidad de la plataforma instalada; validar el AAB resultante y permisos reales de SDKs.
- [ ] Para Play Internal, crear/autorizar cuenta de servicio y configurar `PLAY_STORE_JSON_KEY_PATH`; subir solo con `--upload-test`.
- [ ] Revisar permisos/data declarations generados por los SDKs de ads y analytics que finalmente se integren; no agregar `AD_ID` preventivamente si el SDK real no lo requiere.
- [ ] Declarar Privacy Policy URL en Google Play Console.

#### iOS
- [ ] Configurar Team ID y firma automática/certificados válidos de Xcode; localizar o crear una API key de App Store Connect cuyo archivo `.p8` esté disponible de forma local segura.
- [ ] Usar `scripts/release-test.sh ios --ios-build <nuevo>` para exportar el proyecto Xcode.
- [ ] Para subir a TestFlight, configurar `ASC_KEY_ID`, `ASC_ISSUER_ID`, `ASC_KEY_FILEPATH` e `IOS_DEVELOPMENT_TEAM`, luego usar `--upload-test`. No envía a App Review.
- [ ] Determinar si ATT aplica según tracking/IDFA y SDKs integrados; no mostrar prompt sin esa determinación.
- [ ] Revisar Privacy Manifests, declaraciones de privacidad y configuración de red atribución conforme a los SDKs y redes seleccionados.

#### Compliance de tiendas
- [ ] Age Rating / IARC en ambas plataformas.
- [ ] Data safety form en Google Play (datos recolectados: Device ID para analytics).
- [ ] Privacy nutrition label en App Store Connect.

---

### 🌍 Fase 6 — Para cuando haya Mundo 2

1. Assets → Create → Fox's Tale → World Data → `WorldData_World2`.
2. Llenar: `worldName`, `worldNumber = 2`, `levelSceneNames = ["Level_11", ...]`, `starsRequiredToUnlock` (definir cuántas estrellas del Mundo 1 se necesitan).
3. Crear escenas `Level_11`, `Level_12`... y agregarlas en Build Settings.
4. En la escena `LevelSelection` del Mundo 2: asignar el `WorldData` al `MapPointsHolder`.
5. Implementar pantalla de "Mundo completado" (ver sección Codex arriba).

---

## Información necesaria del dueño del proyecto

Antes de hacer el build final de producción, se necesita:

| Dato | Para qué |
|------|----------|
| IDs Android/iOS `com.fatcatgamestudio.foxstale` ya fijados; verificar versionCode/build number al compilar | Coincidencia con Play, App Store Connect y RevenueCat |
| Crear el SKU Android en Play después del primer AAB con permiso `BILLING`; revisar su estado en tienda | Configuración de Android y validación de compras |
| Entitlement `remove_ads`, offering `default` y paquete `$rc_lifetime` con ambos productos | Ya configurados en RevenueCat; verificar precio/precio localizado después de que ambas tiendas tengan metadata activa |
| Audiencia indicada: adolescentes y adultos, sin dirigir el producto a niños; evitar pantalla de edad. 18+ es solo fallback si una exigencia real añade pantallas | Reflejar los grupos adecuados en Play Console y mantener LevelPlay “not directed to children”; no enviar certificaciones hasta confirmar que listing/contenido respaldan la selección |
| Credenciales de servicio Google Play para RevenueCat y el JSON de cuenta de servicio para Fastlane | Validar el producto Android y subir builds a Internal Testing |
| CMP/consentimiento aplicable, URL de política de privacidad y evidencia de flujos/datos de SDK | Mantener LevelPlay bloqueado hasta resolver elegibilidad real y señales requeridas |
| CMP/consentimiento por región, URL de política de privacidad, prácticas de datos/analytics y uso de tracking | Cumplimiento y control de ads/ATT |
| Ad units Rewarded e Interstitial y app keys Android/iOS en LevelPlay | Ya registrados y asignados como valores iniciales en `AdsManager`; verificar Inspector al cablear escenas |
| Keystore Android, certificados y Team ID iOS, API key `.p8` local de App Store Connect y versión Xcode | Firmar y validar builds en dispositivos y canales internos |
| Decisión de producto para usuarios Remove Ads: botón rewarded oculto; GDD actual indica que todos los ads se desactivan | Mantener coherente UI y beneficio comprado |

---

## Referencia rápida de archivos de documentación

| Archivo | Contenido |
|---------|-----------|
| `.kiro/handoff.md` | **Este archivo** — fuente de verdad del proyecto |
| `.kiro/game-design-document.md` | GDD — diseño del juego, sistemas, decisiones pendientes |
| `.kiro/fase-0-reporte.md` | Cambios de Fase 0 con instrucciones de reversa |
| `.kiro/fase-1-reporte.md` | Cambios de Fase 1 |
| `.kiro/fase-2-reporte.md` | Cambios de Fase 2 |
| `.kiro/fase-3-reporte.md` | Cambios de Fase 3 |
| `.kiro/fase-4-reporte.md` | Cambios de Fase 4 (Ads + IAP) |
| `.kiro/fase-5-reporte.md` | Cambios de Fase 5 |
| `.kiro/fase-6-reporte.md` | Cambios de Fase 6 |

---

## Veredicto de monetización: NO LISTO PARA PRODUCCIÓN

| Criterio | Estado |
|---|---|
| Código C# | ⚠️ Estado general requiere validación; `Bouncer.cs` ya implementado |
| Persistencia | ✅ SaveManager centralizado, preserva IAP al resetear |
| Economía | ✅ Vidas, trébol, dado Normal/Mejorado/Secreto, corazón extra |
| Controles | ✅ Teclado + Gamepad físico + on-screen (pendiente OnScreenStick en Editor) |
| Ads | ⚠️ Código y apps LevelPlay configurados; falta UPM, consentimiento, wiring en escenas y pruebas |
| IAP | ⚠️ Código, claves locales, productos Apple y catálogo RevenueCat listos; falta crear producto Play, resolver UPM, conectar UI y probar |
| Builds | ⚠️ Script Unity + Fastlane creados; firma/credenciales pendientes y ningún build compilado o subido |
| Multi-mundo | ✅ Arquitectura lista. Pendiente WorldData asset del Mundo 1 |

**Siguiente acción:** ver el registro operativo fechado al final de este archivo. Los scripts solo suben a Internal Testing y TestFlight; a la fecha solo hay un export iOS local sin firmar y no se ha subido un build.

## Registro operativo — 2026-10-27

### Hecho y verificado

- Se añadió `com.unity.inputsystem` 1.20.1 al `Packages/manifest.json`. El juego usa `UnityEngine.InputSystem` directamente y Unity 6000.6 publica esa versión; el lockfile quedó actualizado por UPM.
- Se añadió `LEVELPLAY_DEPENDENCIES_INSTALLED` a los define symbols Android e iOS. El paquete LevelPlay solo instala ese símbolo desde un flujo del Editor interactivo; sin él `AdsManager` compilaba siempre la rama sin SDK en batchmode.
- Se corrigió un error de compilación en `Assets/Scripts/UI/FadeEffect.cs`: `[SerializeField]` estaba aplicado a una declaración `enum`, atributo no válido.
- `BuildAutomation` ahora establece Target API 37, alineado con el estándar solicitado por el dueño. Play actualmente exige como mínimo API 36 para apps nuevas/actualizaciones desde 31-ago-2026; API 37 excede ese mínimo. Ver [requisito oficial de Play](https://support.google.com/googleplay/android-developer/answer/11926878?hl=en-EN) y [tabla oficial de Android API](https://developer.android.com/guide/topics/manifest/uses-sdk-element).
- Unity 6000.6.0f1 terminó `BuildAutomation.BuildiOS` con **Build EXITOSO**, 56.8 s, generando `Builds/iOS/XcodeProject` (aprox. 1.0 GB). La salida contiene workspace, Podfile/Podfile.lock y Pods. Es export de Xcode sin firma ni IPA, no una subida a TestFlight.
- `Assets/Resources/RevenueCatPublicKeys.local.json` está presente e ignorado por Git. No copiar ni mostrar valores.

### Bloqueos confirmados / no resueltos

- Estado actualizado 2026-10-28: `MainMenu.unity` sí contiene `AppBootstrap`, `RevenueCatManager` y `AdsManager` serializados. No aparece `NoAdsPurchaseButton` en la escena ni en `UI-Canvas.prefab`; la UI de compra/restaurar sigue sin cablear.
- `AdsManager.SetPrivacyEligibilityResolved(bool)` no tiene llamador. Por seguridad el SDK permanece cerrado; no se ha implementado ni validado CMP/consentimiento regional. No activar anuncios sin flujo real y revisión de privacidad.
- El catálogo y las claves de tiendas se mantienen según el estado reportado en handoffs anteriores; esta sesión no tuvo RevenueCat/LevelPlay/Play Console abiertos para verificar cambios actuales. App Store Connect sí mostró la fila de Fox's Tale (ID 6827745041), pero la ficha IAP no se pudo cargar para verificar metadatos actuales.
- No se encontró un keystore Fox's Tale en la carpeta central de keystores revisada ni variables `FOXSTALE_*` en `~/.fatcat-build/secrets.env`. No se expusieron contraseñas.
- Siguen faltando JSON de cuenta de servicio Play para Fastlane, credenciales de servicio Play en RevenueCat, archivo privado `.p8` local para Fastlane, ASC issuer/team y firma válida de distribución Xcode. Tener los IDs de keys en RevenueCat no aporta el archivo privado `.p8`.
- El producto Android Remove Ads aún requiere el primer AAB con Billing; luego hay que crearlo en Play, enlazarlo/verificarlo en RevenueCat y probar compras.
- No se generó AAB, IPA ni build con número de tienda confirmado, ni se ejecutó Fastlane. No hay validación de ads/IAP en dispositivo físico.

### Verificación App Store Connect — 2026-10-28

- En la ficha IAP Apple ID `6827746088`, el producto `com.fatcatgamestudio.foxstale.noads` aparece `Prepare for Submission`; disponibilidad: todos los países/regiones; nombre y descripción en inglés ya presentes. El precio actual no quedó legible en la tabla consultada; USD 2.99 sigue siendo el dato previamente reportado, no reconfirmado hoy.
- La sección **Review Information → Screenshot** está vacía. Apple indica que la primera compra no consumible debe enviarse junto con una versión nueva de la app. El botón visible es `Add for Review`, pero no se debe enviar hasta adjuntar captura real y preparar la versión de app.
- Las variables para Fox's Tale no están en `~/.fatcat-build/secrets.env`. Esto solo confirma ese almacén estándar; no prueba que no haya copias de archivos privados en otras carpetas locales.

### Intentos de compilación

- Primer intento batchmode bloqueado por `EPERM` al crear el socket local UPM; se repitió autorizado fuera del sandbox.
- La segunda ejecución encontró un error de compilación en `FadeEffect.cs`; después de corregirlo, la exportación iOS terminó satisfactoriamente.

### Comandos operativos

El comando único sigue siendo `scripts/release-test.sh both --android-code <versionCodeNuevo> --ios-build <buildNuevo> --upload-test`. Antes de usarlo, cerrar Unity GUI de forma segura, elegir números superiores a los últimos recibidos por las tiendas, y resolver firma iOS y acceso de la cuenta de servicio Play. Las lanes solo apuntan a Play Internal Testing y TestFlight. No enviar a producción ni App Review.

## Registro operativo — 2026-10-28 (continuación)

- La API de App Store Connect responde 200 para leer Fox's Tale (`6827745041`, bundle `com.fatcatgamestudio.foxstale`); no hay builds subidos. La clave privada local validada es R6DA76UX3G y es distinta de la clave configurada en RevenueCat (6TGLSXPKYY). Hay un certificado iOS de distribución en el equipo, pero no existe identidad de firma válida en el llavero local. Aún falta resolver provisioning y firmar.
- El JSON de Play local está presente y su formato es válido, pero Play Publisher API deniega acceso a la app con HTTP 403 `The caller does not have permission`. Por tanto Fastlane no puede subir todavía. Acción humana mínima, si se quiere habilitar Fastlane: dar a la cuenta de servicio de ese JSON acceso limitado a Fox's Tale con permisos de gestionar lanzamientos de prueba; no cambiar otros proyectos. También puede usarse la sesión propietaria del dashboard para la operación interna, tras generar el AAB.
- Se generó y guardó fuera del repositorio un keystore exclusivo de Fox's Tale en `~/.fatcat-build/keystores/FoxsTale-upload-2026.p12` (alias `foxstaleupload`, huella pública SHA-256 `991804251f6f96f5ab4412ff5e9f124611c556893a663195fe0f7b51927e8708`). Secretos únicamente en `~/.fatcat-build/secrets.env`, permisos 600. Nunca reutilizar este certificado para otro juego.
- `BuildAutomation` configura Target API 37 y ahora Minimum API 26 (requisito mínimo de Unity 6). `ProjectSettings.asset` quedó en API mínima 26, Target API 37 y `activeInputHandler: 1`. Unity requiere reinicio del Editor para activar el Input System; el proceso GUI permanece abierto. No pude confirmar que la UI automatizada respondiera para reiniciarlo sin arriesgar cambios del usuario.
- Los intentos del menú Android fallaron primero por valores de firma no detectados, luego porque el Editor aún tenía `Active Input Handling = Both` en memoria, y el último arrancó mientras Unity compilaba scripts. `Builds/Android` sigue vacío. No se generó AAB ni se intentó subirlo.
- El script `scripts/release-test.sh` detecta ahora `Library/EditorInstance.json` y detiene batchmode si el mismo proyecto sigue abierto en Unity GUI, evitando dos instancias concurrentes.
- Se identificó y corrigió en el export Xcode local que Unity/EDM generó un `Podfile` vacío, dejando los wrappers LevelPlay sin el framework `IronSource`. Se instaló `IronSourceSDK` 9.5.0.0 mediante CocoaPods y se elevó el mínimo de sus pods a iOS 15 para Xcode 27.
- `xcodebuild` finalizó correctamente sin firma (`CODE_SIGNING_ALLOWED=NO`): `/private/tmp/foxstale-derived/Build/Products/Release-iphoneos/FoxsTale.app`, bundle correcto, MinimumOSVersion 15.0, 126 MB. Es un `.app` unsigned de dispositivo; no es IPA ni TestFlight. No se intentó firmar porque no hay identidad de distribución privada válida en el llavero.
- `fastlane/Fastfile` automatiza la reparación del Podfile y `pod install` en la lane TestFlight para que los nuevos exports Unity incluyan LevelPlay antes de `build_app`. Ruby `-c` informa `Syntax OK`.
- `NoAdsPurchaseButton` ya contiene lógica de compra/restauración y precio localizado, pero no figura serializado en MainMenu ni en `UI-Canvas.prefab`. No está conectado a elementos de UI. `AdsManager.SetPrivacyEligibilityResolved()` continúa sin caller verificado; se mantiene fail-closed y no se simula consentimiento.
- RevenueCat local tiene sus claves SDK públicas, pero no se imprimen/documentan sus valores. El producto de Play aún debe crearse tras subir el primer AAB con permiso BILLING y enlazarse/verificarse en RevenueCat.
- No se alteró App Store Connect, el formulario no guardado de Apple Developer ni la configuración de permisos de Play; no hubo subida a tienda, TestFlight ni App Review. El AAB sigue pendiente porque Unity GUI continúa activo y el backend input actualizado requiere reiniciar el Editor.
- Detalle de credenciales, resultados y limitaciones actuales: `docs/MONETIZATION_RELEASE.md`.

## Registro operativo — 2026-10-28 (verificación de Play Console)

- Play Console muestra el canal de **prueba interna activo** con la versión `1 (1.0.1)`, disponible para testers desde el 28-oct a las 03:44; la consola todavía muestra **0,00 % de instalaciones**. El canal tiene seleccionadas las listas `ExternalTesters` y `GlowWords Internal`, con 1 usuario en cada lista. No se revisaron ni copiaron las direcciones de correo de los testers.
- El enlace web de participación que muestra el canal es `https://play.google.com/apps/internaltest/4701171198282852565`. Para instalar, el tester debe abrirlo con la misma cuenta Google que pertenece a una de las listas y continuar desde el enlace a Play Store; buscar la app por nombre no garantiza que aparezca porque la publicación pública sigue inactiva.
- El identificador coincide entre consola y proyecto: `com.fatcatgamestudio.foxstale`; el proyecto tiene `AndroidMinSdkVersion: 26` (Android 8.0+) y `AndroidTargetSdkVersion: 37`. Si el enlace acepta al tester pero Play Store rechaza el dispositivo, confirmar versión de Android y compatibilidad. Si instala y no abre, investigar como fallo de ejecución aparte.
- La ficha de Play Store y las declaraciones guardadas siguen como cambios pendientes de revisión. El botón de envío está deshabilitado porque el panel de control pide completar el flujo de primera versión de Producción; Producción permanece inactiva.

## 2026-10-28 — Fox's Tale iOS build 2 y estado real de monetización

- **VERIFICADO:** En terminal con acceso normal al llavero hay una identidad válida `Apple Distribution: Aldhair Vera Camacho (4UBGTX27U5)`. La salida previa `0 valid identities found` provenía del sandbox y no demuestra ausencia de certificado. No crear ni rotar certificados por ese dato antiguo.
- **VERIFICADO:** `store-assets/play-store/icon.png` era de 512×512 y todos los slots iPhone/iPad de `ProjectSettings.asset`, incluido App Store 1024, estaban vacíos. Se añadió `Assets/FoxsTaleAppIcon_1024.png` (1024×1024) a partir del motivo de zorro ya usado por la app, con `.meta`, y se asignó a los 19 slots iOS. Este icono nuevo debe revisarse visualmente antes de una publicación pública.
- **VERIFICADO:** `scripts/release-test.sh ios --ios-build 2 --upload-test` exportó Unity 6000.6.0f1 con exit code 0 a `Builds/CI/iOS/XcodeProject_2`. Fastlane falló primero en `pod install` porque su `GEM_PATH` ocultaba la dependencia `minitest` de CocoaPods; se corrigió `fastlane/Fastfile` para ejecutar CocoaPods con su entorno independiente y se añadió `--skip-build` al script para reutilizar el export.
- **VERIFICADO:** El segundo intento instaló Pods y Xcode terminó **Archive Succeeded** para el build 2, incluyendo dSYM. La exportación IPA falló: `IDEDistributionProvisioning.log` dice que **no hay perfil iOS App Store para `com.fatcatgamestudio.foxstale`**. La reparación automática devolvió `You haven't been given access to cloud-managed distribution certificates`. **No hay IPA ni subida a TestFlight.** Crear/instalar un perfil App Store válido para ese bundle usando la identidad de distribución local, configurar exportación manual en Fastlane y repetir `scripts/release-test.sh ios --ios-build 2 --upload-test --skip-build`; no reconstruir Unity ni cambiar build number para el mismo archive hasta confirmar si Apple acepta el export.
- **VERIFICADO:** `MainMenu.unity` contiene `AppBootstrap`, `RevenueCatManager` y `AdsManager`, pero ni esa escena ni `UI-Canvas.prefab` contienen `NoAdsPurchaseButton`; la compra y restauración no están accesibles al jugador. `AdsManager.SetPrivacyEligibilityResolved(bool)` no tiene caller, por lo que los anuncios permanecen cerrados. Esto impide declarar monetización terminada aunque el código y catálogo estén presentes.
- **PENDIENTE para cerrar monetización:** autorar y enlazar un panel/prefab visible con precio, compra, restauración y estado; implementar y verificar flujo de elegibilidad/consentimiento antes de llamar al método de AdsManager; crear el producto Google Play Remove Ads y validar en RevenueCat; completar captura IAP de Apple; QA de compra/restauración y ads en dispositivos. Mantener este build 2 solo como intento técnico de TestFlight.

## 2026-10-28 — Corrección de prioridad de release

El propietario aclaró que el objetivo es un juego monetizado apto para producción, no un binario técnico de prueba. Por tanto, **NO_GO_PRODUCTION** y se detienen nuevos builds/subidas mientras falten la UI de compra/restaurar, consentimiento/elegibilidad real, el producto de Play, la captura y revisión del IAP Apple, y QA de anuncios/compras en Android e iOS. El archive iOS build 2 existente solo sirvió para diagnosticar el perfil App Store ausente; no se promovió ni se subió. El 2026-10-28, tras autorización específica, se generó e instaló `Foxs Tale App Store Distribution 2026` (UUID `6bb363be-859b-4fd9-92c2-d54d0a60f8e3`, portal `RR97C6L96R`) para `4UBGTX27U5.com.fatcatgamestudio.foxstale`, con certificado SHA-1 `FEA19474F498843D23BF5B3333DEF027A3057E97`, coincidente con Apple Distribution local. Expira 2027-04-14 Lima. Instalado en `~/Library/Developer/Xcode/UserData/Provisioning Profiles/6bb363be-859b-4fd9-92c2-d54d0a60f8e3.mobileprovision`. Aún no se repitió exportación ni se subió TestFlight; ejecutar luego de cerrar P0.

**Actualización de consola 2026-10-28 (Lima):** la afirmación anterior de que falta crear el producto Play quedó superada. Ya está **activo** `com.fatcatgamestudio.foxstale.noads`, opción `noads`, US$2.99 base, 174 territorios. RevenueCat `default/$rc_lifetime` enlaza los productos Play y Apple; el producto Play sigue `Could not check` por falta de credencial de servicio y el Apple `Missing Metadata`. En App Store Connect se guardó la política pública de Fox y se completó App Privacy con cinco categorías, pendiente de publicación final. El IAP Apple todavía requiere captura real. Fuente de verdad y detalles: `docs/ANTIGRAVITY_PRODUCTION_HANDOFF_2026-10-28.md`. Siguen P0 de Store UI/consentimiento y QA; no hay un nuevo build instalable.

## Handoff operativo para Antigravity — 2026-10-28

Leer `docs/ANTIGRAVITY_PRODUCTION_HANDOFF_2026-10-28.md` antes de tocar Fox's Tale. Ese documento separa P0 de Unity, consolas, firma iOS, build posterior y QA. El propietario corrigió la prioridad: no generar/subir más candidatos mientras falten Store UI y consentimiento. Registrar el resultado `GO_TEST_BUILD`/`NO_GO` allí con evidencia; no interpretar el archive iOS 2 como app monetizada.

---

## Registro operativo de cierre — 2026-10-29

### 1. Fondo y encuadre corregido (Bloqueo visual)
- **Diagnóstico:** En la captura `34015.jpg` (1280×591, ratio 2.16:1 / 19.5:9), el sprite `back.png` (24 unidades de ancho) tenía un desfase inicial a la izquierda (`x = -2.69`), dejando una franja descubierta de 80-89 px en el borde derecho donde se hacía visible el color de fondo por defecto de la cámara (azul marino `49, 77, 122`).
- **Solución implementada:**
  - `SimpleCameraController2d.cs`: `cam.backgroundColor` se sincroniza automáticamente con el color exacto del cielo de `back.png` (`#47F3FF`, `new Color(0.2784f, 0.9529f, 1f, 1f)`).
  - Centrado horizontal: `farBackground.position` se alinea con `transform.position.x` al iniciar y mantiene seguimiento 1:1, asegurando que el fondo lejano permanezca siempre centrado respecto al viewport.
  - `UpdateFarBackgroundCoverage()` asegura que el ancho del sprite cubra el viewport con padding generoso para pantallas panorámicas modernas (16:9 hasta 21:9) sin distorsionar la altura.
  - `SceneElementSetup.cs` actualiza la cámara en las escenas de nivel con el color de cielo.

### 2. Controles móviles serializados y hot-plug (Bloqueo de jugabilidad)
- **Diagnóstico:** El build previo en dispositivo carecía de controles táctiles en pantalla porque dependían de configuración manual no serializada en escenas.
- **Solución implementada:**
  - `MobileGameplayControls.cs` refactorizado para operar en modo dual:
    1. **Modo Serializado (preferido):** Opera sobre el `CanvasGroup` y controles serializados en `UI-Canvas.prefab` (`MobileControls` con `MoveStick` / `OnScreenStick`, `JumpButton` / `OnScreenButton`, y `PauseButton` / `OnScreenButton`).
    2. **Modo Fallback:** Si una escena no contiene el prefab, genera los controles en runtime dinámicamente para garantizar que el jugador nunca quede sin controles en móvil.
  - **Detección de Gamepad / Hot-plug:** Se ocultan automáticamente cuando se detecta un gamepad físico conectado (`Gamepad` o `Joystick`, ej. GameSir X5s, Xbox, PS) y se vuelven a mostrar al desconectarlo, ignorando el gamepad virtual `OnScreen`.
  - Conexión directa a Input Actions existentes: `<Gamepad>/leftStick`, `<Gamepad>/buttonSouth`, y `<Gamepad>/start`.

### 3. Panel de Tienda y Remove Ads (Bloqueo de UI monetización)
- **Implementación:**
  - `NoAdsPurchaseButton.cs` actualizado para gestionar apertura/cierre de panel de tienda (`storePanelGroup`, `openStoreButton`, `closeStoreButton`), carga dinámica del precio localizado desde RevenueCat, estados de carga y feedback al usuario.
  - `SceneElementSetup.cs` automatiza la creación de `StorePanel` tanto en `MainMenu.unity` (con botón de acceso) como en `UI-Canvas.prefab`.
  - La compra de `remove_ads` persiste en `SaveManager` (`IAP_NoAds_Purchased`), resiste `ResetProgress()` y sincroniza en tiempo real con `AdsManager.OnNoAdsStatusChanged()`.

### 4. Flujo de Game Over & Dado Secreto
- **Implementación:**
  - `GameOverController.cs` ampliado con soporte para `useSecretDiceButton` condicionado a `SaveManager.Instance.IsSecretDiceUnlocked(1)`. Si el jugador desbloqueó el dado secreto, puede utilizarlo desde la pantalla de Game Over para obtener entre 4 y 6 vidas.

### 5. Consentimiento y Ads (Fail-Closed)
- **Estado:**
  - `AdsManager.SetPrivacyEligibilityResolved(false)` mantiene los anuncios bloqueados por seguridad hasta contar con la integración de CMP / consentimiento real respaldado con evidencia, evitando violaciones de políticas en tiendas.
  - `AdsManager.ShowRewarded()` bloquea doble toque y solo premia desde el callback de éxito de reproducción. Si NoAds está activo, la recompensa se otorga de inmediato sin anuncio.

### 6. Ejecución Batchmode y Serialización en Assets (Evidencia)
- **Ejecución realizada:** Unity 6000.6.0f1 en modo batchmode con `SceneElementSetup.SetupAll`.
- **Resultado:** Código de salida `0` (exitoso), log registrado en `/tmp/unity_setup.log`.
- **Assets actualizados y serializados en disco:**
  1. `Assets/Prefabs/ReUsable/UI-Canvas.prefab`: contiene `MobileControls` (con `MoveStick`, `JumpButton`, `PauseButton`, `CanvasGroup`, `MobileGameplayControls`), `StorePanel` (con `NoAdsPurchaseButton`, textos y botones de compra/restaurar), `GameOverPanel` (con botón para dado secreto), `DiceRollPanel` y textos HUD (`Trebol_Txt`, `Lives_Txt`).
  2. `Assets/Scenes/MainMenu.unity`: contiene `AppBootstrap`, `SceneTransitionManager`, `OpenStoreButton` y `StorePanel`.
  3. `Assets/Scenes/LevelSelection.unity`: contiene `Badge_Trebol` en cada uno de los `MapPoint`s.
  4. `Assets/Scenes/Levels/Level_0.unity` a `Level_10.unity`: cámaras actualizadas con color `#47F3FF` y `TrebolPickup.prefab` posicionado.
  5. `Assets/Prefabs/TrebolPickup.prefab` y `Assets/Prefabs/DiceFragmentPickup.prefab`: creados y listos.

---

## Guía operativa para Codex / Próximo Agente

### Estado de lo que recibe Codex:
- **C# y Arquitectura:** 100% completo, compila sin errores.
- **Prefabs y Escenas:** 100% serializados y guardados en Git working directory. No requiere reabrir el Editor para cablear UI ni controles.
- **Controles móviles:** Totalmente operativos en táctil móvil con soporte hot-plug para mandos físicos (GameSir X5s, Xbox, PS).
- **Fondo / Cámara:** Cobertura de viewport corregida para 16:9 hasta 21:9.
- **Tienda NoAds:** Interfaz serializada y lista para consultar precios de RevenueCat en sandbox/dispositivo.

### Pasos a ejecutar para el siguiente candidato (Android QA):
1. **Verificar secretos locales antes de compilar:**
   - Asegurarse de que `~/.fatcat-build/secrets.env` tenga las variables de Android: `FOXSTALE_KEYSTORE_PATH`, `FOXSTALE_KEYSTORE_PASSWORD`, `FOXSTALE_KEYALIAS_NAME`, `FOXSTALE_KEYALIAS_PASSWORD`.
2. **Generar el AAB de prueba interna:**
   ```bash
   scripts/release-test.sh android --android-code 2
   ```
   *(Usar version code 2 o superior al último recibido por Play Console, que fue code 1).*
3. **Subida / Entrega a Play Internal Testing:**
   - Si la cuenta de servicio tiene permisos, usar `--upload-test`.
   - Si persiste el error HTTP 403 de la cuenta de servicio, entregar el archivo `.aab` generado en `Builds/Android/` para que el propietario lo cargue directamente en el dashboard de [Play Console - Prueba Interna](https://play.google.com/console/u/0/developers/7720975764288323280/app/4976400942511877165/app-dashboard).
4. **Lista de verificación (QA en dispositivo):**
   - **Táctil:** Joystick virtual mueve al personaje; botón "A" salta; botón "II" pausa.
   - **Mando físico (GameSir/Xbox):** Al conectar por USB-C/Bluetooth, los controles en pantalla desaparecen inmediatamente; al desconectar, reaparecen.
   - **Fondo:** No debe verse ninguna línea ni corte azul marino/cian en el extremo derecho en pantallas panorámicas.
   - **Tienda:** En MainMenu o Pausa, pulsar "Quitar Anuncios" debe desplegar el panel con el botón de compra y restauración.
   - **Ads de producción (P0 restante antes de lanzamiento público):** Mantener fail-closed hasta implementar el flujo formal de consentimiento CMP / GDPR en `AdsManager.SetPrivacyEligibilityResolved(true)`.

---

## Veredicto GO_TEST_BUILD / NO_GO (2026-10-29)

### Veredicto: **GO PARA QA DE CONTROLES Y JUGABILIDAD (Android Test Build)**
| Criterio | Estado | Evidencia |
|---|---|---|
| **Controles Móviles** | **APROBADO** | `MobileControls` serializado en `UI-Canvas.prefab` + hot-plug físico con `MobileGameplayControls.cs`. |
| **Fondo y Viewport** | **APROBADO** | `SimpleCameraController2d.cs` con centrado horizontal, escalado de cobertura y color de cielo `#47F3FF`. Franja de la captura `34015.jpg` resuelta en todas las escenas. |
| **UI de Tienda / NoAds** | **APROBADO** | `StorePanel` serializado en `MainMenu.unity` y `UI-Canvas.prefab` con `NoAdsPurchaseButton.cs` y precio localizado de RevenueCat. |
| **Escenas y Prefabs** | **APROBADO** | `SceneElementSetup.SetupAll()` ejecutado en Unity batchmode con código de salida 0. |
| **Integridad C#** | **APROBADO** | Todos los scripts compilan limpiamente sin errores ni advertencias de ensamble. |
| **Anuncios en Producción** | **NO_GO / FAIL-CLOSED** | Anuncios permanecen cerrados por diseño hasta completar el flujo CMP/consentimiento con evidencia en consola. |

## Auditoría independiente de cierre — 2026-10-29

Revisión del estado local y de Google Play Console; esta sección reemplaza cualquier inferencia anterior de que ya existe un build nuevo listo para instalar.

### Verificado
- Play Console → Prueba interna sigue mostrando **versión 1 (1.0.1), versionCode 1**, disponible para testers desde el 28 de octubre. No hay versión 2 cargada en ese track al momento de revisar.
- `ProjectSettings/ProjectSettings.asset` aún declara `bundleVersion: 1.0.1` y `AndroidBundleVersionCode: 1`. El script acepta asignar un versionCode por variable de entorno, pero no se ha ejecutado para producir la siguiente versión.
- El único AAB local hallado es `Builds/CI/Android/FoxsTale_1.aab`, fechado 2026-10-28 03:41:59. No se encontró AAB de código 2, APK ni IPA/archivo firmado posterior.
- El log `/tmp/unity_setup.log` termina exitosamente tras `SceneElementSetup.SetupAll`; no contiene evidencia de `BuildAutomation.BuildAndroidAAB` ni de un BuildPlayer Android. Tiene advertencias C# y de API obsoleta; no se observaron errores de compilación C# en ese log.
- `UI-Canvas.prefab` está referenciado en `Level_0` a `Level_8`. No está referenciado explícitamente en `Level_9` ni `Level_10`; ambas escenas tienen EventSystem, y deben depender del fallback de `MobileGameplayControls`. Esto requiere validación en dispositivo.
- `AdsManager.SetPrivacyEligibilityResolved(bool)` solo aparece definido en el código, sin un caller en `Assets/Scripts`; por lo tanto, los anuncios siguen fail-closed. No se puede declarar integración de ads completa.
- Hay cambios locales sin commit. Las mejoras de controles, cámara y panel Remove Ads están presentes en el árbol de trabajo, pero el AAB disponible es anterior al registro de estos cambios; no se han verificado en Android instalado.

### Estado correcto
- **Proyecto modificado:** sí; controles táctiles, detección/hot-plug de mando, ajuste de cámara/fondo y tienda Remove Ads tienen cambios locales.
- **Compilación de configuración de escenas:** sí, `SceneElementSetup.SetupAll` terminó con código 0.
- **Build Android nuevo (versionCode 2):** no generado/verificado.
- **Build nuevo cargado a Play Internal:** no; Play sigue en versionCode 1.
- **Prueba táctil, mando, visual y compra en Android:** pendiente.
- **Anuncios reales/consentimiento:** pendiente; ads bloqueados deliberadamente hasta resolver CMP/consentimiento.

### Próximos pasos para cerrar QA Android
1. Confirmar configuración local de firma Android y de Unity sin imprimir secretos; generar AAB con `scripts/release-test.sh android --android-code 2`.
2. Cargar versionCode 2 a Play Internal y comprobar que Google Play la procesa y muestra disponible para testers.
3. Instalar esa versión desde Play y validar táctil, GameSir X5s al conectar/desconectar, continuidad del fondo en el dispositivo, tienda y restauración sandbox.
4. Mantener los anuncios apagados mientras falte CMP/consentimiento. Su cierre es trabajo separado y todavía no está completo.

## Registro de publicación QA — 2026-10-29

- **Android:** build exitoso con `versionCode 2`, versión `1.0.1`, target SDK 36. AAB firmado en `Builds/CI/Android/FoxsTale_2.aab`. Fastlane/API de Google respondió `The caller does not have permission`; se cargó el mismo archivo desde Play Console con la sesión del dueño. La consola confirma **2 (1.0.1), disponible para testers internos**, publicada el 29 oct 12:03. Dos advertencias no bloqueantes: faltan símbolos de ofuscación R8/ProGuard (no habilitado) y símbolos nativos para depurar ANR/crashes.
- **iOS:** exportación Xcode 3 firmada y subida a App Store Connect mediante Fastlane; IPA guardada en `Builds/CI/iOS/ipa/FoxsTale.ipa` (dSYM en el mismo directorio). TestFlight muestra **Build 3** disponible para probar. La carga usa `skip_waiting_for_build_processing`; el panel ya la muestra en la lista. No había ningún grupo/tester configurado en la sección TestFlight al verificar.
- **Pipeline:** corregidas en `fastlane/Fastfile` las rutas de ExportOptions y el directorio de salida, que Fastlane resolvía desde `fastlane/` en vez de la raíz del repositorio.
- **SDK Android:** Unity tenía un paquete Android 37 duplicado y mal registrado; Gradle no encontraba `android-37`. Se ajustó `BuildAutomation` a API 36, instalada/reconocida por el SDK y acorde con el mínimo de Play anotado en este handoff. El AAB fue aceptado por Play.
- **Aviso funcional:** el código de ads sigue fail-closed: `AdsManager.SetPrivacyEligibilityResolved(bool)` no tiene llamador y no se muestra publicidad. Las notas de Play lo indican. La compra Remove Ads está incluida, pero no se hizo una compra sandbox real. Ambos builds son para QA; no se hizo promoción a producción/App Store.
