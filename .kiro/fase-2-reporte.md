# Fase 2 — Refactorización de arquitectura: Reporte de cambios

**Fecha:** 25/10/2026  
**Depende de:** Fase 0 y Fase 1 completadas.

---

## Resumen ejecutivo

Se atacaron los 4 problemas estructurales principales identificados en el análisis inicial:
1. **Persistencia dispersa** → `SaveManager` centralizado.
2. **`AudioMixerManager` God Class** → Limpiado, código muerto eliminado, responsabilidades delimitadas.
3. **`LevelManager` God Class** → Simplificado, guardado delegado a `SaveManager`.
4. **`AnalyticsManager` bug** → Eliminado el guardado ilimitado en `PlayerPrefs`.
5. **`ResetPosition.cs` vacío** → Implementado con funcionalidad real.

---

## Archivos creados

### `Assets/Scripts/Data/SaveManager.cs` (NUEVO)
**Qué hace:** Singleton con `DontDestroyOnLoad`. Capa única de acceso a `PlayerPrefs`. Ninguna otra clase debe llamar `PlayerPrefs` directamente.

**API:**
```csharp
SaveManager.Instance.HasSaveData()                          // ¿hay partida guardada?
SaveManager.Instance.IsLevelUnlocked(int level)            // ¿nivel desbloqueado?
SaveManager.Instance.UnlockNextLevel(int currentLevel)     // desbloquea nivel+1
SaveManager.Instance.SaveGemsIfBetter(int level, int gems) // guarda si es récord
SaveManager.Instance.SaveTimeIfBetter(int level, float t)  // guarda si es récord
SaveManager.Instance.LoadGems(int level)                   // carga gemas
SaveManager.Instance.LoadBestTime(int level)               // carga mejor tiempo
SaveManager.Instance.LoadMapIndex()                        // carga posición en mapa
SaveManager.Instance.LoadMusicEnabled() / LoadSFXEnabled() // configuración audio
SaveManager.Instance.SaveMusicEnabled(bool) / SaveSFXEnabled(bool)
SaveManager.Instance.ResetProgress(int totalLevels = 10)   // borra progreso (nueva partida)
```

**Nota:** `ResetProgress` preserva la configuración de audio al borrar todo.

**⚠️ Acción requerida en Unity:** `AppBootstrap` ahora tiene `[RequireComponent(typeof(SaveManager))]`. Al abrir la escena `MainMenu`, Unity lo agrega automáticamente al GameObject de `AppBootstrap`.

---

## Archivos modificados

### `Assets/Scripts/Core/LevelManager.cs`
**Cambios:**
- Eliminadas las 3 funciones privadas de `PlayerPrefs` (`SetPlayerPrefsLevelToUnlock`, `SetPlayerPrefsGems`, `SetPlayerPrefsTime`).
- Nuevo método privado `SaveProgress()` que delega a `SaveManager`.
- `timeInLevel` ahora solo avanza cuando `stopGame = false` (antes avanzaba siempre).
- Getter público `GetGemsCollected()` agregado.
- Nombres de coroutines renombrados a `RespawnRoutine` / `EndLevelRoutine` (más claros).
- `respawnMaxTime` renombrado a `respawnHoldTime` para mayor claridad.

---

### `Assets/Scripts/Core/MainMenuManager.cs`
**Cambios:**
- Usa `SaveManager.Instance.HasSaveData()` en lugar de `PlayerPrefs.HasKey("Level_1")`.
- Nueva partida usa `SaveManager.Instance.ResetProgress()` en lugar del bucle manual de `PlayerPrefs.DeleteAll()` + loops de escritura.
- Función privada `StartNewGame()` para evitar duplicación entre `ToNextScene()` y `ActivatePanel()`.

---

### `Assets/Scripts/LevelSelection/MapPointsHolder.cs`
**Cambios:**
- Lee datos de niveles vía `SaveManager` con fallback a `PlayerPrefs` si `SaveManager` no está disponible (para retrocompatibilidad durante transición).
- Código comentado extenso eliminado.
- Lógica dividida en `ConfigureMapPoints()` y `PositionPlayer()`.
- `Mathf.Clamp` en el índice del mapa para evitar out-of-bounds.

---

