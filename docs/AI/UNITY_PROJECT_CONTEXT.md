# Fox's Tale Architecture Review and Project Context

**Review date:** 2026-11-13
**Reviewed revision:** `ffffd32` (`codex/fix-horizontal-platform-riding`)
**Review type:** Repository and serialized-project static review; read-only for Unity assets and runtime code.

## Executive summary

Fox's Tale is a Unity 2D platformer with a playable mobile/desktop input layer, authored level scenes, local progression, collectible and life systems, a level map, UI, and integrations for LevelPlay and RevenueCat. Most gameplay is built from focused `MonoBehaviour` components and prefab/scene references. `WorldData` is the only first-party gameplay `ScriptableObject` definition found in `Assets/Data`.

The earlier architecture pass is `.kiro/fase-2-reporte.md` (2026-10-25). It records a targeted refactor around `SaveManager`, `AudioMixerManager`, `LevelManager`, and `AnalyticsManager`; it is useful history but is not a current codebase map. This review updates the picture: the main architectural pressure now comes from responsibilities and dependencies concentrated in a few managers, repeated access to persistent data outside `SaveManager`, cross-links between ads and purchases, and a lack of compile-time boundaries between first-party runtime systems.

The player, enemy, hazard, platform, and pickup behaviours are already divided into small reusable components. They are not the first candidates to split further. The safest modularization order is to centralize existing data ownership, clarify feature boundaries, make level metadata a single source of truth, and add assembly/test boundaries only when those seams are stable. No code or Unity asset refactor was made during this review.

## Environment and project shape

| Area | Confirmed state |
|---|---|
| Unity | 6000.6.0f1 (`f7f8ed4d1e24`) |
| Render pipeline | Built-in Render Pipeline; `GraphicsSettings.m_CustomRenderPipeline` is null |
| Input | Input System package 1.20.1, generated `PlayerInputSystem` wrapper, and a project `InputManager` hub. On-screen controls use Input System components. |
| UI | uGUI 2.6.0 with TextMeshPro; persistent UI is primarily serialized in scenes and prefabs, though Settings also constructs optional controls at runtime. |
| Third-party runtime packages | LevelPlay 9.5.1, RevenueCat 9.15.0, External Dependency Manager 2.1.1, DOTween under `Assets/Demigiant` |
| Networking / online level service | No first-party networking or UGC backend was found in package dependencies or source. `ProjectSettings/MultiplayerManager.asset` alone does not establish multiplayer use. |
| First-party runtime code | 100 C# files under `Assets/Scripts`, approximately 11,645 lines; plus generated `Assets/PlayerInputSystem.cs` |
| First-party editor code | 9 C# files under `Assets/Editor` |
| Assembly definitions | The 3 `.asmdef` files found are inside the PlayerPrefsEditor asset. First-party game code has no separate assembly definition and compiles in `Assembly-CSharp`. |
| Content | 16 scene assets, 73 prefabs under `Assets/Prefabs`, and one first-party asset under `Assets/Data` (`WorldData_World1.asset`). Imported asset folders add more content outside this count. |
| Build scenes | `EditorBuildSettings.asset` enables Splash, Main Menu, Level Selection, Level_0 through Level_8, and a Testing scene. `BuildAutomation` separately hardcodes Splash, menus, and Level_0 through Level_10. This is two scene lists with different contents. |
| Tests | No project-owned EditMode/PlayMode test assembly or Unity Test Framework dependency was found. `Assets/Scenes/Testing` and `Assets/Scripts/Testing.cs` are manual development assets, not automated test coverage. Tests were not run. |
| Unity Editor tooling | Unity CLI status reported no Pipeline instance for this project; no live Editor scene inspection or Play Mode validation was available. |

The current local branch is `codex/fix-horizontal-platform-riding` at `ffffd32`. At review start, the worktree also contained untracked `.codex-attachments/` folders and `Assets/Resources/RevenueCatPublicKeys.local.json.meta`; those were left untouched and are not part of this report. No credential or key material was opened.

## Runtime flow

