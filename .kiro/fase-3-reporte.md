# Fase 3 — Economía del juego: Reporte de cambios

**Fecha:** 25/10/2026  
**Depende de:** Fase 0, 1 y 2 completadas.

---

## Resumen ejecutivo

Se implementó la economía completa del juego:
1. **Sistema de vidas global** — persiste entre niveles, se consume al morir.
2. **Wallet de gemas globales** — se acumula al completar niveles.
3. **Pantalla de Game Over** — con opciones de continuar (ad simulado / gemas / volver al mapa).
4. **Sistema de estrellas integrado** — `LevelRatingSystem` ahora se llama al fin de nivel.
5. **UIController ampliado** — HUD de vidas, gemas globales y estrellas.

---

## Archivos creados

### `Assets/Scripts/Systems/LivesManager.cs` (NUEVO)
**Qué hace:** Singleton con DontDestroyOnLoad. Gestiona vidas globales del jugador.

**API:**
```csharp
LivesManager.Instance.CurrentLives                   // vidas actuales
LivesManager.Instance.ConsumeLifeAndCheck()          // consume vida → retorna true si puede continuar
LivesManager.Instance.AddLivesFromAd()               // +2 vidas (rewarded ad)
LivesManager.Instance.SpendGemsForLife()             // gasta gemas → +1 vida
LivesManager.Instance.CanAffordLife()                // ¿tiene gemas suficientes?
event OnGameOver                                     // disparado cuando vidas = 0
event OnLivesChanged(int)                            // disparado al cambiar vidas
```

**Configuración en Inspector:**
- `gemCostPerLife` = 10 (ajustar según diseño)
- `livesPerAd` = 2 (ajustar según diseño)

**⚠️ Acción requerida en Unity:** `AppBootstrap` ahora tiene `[RequireComponent(typeof(LivesManager))]`. Se agrega automáticamente.

---

### `Assets/Scripts/UI/GameOverController.cs` (NUEVO)
**Qué hace:** Pantalla de Game Over con animación DOTween slide in/out.

**Opciones al jugador:**
1. "Ver Ad" → `LivesManager.AddLivesFromAd()` → continuar. *(Ad simulado — real en Fase 4)*
2. "Usar gemas" → `LivesManager.SpendGemsForLife()` → continuar. El botón se deshabilita si no hay gemas suficientes.
3. "Volver al mapa" → resetea vidas al valor por defecto → carga `LevelSelection`.

**⚠️ Acción requerida en Unity:**
- Crear un Canvas/CanvasGroup en la escena de gameplay con el panel de Game Over.
- Agregar este script al panel.
- Asignar en Inspector: `panelGroup`, `panelRect`, 3 botones, `gemCostText`, `currentGemsText`.
- El panel debe empezar con `alpha = 0`, `interactable = false`.
- `firstSelectedButton` debe apuntar al botón de "Ver Ad" para accesibilidad con gamepad.

---

## Archivos modificados

### `Assets/Scripts/Data/SaveManager.cs`
**Cambios:**
- Nuevas constantes: `KEY_GLOBAL_GEMS`, `KEY_LIVES`, `DEFAULT_LIVES = 3`, `MAX_LIVES = 9`.
- Nuevos métodos de **gemas globales**: `LoadGlobalGems()`, `AddGlobalGems(int)`, `SpendGlobalGems(int)`.
- Nuevos métodos de **vidas**: `LoadLives()`, `SaveLives(int)`, `ConsumeLife()`, `AddLives(int)`, `GetMaxLives()`, `GetDefaultLives()`.
- Nuevo método de **rating**: `LoadRating(int)`, `SaveRatingIfBetter(int, int)`.
- `ResetProgress()` ahora también inicializa `SaveLives(DEFAULT_LIVES)` y `GlobalGems = 0`.

---

### `Assets/Scripts/Core/LevelManager.cs`
**Cambios en `RespawnPlayer()`:**
- Antes: siempre iniciaba la coroutine de respawn.
- Ahora: llama `LivesManager.Instance.ConsumeLifeAndCheck()`. Si retorna `false`, no respawnea (LivesManager disparó `OnGameOver` y GameOverController lo maneja).

**Cambios en `SaveProgress()`:**
- Agrega las gemas del nivel al wallet global: `SaveManager.Instance.AddGlobalGems(gemsCollected)`.
- Calcula rating con `LevelRatingSystem._instance.CalculateRating(...)`.
- Guarda el rating con `SaveManager.Instance.SaveRatingIfBetter(...)`.
- Llama `UIController._instance.ShowLevelRating(rating)`.

