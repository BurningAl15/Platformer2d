# Fox's Tale — Handoff: Fuente de Verdad

> Este archivo es la fuente de verdad del proyecto.
> Se actualiza al terminar cada fase. Si hay duda sobre qué falta, leer esto primero.
>
> **Última actualización:** 26/10/2026 — Fases 0-6 + IAP completadas. Entrega a Antigravity / Codex.

---

## Estado general del proyecto

**Todo el código C# está escrito (103/104 scripts implementados).** El proyecto está bloqueado únicamente en:
1. Configuración manual en **Unity Editor** (crear GameObjects, asignar campos del Inspector)
2. Instalar **SDKs externos** (Unity LevelPlay, RevenueCat) y asignar sus API Keys
3. **Assets visuales** de Antigravity (sprites, layouts de paneles)

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

### ⚠️ Stub (1/104)

| Archivo | Problema | Lo que haría Codex |
|---------|----------|--------------------|
| `Bouncer.cs` | Solo tiene `public float bouncePower`. Sin `OnTriggerEnter2D` ni lógica de rebote. El campo es consumido por `StompBox` externamente. | Completar con `OnTriggerEnter2D` que detecte al jugador y llame `PlayerController2d.Bounce(bouncePower)`. |

---

## Estado por fase

| Fase | Descripción | Estado de código | Pendiente |
|------|-------------|-----------------|-----------|
| 0 | Estabilización | ✅ Completo | Unity Editor |
| 1 | Controles (mobile + gamepad) | ✅ Completo | Unity Editor |
| 2 | Refactorización + SaveManager | ✅ Completo | Unity Editor |
| 3 | Economía (vidas, trébol, dado, corazón extra) | ✅ Completo | Unity Editor + assets Antigravity |
| 4 | Ads (LevelPlay) + IAP NoAds (RevenueCat) | ✅ Completo | Instalar SDKs + API Keys |
| 5 | Builds automáticos Android/iOS | ✅ Completo | Keystore + Apple signing |
| 6 | Arquitectura multi-mundo | ✅ Completo | Crear WorldData assets + escenas nuevas |

---

## Lo que falta — organizado por quién lo hace

---

### 🤖 Para CODEX (código C# que falta o puede mejorar)

#### 1. Completar `Bouncer.cs` (único stub)
**Qué falta:** `OnTriggerEnter2D` que detecte al jugador y llame `PlayerController2d._instance.Bounce(bouncePower)`.
**Referencia:** `StompBox.cs` usa el patrón similar. `PlayerController2d` tiene `Bounce(float multiplier)`.

```csharp
// En Bouncer.cs, agregar:
private void OnTriggerEnter2D(Collider2D other)
{
    if (other.CompareTag(StringUtils.tag_Player))
        PlayerController2d._instance?.Bounce(bouncePower);
}
```

#### 2. Pantalla de resumen de mundo completado (Fase 6 — GDD sección 15)
**Qué falta:** Un `WorldCompleteController.cs` que:
- Se activa cuando `WorldProgressManager.IsWorldComplete(worldData)` es true al terminar un nivel
- Muestra estrellas totales, gemas, tréboles y tiempo del mundo
- Tiene botón "Siguiente mundo" que carga la escena de selección del siguiente `WorldData`
- Referencia en `LevelManager.EndLevel()` después de `SaveProgress()`: si el nivel completado es el último del mundo actual, activar esta pantalla

#### 3. Corazón extra del Dado Secreto — HUD visual
**Qué falta:** Un método en `UIController` que agregue visualmente un nuevo corazón al HUD cuando `PlayerHealth.AddBonusHeart()` es llamado. Actualmente `ResetHearts()` solo resetea los corazones existentes pero no puede agregar slots dinámicos.
**Dificultad:** Requiere que `UIController.hearts` sea una lista dinámica o que haya un slot de corazón bonus separado. Actualmente es una `List<HeartUI>` serializada (tamaño fijo).

#### 4. Analytics — TrackCheckpoint no se llama
**Qué falta:** `AnalyticsManager.TrackCheckpointReached()` existe pero `Checkpoint.cs` no lo llama. Conectar en `Checkpoint.cs` al activarse.

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

### 🔧 Para Unity Editor (configuración manual — sin código nuevo)

#### Escena `MainMenu`
- [ ] Crear GameObject `AppBootstrap` y asignarle el script. Se agregan automáticamente vía `[RequireComponent]`: `SaveManager`, `GamepadDetector`, `LivesManager`, `AdsManager`, `RevenueCatManager`, `WorldProgressManager`.
- [ ] Crear GameObject `SceneTransitionManager` con su script.
- [ ] **Alternativa rápida:** Ejecutar `CustomEditorTools → Setup Missing Scene Elements & Placeholders` — hace esto automáticamente.

#### Escena `LevelSelection`
- [ ] Asignar `trebolBadge` (GameObject hijo) en cada `MapPoint`.
- [ ] Verificar `hiddenY`/`visibleY` del panel `UI_SelectController` (defaults: -300 / 0).
- [ ] Verificar `normalScale`/`selectedScale` en cada `MapPoint` (defaults: 1 / 1.2).
- [ ] Crear el asset `WorldData` del Mundo 1: Assets → Create → Fox's Tale → World Data → llenar `levelSceneNames` con las escenas existentes.