1. `FatCatSplash.unity` is the first enabled build scene, followed by `MainMenu.unity`.
2. `AppBootstrap` persists across scenes and initializes DOTween, frame rate, and landscape orientation. Its required components include save/progression, lives, gamepad, ads, RevenueCat, and world progress.
3. `MainMenuManager` chooses new game or continue using save/tutorial state. `SceneTransitionManager` and the static `SceneUtils` helpers handle scene transitions, fades, and tutorial guards.
4. `LevelSelection.unity` composes map points from `WorldData`/`MapPointsHolder`; `LSPlayerController` handles map navigation, camera interaction, and level selection.
5. A gameplay scene composes the player, level managers, reusable UI canvas, hazards, enemies, collectibles, and environment prefabs. `LevelManager` tracks level time/gems, damage and respawn flow, completion, persistence, rating, and subsequent UI/scene flow.
6. UI and economy managers mediate pause, game over, lives/dice rewards, world-completion flow, and return to the map.

This flow is mostly Inspector- and scene-composed rather than dependency-injection composed. Static singleton access and direct concrete-type references are common; a small number of C# events exist for events such as game over and dice roll completion.

## Functional architecture and code inventory

The table covers all 100 project-owned runtime C# files in `Assets/Scripts`. The responsibility descriptions are based on file contents, exposed methods, and cross-references, not just folder names.

### App flow and scene coordination

| Script | Responsibility |
|---|---|
| `Core/AppBootstrap.cs` | Persistent startup composition; initializes DOTween, frame rate, and mobile orientation; requires major app services. |
| `Core/MainMenuManager.cs` | Main menu, new/continue flow, panel activation, tutorial/progress routing, quit. |
| `Core/SceneTransitionManager.cs` | Asynchronous scene loading, fade coordination, tutorial protection, and transition input/pause state. |
| `Core/LevelSelectManager.cs` | Starts gameplay from the selected map level. |
| `Core/LevelManager.cs` | Per-level lifecycle, elapsed time, gem count, death/fall respawn, completion, save/rating, analytics, and next-screen orchestration. |
| `Core/CheckpointManager.cs` | Stores and restores the active checkpoint position. |
| `TutorialEnding.cs` | Tutorial completion and exit flow. |
| `Utils/SceneUtils.cs` | Static scene-name parsing, common scene routing, and fallback/tutorial guards. |
| `Utils/StringUtils.cs` | Legacy level/tag/preference-key parsing and utility conversions. |
| `ResetPosition.cs` | Captures and restores an object's starting transform. |

### Save data, world progression, and scoring

| Script | Responsibility |
|---|---|
| `Data/SaveManager.cs` | PlayerPrefs-backed owner for level unlocks, best times/gems/ratings, wallet/lives, heart fragments, clovers, dice fragments, audio preferences, tutorial flags, and reset. |
| `Data/WorldData.cs` | ScriptableObject containing world name/number, scene-name list, unlock threshold, collectible totals, selection scene, thumbnail, and accent color. |
| `Data/WorldProgressManager.cs` | Aggregates ratings, gems, clovers, completion, and unlock status by parsing level scene names. |
| `Systems/LevelRatingSystem.cs` | Calculates one-to-three-star ratings from completion, gem total, and target time. |
| `Systems/LivesManager.cs` | Owns life economy decisions/events and delegates persistence to `SaveManager`. |
| `Systems/ComboManager.cs` | Tracks consecutive gem pickup combo timing and asks audio to reset pitch. |

### Player and physics

| Script | Responsibility |
|---|---|
| `Player/PlayerController2d.cs` | Player-level coordinator for input, movement/jump, pause, knockback, effects, and jump audio. |
| `Player/PlayerMovement.cs` | Horizontal movement acceleration and speed. |
| `Player/PlayerJump.cs` | Jump requests, jump processing/cut, and coyote-time/ground integration. |
| `Player/PlayerGroundCheck.cs` | Ground detection and coyote-time state. |
| `Player/PlayerHealth.cs` | Damage, fall damage, invulnerability, death and reset flow, bonus/permanent hearts, related save/UI/audio/analytics/checkpoint calls. |
| `Player/PlayerKnockback.cs` | Applies knockback and bounce impulses. |
| `Player/PlayerAnimator.cs` | Connects movement/ground state and hurt triggers to Animator. |
| `Player/PlayerEffects.cs` | Player visual/particle effects driven by movement and jump state. |
| `Player/PlatformRider.cs` | Detects/maintains moving-platform contact and velocity inheritance. |