### `Assets/Scripts/Audio/AudioMixerManager.cs`
**Cambios importantes:**
- **Eliminados ~180 líneas de código comentado** (referencias a `SaveDataManager`, versiones antiguas de `CallButtonClick`, `SuccessSound`, `FailSound`, etc.).
- **Persistencia de audio** migrada a `SaveManager` (`LoadMusicEnabled/SFXEnabled`, `SaveMusicEnabled/SFXEnabled`).
- **Método `Start()`** ahora carga la configuración de audio al inicio en lugar de tenerla comentada.
- **Pitch de gemas** refactorizado: constantes nombradas (`GemPitchStart`, `GemPitchStep`, `GemPitchMax`) en lugar de campos magic-number.
- **Nombres de campos** normalizados: `pickup_helth_clip` → `pickup_health_clip`, `SmashImpact_Clip` → `smashImpact_Clip`.
- **Null checks** en `SFXElement.CallSFX()` y `SFXElement.SetPitch()`.
- **Bug corregido:** en el caso `Smash_Impact`, antes se llamaba `warpJingle_Clip.SetPitch(1)` en lugar de `smashImpact_Clip.SetPitch(1)`.
- Región `#region Not Used` con métodos de volumen por float eliminada.
- Región `#region In case of changes` renombrada a nombres descriptivos de métodos.

**⚠️ Acción requerida en Unity:** El campo `pickup_helth_clip` en el Inspector cambió de nombre a `pickup_health_clip`. Reasignar el AudioClip en el Inspector del GameObject de `AudioMixerManager` en cada escena que lo tenga.

---

### `Assets/Scripts/Managers/AnalyticsManager.cs`
**Cambios:**
- **Eliminado `SaveAnalyticsToPlayerPrefs()`** — este método creaba una clave nueva por evento con timestamp, acumulando miles de entradas en `PlayerPrefs` sin límite ni limpieza.
- Compilación condicional actualizada de `UNITY_ANALYTICS` a la API moderna de Unity Gaming Services (`Unity.Services.Analytics.AnalyticsService`).
- `LogDebug` usa `StringBuilder` en lugar de concatenación string.
- `[SerializeField]` privados eliminados de campos de estado (no necesitan mostrarse en el Inspector).
- Getter `GetCheckpointsReachedInCurrentLevel()` agregado.

---

### `Assets/Scripts/ResetPosition.cs`
**Cambios:**
- Archivo estaba completamente vacío. Implementado con funcionalidad útil: guarda posición/rotación inicial y las restaura en `Reset()` o automáticamente en `OnEnable`.

---

### `Assets/Scripts/Core/AppBootstrap.cs`
**Cambios:**
- Agregado `[RequireComponent(typeof(SaveManager))]`.

---

## Acciones requeridas en el Editor de Unity

| Acción | Dónde |
|--------|-------|
| Verificar que `SaveManager` está en el GameObject de `AppBootstrap` | Escena `MainMenu` |
| Reasignar `pickup_health_clip` en el Inspector | Escena de gameplay, componente `AudioMixerManager` |
| Verificar `smashImpact_Clip` (renombrado de `SmashImpact_Clip`) | Escena de gameplay, componente `AudioMixerManager` |

---

## Deuda técnica restante (no atacada en esta fase)

- `AudioMixerManager` sigue siendo un archivo grande (~270 líneas limpias). Para separarlo en `MusicManager` + `SFXManager` haría falta refactorizar todos los GameObjects que tienen el componente. Se puede hacer en el futuro si crece mucho.
- `LevelManager` sigue orquestando el respawn. Si el juego crece con más tipos de muerte/respawn, mover a `RespawnManager` separado (planificado para Fase 3 cuando lleguen las vidas).

---

## Cómo revertir

```bash
# Ver qué cambió
git diff HEAD --name-only

# Revertir archivos modificados
git checkout HEAD -- Assets/Scripts/Core/LevelManager.cs
git checkout HEAD -- Assets/Scripts/Core/MainMenuManager.cs
git checkout HEAD -- Assets/Scripts/LevelSelection/MapPointsHolder.cs
git checkout HEAD -- Assets/Scripts/Audio/AudioMixerManager.cs
git checkout HEAD -- Assets/Scripts/Managers/AnalyticsManager.cs
git checkout HEAD -- Assets/Scripts/ResetPosition.cs

# Eliminar archivos nuevos
# rm Assets/Scripts/Data/SaveManager.cs
```
