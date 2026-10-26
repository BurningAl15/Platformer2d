# Fase 0 — Estabilización: Reporte de cambios

**Fecha:** 25/10/2026  
**Branch aplicado en:** el branch activo al momento de ejecutar (se recomienda committear antes de la Fase 1)

---

## Resumen ejecutivo

Se corrigieron 6 problemas de estabilidad que existían antes de agregar nuevas features:
1. Transiciones de escena sin fade
2. `DOTween.Init()` duplicado en múltiples clases
3. Singletons frágiles (sin protección contra duplicados)
4. `SceneUtils.ParseLevelNumber` lanzaba excepción en vez de retornar error
5. Feedback visual y sonoro ausente en el mapa de selección
6. Bug de timing en la pausa (`Time.timeScale` se aplicaba tarde)

---

## Archivos creados

### `Assets/Scripts/Core/SceneTransitionManager.cs` (NUEVO)
**Qué hace:** Singleton con `DontDestroyOnLoad` que envuelve todas las cargas de escena con un fade out del `FadeEffect` antes de cargar. Si no hay `FadeEffect` en la escena, hace un fallback con `WaitForSeconds`.

**API pública:**
```csharp
SceneTransitionManager.Instance.LoadScene("NombreEscena");
SceneTransitionManager.Instance.LoadGameplayScene(int level);
SceneTransitionManager.Instance.ToSelectionScene();
SceneTransitionManager.Instance.ToMainScene();
```

**⚠️ Acción requerida en Unity:** Agregar un GameObject llamado `SceneTransitionManager` con este componente en la escena `MainMenu`. Como tiene `DontDestroyOnLoad`, persistirá a través de todas las escenas.

---

### `Assets/Scripts/Core/AppBootstrap.cs` (NUEVO)
**Qué hace:** Singleton con `DontDestroyOnLoad`. Centraliza la inicialización única de DOTween y la configuración de framerate (que antes estaban duplicadas en `PauseMenu`, `MainMenuManager` y `LevelManager`).

**⚠️ Acción requerida en Unity:** Agregar un GameObject llamado `AppBootstrap` con este componente en la escena `MainMenu`. Debe ser el primero en ejecutarse (ajustar Script Execution Order si hay problemas).

---

## Archivos modificados

### `Assets/Scripts/Utils/SceneUtils.cs`
**Cambios:**
- Todas las funciones de carga ahora delegan a `SceneTransitionManager` cuando está disponible.
- `Get_CurrentLevelName()` y `Get_NextLevelName()` usan `int.TryParse()` en lugar de `Int32.Parse()`. Ya no lanzan excepción — retornan `-1` si el nombre no tiene el formato `Level_N`.
- `IsInGameplay()` usa `StartsWith("Level_")` en lugar de `Contains("Level_")` para evitar falsos positivos.

**Antes:**  
```csharp
string[] subs = tempSceneName.Split(separator);
sceneNumber = Int32.Parse(subs[1]); // ← lanza excepción si falla
```
**Después:**  
```csharp
if (parts.Length >= 2 && int.TryParse(parts[1], out int number))
    return number;
return -1; // ← retorna -1, loguea warning
```

---

### `Assets/Scripts/UI/PauseMenu.cs`
**Cambios:**
- Eliminado `DOTween.Init()` del `Awake()` — ahora lo hace `AppBootstrap`.
- Corregido bug de timing: `Time.timeScale = 0` ahora se aplica **inmediatamente** al pausar, no al terminar la animación.
- Al reanudar, `Time.timeScale = 1` se aplica en el `OnComplete` del tween (correcto).
- Agregado `DOKill()` antes de iniciar tweens para evitar conflictos si el jugador pausa/despausa rápidamente.
- Nuevo método `ForceResume()` para transiciones de escena desde el menú de pausa.
- Singleton con protección contra duplicados.

---

### `Assets/Scripts/Core/MainMenuManager.cs`
**Cambios:**
- Eliminado `DOTween.Init()` del `Awake()` — ahora lo hace `AppBootstrap`.
- Singleton con protección contra duplicados.

---

### `Assets/Scripts/Core/LevelManager.cs`
**Cambios:**
- Eliminada la configuración de framerate (`QualitySettings.vSyncCount`, `Application.targetFrameRate`) del `Awake()` — ahora lo hace `AppBootstrap`.
- Singleton con protección contra duplicados.