### Enemies and boss

| Script | Responsibility |
|---|---|
| `Enemies/EnemyParent.cs` | Shared enemy death effect, collider disabling, and deactivate/destroy sequence. |
| `Enemies/EnemyController.cs` | Basic enemy movement/behavior based on the shared enemy base. |
| `Enemies/RunningEnemy.cs` | Directional running behavior. |
| `Enemies/FlyingEnemyController.cs` | Flying enemy movement/target behavior. |
| `Enemies/TutorialEnemy.cs` | Tutorial-specific stomp interaction. |
| `Enemies/BossTankController.cs` | Boss state loop, movement, attacks, health, mines, defeat/explosion/platform flow, audio and UI. |
| `Enemies/BossTankHitbox.cs` | Routes boss hitbox contacts to player/stomp/boss logic. |
| `Enemies/BossTankMine.cs` | Boss mine behavior and destruction. |
| `Enemies/Bullet.cs` | Boss projectile collision, damage and impact. |
| `Enemies/Dart.cs` | Dart projectile collision, damage and impact. |

### Level objects: collectibles, checkpoints, interactions

| Script | Responsibility |
|---|---|
| `Environment/Interactables/Pickup.cs` | Gem or health pickup, guarded collection, SFX, level/player health updates, collection effect. |
| `Environment/Interactables/TrebolPickup.cs` | Per-level Clover persistence, duplicate suppression, HUD refresh and collection effect. |
| `Environment/Interactables/HeartFragmentPickup.cs` | Unique per-level permanent-heart fragment, persistence and health/HUD update. |
| `Environment/Interactables/DiceFragmentPickup.cs` | Per-world/level secret-dice fragment persistence and collection feedback. |
| `Environment/Interactables/Checkpoint.cs` | Activates checkpoint and updates current spawn position. |
| `Environment/Interactables/Goal.cs` | Detects player goal contact and completes the level. |
| `Environment/Interactables/StompBox.cs` | Resolves stomp contacts, enemy death/bounce, and activation-layer behavior. |
| `Environment/Interactables/Bouncer.cs` | Bounces the player through `PlayerController2d`. |
| `Environment/Interactables/Switch.cs` | Activates/deactivates connected level mechanisms. |
| `Environment/Interactables/SpawnerSwitch.cs` | Adapts switch activation to spawner actions. |
| `Environment/Interactables/BossSwitch.cs` | Starts boss music/encounter flow. |

### Hazards, platforms, and spawners