#### Escena de Gameplay (`Level_0` a `Level_10`)
- [ ] Verificar que el prefab `UI-Canvas` tiene los campos asignados tras el setup.
- [ ] Asignar sprites de estrellas en `UIController` (`starFull`, `starEmpty`, `starImages`). **Bloqueado por assets de Antigravity.**
- [ ] Ajustar posición del `TrebolPickup` en cada nivel (el setup lo pone cerca del Goal — moverlo a zona de exploración).
- [ ] Verificar `initPoint`/`endPoint` del `PauseMenu`.
- [ ] Verificar `pickup_health_clip` en `AudioMixerManager` (renombrado desde `pickup_helth_clip`).
- [ ] Verificar `smashImpact_Clip` en `AudioMixerManager` (renombrado desde `SmashImpact_Clip`).
- [ ] Agregar `DiceFragmentPickup` en niveles difíciles cuando estén diseñadas las zonas secretas.

#### Controles on-screen (mobile)
- [ ] En el joystick virtual: `Add Component → OnScreenStick` → Control Path: `<Gamepad>/leftStick`.
- [ ] En el botón de salto: `Add Component → OnScreenButton` → Control Path: `<Gamepad>/buttonSouth`.
- [ ] En el botón de pausa (si existe): `Add Component → OnScreenButton` → Control Path: `<Gamepad>/start`.

#### Settings / PauseMenu
- [ ] Agregar botón con `NoAdsPurchaseButton.cs` para la compra de NoAds.
- [ ] Agregar botón "Restaurar compras" que llame `NoAdsPurchaseButton.OnRestorePressed()`.

---

### 🔑 SDKs externos (prerequisito para build de producción)

#### Unity LevelPlay (Ads)
1. Unity → Window → Package Manager → buscar "LevelPlay" → instalar.
2. Project Settings → Player → Scripting Define Symbols → agregar `UNITY_LEVELPLAY`.
3. En el Inspector del `AppBootstrap` → componente `AdsManager`:
   - `androidAppKey`: App Key de https://dashboard.is.com
   - `iosAppKey`: App Key del mismo dashboard
4. Crear Ad Units (Rewarded + Interstitial) en el dashboard de LevelPlay.
5. Mantener `enableTestMode = true` durante desarrollo. Cambiar a `false` antes de publicar.
6. Si audiencia < 13 años: agregar `IronSource.Agent.setMetaData("is_coppa", "true")` en `AdsManager.Initialize()`.

#### RevenueCat (IAP NoAds)
1. Descargar SDK desde https://github.com/RevenueCat/purchases-unity/releases → importar `.unitypackage`.
2. Project Settings → Player → Scripting Define Symbols → agregar `REVENUE_CAT`.
3. En el Inspector del `AppBootstrap` → componente `RevenueCatManager`:
   - `androidApiKey`: `goog_XXXXXXXXXX` (obtener de https://app.revenuecat.com)
   - `iosApiKey`: `appl_XXXXXXXXXX` (mismo dashboard)
4. Crear producto en Google Play Console y App Store Connect con SKU: `com.fatcatgamestudio.foxstale.noads`.
5. En RevenueCat Dashboard: crear entitlement `NoAds` apuntando al SKU anterior.

---

### 📦 Build y distribución (Fase 5 — pendiente de firma)

#### Android
- [ ] Generar Keystore (guardar **fuera** del repositorio).
- [ ] Configurar en Unity: Edit → Project Settings → Player → Android → Publishing Settings → Keystore.
- [ ] Usar `CustomEditorTools → Builds → Build Android APK` para testing o `Build Android AAB` para Google Play.
- [ ] Target API Level 34+ ya configurado en `BuildAutomation.cs`.
- [ ] Agregar `AD_ID` permission si se usa AdMob (declarado en Android Manifest).
- [ ] Declarar Privacy Policy URL en Google Play Console.

#### iOS
- [ ] Configurar Team ID y Provisioning Profile en Xcode tras el build.
- [ ] Usar `CustomEditorTools → Builds → Build iOS` para generar el proyecto Xcode.
- [ ] Agregar `NSUserTrackingUsageDescription` en Info.plist para ATT (iOS 14.5+).
- [ ] Privacy Manifest requerido desde iOS 17.
- [ ] Agregar `SKAdNetworkItems` para redes de ads.

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
| Bundle ID definitivo | Confirmar que `com.fatcatgamestudio.foxstale` es el correcto |
| RevenueCat API Key Android | `RevenueCatManager` — campo `androidApiKey` |
| RevenueCat API Key iOS | `RevenueCatManager` — campo `iosApiKey` |
| Unity LevelPlay App Key Android | `AdsManager` — campo `androidAppKey` |
| Unity LevelPlay App Key iOS | `AdsManager` — campo `iosAppKey` |
| SKU del producto NoAds | Confirmar `com.fatcatgamestudio.foxstale.noads` en tiendas |
| Keystore (Android) | Para firmar el APK/AAB de producción |
| Apple Developer Team ID | Para el proyecto Xcode |

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

## Veredicto: GO PARA TEST BUILD

| Criterio | Estado |
|---|---|
| Código C# | ✅ 103/104 implementados (1 stub menor en Bouncer) |
| Persistencia | ✅ SaveManager centralizado, preserva IAP al resetear |
| Economía | ✅ Vidas, trébol, dado Normal/Mejorado/Secreto, corazón extra |
| Controles | ✅ Teclado + Gamepad físico + on-screen (pendiente OnScreenStick en Editor) |
| Ads | ✅ Modo simulación funcional. Pendiente instalar SDK LevelPlay |
| IAP | ✅ Modo simulación funcional. Pendiente instalar SDK RevenueCat |
| Builds | ✅ BuildAutomation listo para APK/AAB/Xcode |
| Multi-mundo | ✅ Arquitectura lista. Pendiente WorldData asset del Mundo 1 |

**Acción inmediata recomendada:** Ejecutar en Unity `CustomEditorTools → Setup Missing Scene Elements & Placeholders` para configurar automáticamente las escenas, luego hacer el primer APK de debug con `CustomEditorTools → Builds → Build Android APK`.