**⚠️ Pendiente (nota de diseño):** El cálculo del rating usa `totalGems = gemsCollected` como placeholder. Para que 2 estrellas requiera recolectar TODAS las gemas del nivel, `LevelManager` necesita saber cuántas hay en total. Por ahora el rating de 2 estrellas solo se alcanza si `gemsCollected >= totalGems` donde `totalGems` viene del `MapPoint`. En el futuro conectar esto con el `Editor/CountGemsInLevel.cs` que ya existe.

---

### `Assets/Scripts/Systems/LevelRatingSystem.cs`
**Cambios:**
- Agregado Singleton `_instance`.
- Nuevo método estático `GetStarStates(LevelRating, int)` que retorna un array de bools para renderizar en UI.
- Null checks en `CalculateRating` para `req.totalGems = 0` y `req.targetTime = 0`.

---

### `Assets/Scripts/UI/UIController.cs`
**Cambios:**
- Nuevo campo `gemsGlobalText` — texto del wallet de gemas en el HUD (opcional).
- Nuevo campo `livesText` — texto de vidas restantes en el HUD (opcional).
- Nuevos campos `starImages`, `starFull`, `starEmpty` — para las 3 estrellas al fin de nivel.
- Nuevo método `ShowLevelRating(LevelRating)` — actualiza los sprites de estrellas.
- Nuevo método `RefreshLivesUI()` — actualiza el contador de vidas.
- Nuevo método `RefreshGlobalGemsUI()` — actualiza el wallet de gemas.
- Se suscribe a `LivesManager.OnLivesChanged` en `Start()` para actualización automática.

**⚠️ Acción requerida en Unity:** Asignar en el Inspector los nuevos campos opcionales (`gemsGlobalText`, `livesText`, `starImages`, `starFull`, `starEmpty`). El juego funciona sin asignarlos, pero el HUD no mostrará esos elementos.

---

### `Assets/Scripts/Environment/Interactables/Pickup.cs`
**Cambios:**
- Comentario explicativo: las gemas se acumulan al COMPLETAR el nivel (en `LevelManager.SaveProgress()`), no al recolectarlas. Esto es intencional: incentiva terminar el nivel.

---

### `Assets/Scripts/Core/AppBootstrap.cs`
**Cambios:**
- `[RequireComponent(typeof(LivesManager))]` agregado.

---

## Acciones requeridas en el Editor de Unity

| Acción | Dónde |
|--------|-------|
| Verificar que `LivesManager` se agrega automáticamente a `AppBootstrap` | Escena `MainMenu` |
| Crear panel de Game Over con `GameOverController` | Escena de gameplay |
| Asignar `panelGroup`, `panelRect`, botones, textos de economía en `GameOverController` | Inspector |
| Asignar `gemCostPerLife` y `livesPerAd` en `LivesManager` según diseño | Inspector de `AppBootstrap` |
| Asignar `gemsGlobalText`, `livesText`, `starImages` en `UIController` (opcional) | Inspector |
| Crear sprites de estrella llena y vacía, asignarlos en `UIController` | Assets/Sprites |
| Agregar `LevelRatingSystem` a algún GameObject de la escena de gameplay | Escena de gameplay |

---

## Decisiones de diseño pendientes

- [ ] **Coste de vida en gemas:** actualmente `gemCostPerLife = 10`. Revisar y ajustar.
- [ ] **Vidas iniciales:** actualmente `DEFAULT_LIVES = 3`. Revisar.
- [ ] **Total de gemas por nivel:** `LevelRatingSystem` necesita el `totalGems` de cada nivel para el rating de 2 estrellas. Conectar con `CountGemsInLevel.cs` (editor) o con el campo `totalGems` de `MapPoint`.
- [ ] **Ad simulado:** `GameOverController.OnWatchAdPressed()` tiene un `// TODO Fase 4`. Al implementar ads, reemplazar la llamada directa con el SDK real.

---

## Cómo revertir

```bash
git diff HEAD --name-only
git checkout HEAD -- Assets/Scripts/Core/LevelManager.cs
git checkout HEAD -- Assets/Scripts/Data/SaveManager.cs
git checkout HEAD -- Assets/Scripts/Systems/LevelRatingSystem.cs
git checkout HEAD -- Assets/Scripts/UI/UIController.cs
git checkout HEAD -- Assets/Scripts/Environment/Interactables/Pickup.cs
git checkout HEAD -- Assets/Scripts/Core/AppBootstrap.cs
# Eliminar nuevos:
# rm Assets/Scripts/Systems/LivesManager.cs
# rm Assets/Scripts/UI/GameOverController.cs
```