| Script | Responsibility |
|---|---|
| `Environment/Hazards/DamagePlayer.cs` | Applies contact damage to the player. |
| `Environment/Hazards/DeathZone.cs` | Processes void/death-zone contact, player fall damage/respawn request, falling logs, and enemy containers. |
| `Environment/Hazards/FallingLog.cs` | Falling log lifecycle and reset. |
| `Environment/Hazards/FallingSpike.cs` | Falling spike activation/behavior. |
| `Environment/Hazards/HangingHazzard.cs` | Hanging hazard behavior. |
| `Environment/Hazards/HoldingDamageBall.cs` | Damage-ball behavior configured by its signal component. |
| `Environment/Hazards/HoldingDamageBall_Signal.cs` | Signal/activation for a holding damage ball. |
| `Environment/Hazards/PendulumChain.cs` | Serialized pendulum-chain point configuration. |
| `Environment/Hazards/PendulumMovement.cs` | Pendulum motion. |
| `Environment/Hazards/Smasher.cs` | Smasher motion/damage feedback including audio and camera shake. |
| `Environment/Hazards/SmasherSignal.cs` | Smasher activation signal. |
| `Environment/Platforms/MovingPlatform.cs` | Rigidbody2D waypoint motion and published fixed-step displacement/velocity. |
| `Environment/Platforms/PlatformPassengerDetector.cs` | Detects/associates player passengers with platforms. |
| `Environment/Platforms/MovingEdge.cs` | Moving edge/level mechanism motion. |
| `Environment/Platforms/RotatatingEdge.cs` | Rotating edge behavior. |
| `Environment/Spawners/SpawnerParent.cs` | Shared spawner contract and base behavior. |
| `Environment/Spawners/SpawnerTime.cs` | Timed enemy spawning and reset/start/stop controls. |
| `Environment/Spawners/SpawnByGroup.cs` | Spawns a configured group of enemies. |
| `Environment/Spawners/SpikeSpawner.cs` | Spike spawning and reset. |
| `Environment/Spawners/ActivateSpawner.cs` | Connects a trigger to timed-spawner activation. |
| `Environment/Spawners/ActivateFallingSpikes.cs` | Connects activation to falling spike spawner. |
| `Environment/Spawners/BushManager.cs` | Bush particle effect. |
| `Environment/Spawners/KillEnemyContainer/KillEnemyContainer.cs` | Removes/cleans spawned enemy groups. |
| `Systems/WaveMovement.cs` | Wave movement behavior for an enemy derived from `EnemyParent`. |

### Input and mobile presentation

| Script | Responsibility |
|---|---|
| `Utils/InputManager.cs` | Persistent Input System hub for Player/UI action maps and jump/pause/submit/navigation events. |
| `Mobile/GamepadDetector.cs` | Detects physical gamepads and forwards changes. |
| `Mobile/PlatformDetector.cs` | Platform/scene-based control configuration, virtual-control suppression, activation repair and diagnostics. |
| `Mobile/MobileGameplayControls.cs` | CanvasGroup visibility/interactivity rules for mobile controls versus physical controllers. |
| `Mobile/MobileInputProvider.cs` | Adapter for custom on-screen move/jump inputs. |
| `Mobile/OnScreenControlsSetup.cs` | Wires Unity on-screen controls into game input. |
| `Mobile/SafeAreaAdjuster.cs` | Applies display safe-area bounds to a UI RectTransform. |
| `Assets/PlayerInputSystem.cs` | Generated action-map wrapper for the `.inputactions` asset; do not hand-edit. |

### Level selector

| Script | Responsibility |
|---|---|
| `LevelSelection/MapPoint.cs` | Represents one level node, selection feedback, and per-level gem/time/Clover state. |
| `LevelSelection/MapPointsHolder.cs` | Configures map nodes from save state and optional `WorldData`, and positions the selector character. |
| `LevelSelection/LSPlayerController.cs` | Handles map movement/route navigation, touch gestures, selection, camera coordination, and level-entry input. |

### UI and presentation

| Script | Responsibility |
|---|---|
| `UI/UIController.cs` and `HeartUI` | Gameplay HUD: health hearts, gems, Clover/lives/global-gem display, level completion/rating animations. |
| `UI/UI_SelectController.cs` | Level selector detail card, level stats, and selection panel presentation. |
| `UI/SettingsController.cs` | Settings panel and navigation, runtime option creation, language/audio/haptics preference changes, progress summary/reset, version display. |
| `UI/PauseMenu.cs` | Pauses gameplay, routes pause-menu actions, reports level/progression summary. |
| `UI/GameOverController.cs` | Game-over UI, rewarded ad, gem/Clover/dice continue actions, selection and lives flow. |
| `UI/WorldCompleteController.cs` | World completion summary and continuation. |
| `UI/FadeEffect.cs` | Screen fade lifecycle and completion signal. |
| `UI/AppVersionDisplay.cs` | Reads build metadata and formats displayed version/build. |
| `UI/UISelectionUtility.cs` | Selects a usable default UI control for keyboard/gamepad navigation. |
| `Camera/SimpleCameraController2d.cs` | Gameplay camera follow/stop behavior. |
| `Camera/LSCameraController.cs` | Level-map camera framing/follow behavior. |
| `Camera/ScreenShake.cs` | Camera shake feedback. |

