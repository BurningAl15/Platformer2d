# Fase 3 — Economía del juego: Reporte de cambios (COMPLETO)

**Fecha:** 26/10/2026
**Depende de:** Fase 0, 1 y 2 completadas.

---

## Resumen ejecutivo

Sistema de economía completo implementado:
1. **Vidas globales** — 3 por defecto, persist entre niveles, Game Over al agotar
2. **Game Over con 3 opciones** — ad (simulado), trébol+dado, volver al mapa
3. **Trébol 🍀** — 1 por nivel, persiste permanentemente, mejora el dado
4. **Fragmento de Dado Secreto 🎲** — en zonas secretas, se ensambla por mundo
5. **DiceRollController** — dado animado con 4 tipos (Normal/Mejorado A-B/Secreto)
6. **Estrellas por nivel** — integradas al fin de nivel y al `MapPoint`
7. **HUD completo** — gemas `X/Total`, vidas, tréboles

---

## Archivos creados

### `Assets/Scripts/Systems/LivesManager.cs`
Singleton con DontDestroyOnLoad. Vidas globales. Eventos `OnGameOver` / `OnLivesChanged`.
Métodos: `ConsumeLifeAndCheck()`, `AddLivesFromAd()`, `SpendGemsForLife()`, `CanAffordLife()`, `NotifyLivesChanged()`.

### `Assets/Scripts/UI/GameOverController.cs`
Pantalla Game Over con slide DOTween. 3 opciones:
- Ad (simulado, TODO Fase 4)
- Trébol → `DiceRollController.ShowFromGameOver()` → respawn tras tirada
- Volver al mapa → reset a 3 vidas

### `Assets/Scripts/Environment/Interactables/TrebolPickup.cs`
Coleccionable trébol. Aparece translúcido si ya fue recogido. Llama `SaveManager.SetTrebolCollected()` y `UIController.RefreshTrebolUI()`.

### `Assets/Scripts/Environment/Interactables/DiceFragmentPickup.cs`
Coleccionable fragmento de dado secreto. Cuenta fragmentos por mundo en `SaveManager`. Aparece translúcido si ya fue recogido.

### `Assets/Scripts/Systems/DiceRollController.cs`
Controlador de la pantalla del dado. Animación de números girando con `WaitForSecondsRealtime`. Botón de parar. Dos modos:
- `Show()` → desde fin de nivel → al cerrar va al mapa
- `ShowFromGameOver(type, callback)` → desde Game Over → al cerrar llama callback y respawnea

### `Assets/Scripts/Data/SaveManager.cs` (extendido)
Secciones nuevas: vidas, gemas globales, rating de estrellas, tréboles, fragmentos de dado secreto.
API de tréboles: `HasTrebol()`, `SetTrebolCollected()`, `LoadTotalTrebols()`, `AddTrebols()`, `SpendTrebol()`.
API de fragmentos: `HasDiceFragment()`, `SetDiceFragmentCollected()`, `GetDiceFragmentsCount()`, `IsSecretDiceUnlocked()`.

---

## Archivos modificados

### `Assets/Scripts/Core/LevelManager.cs`
- Auto-detecta total de gemas con `FindObjectsOfType<Pickup>()` si no está configurado manualmente
- `UpdateGems_UI(collected, total)` — formato `X / Total`
- `EndLevelRoutine` — si `gemsCollected >= totalGemsInLevel` muestra el dado antes de ir al mapa
- `SaveProgress()` retorna `LevelRating` para pasarlo al contexto del dado
- `RespawnPlayer()` consulta `LivesManager.ConsumeLifeAndCheck()` antes de respawnear

### `Assets/Scripts/UI/UIController.cs`
- `UpdateGems_UI(int current, int total)` — acepta total como parámetro opcional
- Campo `trebolCountText` (TextMeshProUGUI, opcional)
- `RefreshTrebolUI()` — actualiza contador de tréboles
- `ShowLevelRating(LevelRating)` — muestra estrellas al fin de nivel
- Eliminado método `RefreshTrebolUI()` duplicado (bug de compilación)

### `Assets/Scripts/Systems/LivesManager.cs`
- `NotifyLivesChanged()` — dispara `OnLivesChanged` manualmente (usado por `DiceRollController`)

### `Assets/Scripts/LevelSelection/MapPoint.cs`
- Campo `hasTrebol` (bool)
- Campo `trebolBadge` (GameObject, serializado)
- Método `CheckTrebol()` activa/desactiva el badge

### `Assets/Scripts/LevelSelection/MapPointsHolder.cs`
- Carga `hasTrebol` desde `SaveManager` y llama `CheckTrebol()` por cada MapPoint

### `Assets/Scripts/Core/AppBootstrap.cs`
- `[RequireComponent(typeof(LivesManager))]` agregado

---

## Tipos de dado implementados

| Tipo | Rango | Cómo se obtiene |
|------|-------|----------------|
| `DiceType.Normal` | 1–3 | Por defecto (10/10 gemas) |
| `DiceType.Upgraded_3_5` | 3–5 | Gasto de trébol (50% probabilidad) |
| `DiceType.Upgraded_2_6` | 2–6 | Gasto de trébol (50% probabilidad) |
| `DiceType.Secret` | 4–6 | Fragmentos ensamblados (TBD en niveles) |

---

## Acciones requeridas en Unity Editor (para que funcione)

| Acción | Dónde |
|--------|-------|
| Verificar `LivesManager` en `AppBootstrap` (auto por `[RequireComponent]`) | Escena `MainMenu` |
| Crear panel Game Over + asignar `GameOverController` con 4 botones y textos | Escena gameplay |
| Agregar `DiceRollController` a un GameObject del Canvas en gameplay | Escena gameplay |
| Asignar `trebolCountText`, `livesText` en `UIController` | Inspector gameplay |
| Asignar `starFull`, `starEmpty`, `starImages` (3 imágenes) en `UIController` | Inspector gameplay |
| Asignar `trebolBadge` en cada `MapPoint` (GameObject de ícono, desactivado por defecto) | Escena `LevelSelection` |
| Agregar prefab `TrebolPickup` en los niveles | Escenas `Level_N` |
| Agregar prefab `DiceFragmentPickup` en zonas secretas (cuando estén diseñadas) | Escenas `Level_N` difíciles |
| Crear prefabs de `TrebolPickup` y `DiceFragmentPickup` con sprites propios | Assets → Antigravity |

---

## Pendiente para Antigravity (solo assets, el código está listo)

- Sprite del Trébol 🍀 (normal, translúcido "ya obtenido", efecto de recolección)
- Sprite del Fragmento de Dado 🎲 (en nivel y en HUD)
- Sprites de los 3 dados (Normal, Mejorado, Secreto)
- Animación de números girando en la pantalla del dado
- Layout visual completo del panel del dado
- Layout visual del panel de Game Over
- Sprites de estrella llena / vacía
- Badge de trébol para `MapPoint`
- HUD actualizado con slots de vidas y tréboles

---

## Cómo revertir

```bash
git diff HEAD --name-only
git log --oneline -5
git revert <commit-hash>  # o
git reset --soft HEAD~1   # solo si no fue pusheado
```