---

### `Assets/Scripts/Core/CheckpointManager.cs`
**Cambios:**
- Singleton con protección contra duplicados.

---

### `Assets/Scripts/Core/LevelSelectManager.cs`
**Cambios:**
- Singleton con protección contra duplicados.

---

### `Assets/Scripts/Player/PlayerController2d.cs`
**Cambios:**
- Singleton con protección contra duplicados.

---

### `Assets/Scripts/Player/PlayerHealth.cs`
**Cambios:**
- Singleton con protección contra duplicados.

---

### `Assets/Scripts/Camera/SimpleCameraController2d.cs`
**Cambios:**
- Singleton con protección contra duplicados.

---

### `Assets/Scripts/Systems/ComboManager.cs`
**Cambios:**
- Singleton con protección contra duplicados.

---

### `Assets/Scripts/UI/UIController.cs`
**Cambios:**
- Singleton con protección contra duplicados.
- En `Start()`, ahora resetea explícitamente `endLevel_animator` y `amazingJob_animator` a inactivo (fix para cuando el jugador recarga el nivel sin pasar por `Awake`).

---

### `Assets/Scripts/UI/UI_SelectController.cs`
**Cambios:**
- Panel de info del nivel ahora usa DOTween para slide in/out (antes era `SetActive` directo sin animación).
- Variables configurables en el Inspector: `hiddenY`, `visibleY`, `animDuration`, `showEase`, `hideEase`.
- Singleton con protección contra duplicados.

**⚠️ Acción requerida en Unity:** Verificar que los valores de `hiddenY` y `visibleY` en el Inspector correspondan a las posiciones anchoradas del panel en el Canvas.

---

### `Assets/Scripts/LevelSelection/MapPoint.cs`
**Cambios:**
- Nuevos métodos `Select()` y `Deselect()` con animación DOTween de escala.
- Variables configurables en el Inspector: `normalScale`, `selectedScale`, `scaleDuration`.

---

### `Assets/Scripts/LevelSelection/LSPlayerController.cs`
**Cambios:**
- Llama `currentPoint.Select()` / `currentPoint.Deselect()` al navegar (feedback visual del nodo activo).
- `SetCurrentPoint()` deselecciona el punto anterior antes de seleccionar el nuevo.
- Al intentar entrar a un nivel bloqueado: reproduce un sonido de "no disponible" (`Warp_Jingle`) en lugar de no hacer nada.
- Sonido de selección de nivel movido a `HandleLevelSelect()` (antes estaba solo en `LevelSelectManager`).
- Eliminados `Debug.Log` de navegación (se mantienen solo los de nodo bloqueado).

---

## Acciones requeridas en el Editor de Unity

| Acción | Dónde |
|--------|-------|
| Crear GameObject `AppBootstrap` con el script `AppBootstrap` | Escena `MainMenu` |
| Crear GameObject `SceneTransitionManager` con el script `SceneTransitionManager` | Escena `MainMenu` |
| Verificar posiciones `hiddenY` / `visibleY` del panel de info | Escena `LevelSelection`, componente `UI_SelectController` |
| Verificar que `MapPoint` tenga valores de `normalScale` / `selectedScale` correctos | Todos los `MapPoint` en la escena `LevelSelection` |
| Verificar `initPoint` / `endPoint` del `PauseMenu` siguen siendo válidos | Escena de gameplay, componente `PauseMenu` |

---

## Lo que NO se cambió en esta fase

- La lógica de gameplay (movimiento, salto, enemigos) — sin tocar.
- `AudioMixerManager` — God Class pendiente para Fase 2 (refactorización).
- `MobileInputProvider` — stub pendiente para Fase 1 (controles).
- El sistema de guardado con `PlayerPrefs` — pendiente para Fase 2.
- `AnalyticsManager` — el bug de acumulación de claves queda pendiente para Fase 2.

---

## Cómo revertir

Todos los archivos originales siguen en git. Para revertir esta fase completa:
```bash
git diff HEAD --name-only   # ver qué archivos cambiaron
git checkout HEAD -- <archivo>  # revertir un archivo específico
# o para revertir todo:
git stash  # si no hay commit
git revert <commit-hash>  # si ya hay commit
```