### Audio, monetization, and analytics

| Script | Responsibility |
|---|---|
| `Audio/AudioMixerManager.cs`, `SFXElement`, `SFXType` | Music/SFX playback, boss music, SFX routing, gem pitch/combos, persisted volume toggles, scene audio handling. |
| `Managers/AdsManager.cs` | LevelPlay lifecycle, initialization/retries, privacy eligibility gate, rewarded/interstitial placements, cadence, remove-ads state, callback/failure handling. |
| `IAP/RevenueCatManager.cs` | RevenueCat initialization, Remove Ads entitlement, purchase/restore and local entitlement cache. |
| `IAP/NoAdsPurchaseButton.cs` | Purchase/restore UI panel and purchase feedback. |
| `Managers/AnalyticsManager.cs` | Session and gameplay event tracking, local debug output and conditional Unity Analytics delivery. |

### Other runtime files

| Script | Responsibility |
|---|---|
| `Systems/DiceRollController.cs` | Dice UI/animation and random-result lifecycle; awards lives/bonus heart and continues level/game-over flow. |
| `Testing.cs` | Small manual Input System development helper. |
| `Utils/TurnOff_OnTime.cs` | Timed effect deactivation/completion signal. |

### First-party editor utilities

| Script | Responsibility |
|---|---|
| `Editor/BuildAutomation.cs` | Unity Android/iOS build settings, required scene list, build info, signing/build output and export operations. |
| `Editor/BuildAutomation/FirebaseAndroidLocalRepositoryPostprocessor.cs` | Post-processes generated Android Gradle project for local Firebase repository configuration. |
| `Editor/CountGemsInLevel.cs` | Editor window/utility for counting level gem pickups. |
| `Editor/PlayerResetEditor.cs` | Editor window to reset a player object in scene. |
| `Editor/SceneElementSetup.cs` | Editor setup helpers for scene elements. |
| `Editor/AudioMixerManagerEditor.cs` | Custom Inspector for audio manager. |
| `Editor/ComboManagerEditor.cs` | Custom Inspector for combo manager. |
| `Editor/SpawnerSwitchEditor.cs` | Custom Inspector for spawner switch. |
| `Editor/SpawnerTimeEditor.cs` | Custom Inspector for timed spawner. |

## Confirmed architectural findings

### A. First-party runtime code has no assembly boundaries

All game code references `Assembly-CSharp`; the only `.asmdef` files belong to the imported PlayerPrefsEditor utility. This makes it easy for any system to depend on any other system and means a future split has no compiler-enforced direction. This is a structural risk, not a current runtime defect.

**Incremental direction:** first establish clear source dependencies, then add runtime/editor/test assemblies in small steps. Avoid a large assembly migration during gameplay iteration.

### B. Persistence is centralized in intent, but not in every call site

`SaveManager` has a broad API for progression and settings, but `PlayerPrefs` is still accessed directly by `MainMenuManager`, `SceneTransitionManager`, `SceneUtils`, `MapPointsHolder`, `SettingsController`, `UI_SelectController`, `PauseMenu`, `AdsManager`, `RevenueCatManager`, and `NoAdsPurchaseButton`. Some direct reads are explicit fallbacks, but they leave multiple definitions of tutorial, language, and purchase state. The old phase-2 report says no other class should access PlayerPrefs; the current source no longer meets that invariant.

**Incremental direction:** preserve all current keys and migrations; route reads/writes through the existing facade first. Later split the facade into domain-specific persistence components only when a second consumer or test seam requires it. Keep purchase entitlement authoritative to RevenueCat and treat local values as cache.

### C. Feature coordination is concentrated in large controllers

The largest first-party files are `AdsManager` (671 LOC), `SettingsController` (624), `GameOverController` (431), `DiceRollController` (410), `LSPlayerController` (400), `SaveManager` (364), `UIController` (354), `PlayerHealth` (299), `LevelManager` (269), and `PlatformDetector` (267). Size alone is not a defect; their observed responsibilities reveal the highest payoff for gradual separation:

- `LevelManager` handles gem-count discovery, clock, persistence, analytics, damage/respawn, rating, dice/world completion, camera/audio, and transitions.
- `SettingsController` handles UI construction, navigation, settings persistence, localization decisions, reset confirmation, progress/version display, audio, and haptics state.
- `GameOverController` and `DiceRollController` mix presentation flow with economy/ad/continuation decisions.
- `LSPlayerController` mixes map input, path/navigation, player marker movement, camera control, level selection, sound, and settings interactions.
- `AdsManager` and `RevenueCatManager` each reference the other directly, creating a bidirectional feature dependency around Remove Ads.
- `PlatformDetector` and `MobileGameplayControls` both participate in platform/gamepad and touch-control visibility decisions; preserve the confirmed working hotplug behavior while consolidating ownership later.

These are refactoring candidates, not proof that each needs a new script. Extract a responsibility only when it gives one owner, one input/output contract, and a testable behavior.

### D. Level metadata has multiple sources of truth

`WorldData_World1.asset` stores level scene names. `WorldProgressManager` and `MapPointsHolder` parse level numbers from those names. The editor build scene list and `BuildAutomation.RequiredScenes` are separately maintained; one includes Level_9/10 while the other excludes them and includes a Testing scene. This creates risk when adding campaign chapters or validating a future user-authored level.

**Incremental direction:** introduce a stable `LevelDefinition` asset only when useful for the next authored levels; have one world/campaign catalog refer to definitions; derive build inclusion and selector metadata from that catalog where Unity's build pipeline permits. Keep scene content as authored Unity scenes for now.

### E. Scene-oriented content is not yet player-authored content

There are 16 `.unity` scene files and current gameplay geometry is authored in scenes/prefabs. `WorldData` describes a list of scenes and world totals, but it does not represent tile placement, object placement, logic links, validation rules, or a serializable player level. No online service/backend was found. Therefore, the existing multi-world model helps organize the story campaign but is not yet a foundation for an online user-level editor.

**Incremental direction for the long-term goal:** finish designing levels with current Unity tooling; define a small data format for supported tiles/objects and a validator; prototype local create/play/export/import; then add hosting, accounts, reports/moderation, and discovery only after the local format works. The online editor is a separate product phase, not a toggle on the current scene system.

### F. The level list differs between manual and scripted builds

The project has scenes Level_0 through Level_10. The enabled editor list includes Level_0 through Level_8 plus Testing; `BuildAutomation` has its own required list through Level_10. Release scripts call the custom automation, but manual Editor builds can therefore package a different scene set. Reconcile this before relying on either build path for campaign expansion.

### G. Settings are a likely next separation point

Settings controls are mostly persistent in the UI prefab/scenes, but `SettingsController.EnsureUIElementsExist()` clones/builds several controls at runtime. The same controller handles view wiring, focus/navigation, toggles, language text, audio, haptics persistence, reset dialog, progress summary, and version. This is a high-regression surface and conflicts with the project's preference for serialized persistent UI.

**Incremental direction:** retain the current view and serialized references, make the UI hierarchy explicit in the prefab, keep the controller as a thin view binder, and move actual settings ownership to narrow existing/game-level services. Do not introduce a generic plugin framework just to support one game. Extract a reusable settings package after a second game proves which options and contracts are truly common.

### H. Gameplay componentization is already reasonably granular

Player movement/jump/ground/health/knockback/effects, enemy base and variants, hazards, moving platforms, switches, pickups, and spawners are mostly separate components. The recent platform work also exposes a useful narrow contract: `MovingPlatform` publishes fixed-step displacement/velocity and `PlatformRider` consumes it. Preserve these targeted boundaries; broad rewriting of these systems would add risk without clear evidence.

## Prioritized modularization plan

This order supports the goal of a polished story mode first and a player-level editor later.

| Priority | Work | Scope and guardrail | Completion evidence |
|---|---|---|---|
| 1 | Reconcile progression, save keys, and build scene registry | Keep existing PlayerPrefs keys and serialized fields. Establish one source for campaign levels/build scenes before adding chapters. | Map, save, manual build, and release build agree on the same authored level catalog; migration and tutorial/progression behavior verified. |
| 2 | Stabilize feature boundaries in small slices | First candidates are settings/preferences and ad/remove-ads entitlement; then level completion flow. Replace only direct cross-feature calls that currently create duplicate ownership. No general event bus. | Each feature has one clear owner; existing buttons/prefabs still bind; behavior passes targeted scene/device QA. |
| 3 | Add testable seams and assembly boundaries | Introduce tests for save-key migrations, gem totals, rating rules, tutorial guards, level catalog validation, and reward limits. Add asmdefs after reference direction is stable. | Tests run in Unity; production/editor/test assembly dependency graph compiles. |
| 4 | Improve authored campaign workflow | Use `WorldData`/new level metadata to make authored levels discoverable and validated. Keep tilemap/physics authoring in Unity and improve editor counting/check tools. | Adding a story level is a short documented workflow with correct selector/build/progression data. |
| 5 | Prototype local player-created levels | Define a constrained, versioned level format for tiles and approved prefabs; validate bounds, exits, hazards, and object budgets; prototype playtest and export/import. | A created level can be saved, loaded, played, validated, and rejected safely without hand-editing scenes. |
| 6 | Plan online publishing | Only after local creation works: hosting, versioning, accounts or anonymous identity, moderation/reporting, and discovery/ranking policies. | Security/privacy/moderation and operations are designed alongside the service; never trust uploaded level data. |

## Complete runtime class inventory

The functional tables above include every one of the 100 project-owned runtime C# files. A quick symbol inventory also found these auxiliary data types in those files: `SFXType`, `SFXElement`, `HeartUI`, `AdContext`, `PickupType`, `PendulumChainPoint`, `SpawnGroup`, `LevelRating`, `LevelRequirements`, `BossStates`, `DiceType`, and the generated input action structs/interfaces. This is a static inventory; Inspector serialization and scene-specific wiring remain separate evidence.

## Documentation and review history

- `.kiro/fase-2-reporte.md`: earlier targeted architecture refactor; useful historical baseline, not a complete current map.
- `.kiro/fase-0-reporte.md` through `.kiro/fase-6-reporte.md`: phased implementation/handoff records for stabilization, input, economy, monetization, builds, and multi-world support.
- `.kiro/game-design-document.md`: current feature/design direction for campaign systems, collectibles, health/lives, dice, ads/IAP, and world progression.
- `.kiro/handoff.md`: chronological integration, QA, release, and known-issue record.
- `docs/AI/MODEL_AND_TASK_REPORTING.md`: model routing and task completion/reporting standard.
- `scripts/release-test.sh` and `fastlane/Fastfile`: internal build/upload pipeline; targets Google Play Internal Testing and TestFlight.

## Limitations and unknowns

- No Unity Editor Pipeline connection was available. Scenes/prefabs were inventoried from serialized files and history; no live hierarchy, Inspector rendering, Play Mode, console, profiling, or physics behavior was inspected.
- No code tests or builds were run; this request was an architecture review and tests were not requested.
- The report maps all 100 runtime script files and all 9 first-party Editor scripts by responsibility. It does not inventory every imported asset, animation/controller, tile, sprite reference, audio clip, or every serialized object instance in 16 scenes and 73 prefabs.
- Ads/RevenueCat remote dashboard state and online services are outside this static code review.
- Existing player-facing localization and runtime behaviors may differ from source-level expectations; validate them in Unity/device before changing them.

## Analysis evidence

Repository evidence inspected: `AGENTS.md`; `FastTask/project.json` and `FastTask/tasks.json`; the current handoff and phase/GDD reports; `ProjectVersion.txt`; package manifest; graphics/build settings; all source-file names and declared types under `Assets/Scripts`; class APIs and project-code cross-references; detailed implementations of app flow, progression/persistence, player/physics, input/mobile, UI, ads/IAP, editor build automation, enemy/hazard/pickup systems; and Git branch/revision state.
