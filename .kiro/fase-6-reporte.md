# Fase 6 — Expansión de contenido (múltiples mundos): Reporte de cambios

**Fecha:** 26/10/2026
**Depende de:** Fases 0-5 completadas.

---

## Resumen ejecutivo

Se implementó la arquitectura base para múltiples mundos sin romper el MVP actual. El `MapPointsHolder` ahora tiene dos modos: legacy (1 mundo, como antes) y multi-mundo (usando `WorldData` ScriptableObjects). El MVP sigue funcionando igual — para agregar nuevos mundos solo hay que crear un `WorldData` y asignarlo en el Inspector.

---

## Archivos creados

### `Assets/Scripts/Data/WorldData.cs` (NUEVO — ScriptableObject)
Define un mundo: nombre, número, lista de escenas, condición de desbloqueo, totales de gemas/tréboles/fragmentos.

**Crear un nuevo mundo:**
Assets → Create → Fox's Tale → World Data

**Campos clave:**
```
worldName           "Mundo 1 — El Bosque"
worldNumber         1
levelSceneNames     ["Level_0", "Level_1", ..., "Level_10"]
starsRequiredToUnlock  0 (primer mundo) / N estrellas del mundo anterior
totalDiceFragments  3
```

### `Assets/Scripts/Data/WorldProgressManager.cs` (NUEVO)
Singleton con DontDestroyOnLoad. Calcula progreso por mundo a partir de los datos de `SaveManager`.

**API:**
```csharp
WorldProgressManager.Instance.GetTotalStarsInWorld(WorldData)  → int
WorldProgressManager.Instance.IsWorldUnlocked(world, prevWorld) → bool
WorldProgressManager.Instance.IsWorldComplete(world)           → bool
WorldProgressManager.Instance.GetGemsCollectedInWorld(world)   → int
WorldProgressManager.Instance.GetTrebolsInWorld(world)         → int
```

---

## Archivos modificados

### `Assets/Scripts/LevelSelection/MapPointsHolder.cs`
**Nuevo campo:** `[SerializeField] private WorldData worldData`

**Dos modos de operación:**
- `worldData = null` → modo legacy (igual que antes, índice secuencial 1, 2, 3...)
- `worldData` asignado → lee los nombres de escena desde el ScriptableObject y extrae el número de nivel

El MVP actual sigue funcionando sin cambios — `worldData` empieza vacío.

### `Assets/Scripts/Core/AppBootstrap.cs`
- `[RequireComponent(typeof(WorldProgressManager))]` agregado

---

## Cómo agregar un nuevo mundo (flujo futuro)

1. Assets → Create → Fox's Tale → World Data → configurar nombre, escenas, condición de desbloqueo
2. Crear una nueva escena de mapa de selección (duplicar `LevelSelection.unity`) o agregar una sección al mapa existente
3. En el `MapPointsHolder` de esa sección, asignar el nuevo `WorldData`
4. Crear los niveles (`Level_11`, `Level_12`, etc.) y agregar las escenas a Build Settings
5. La pantalla de "Mundo completado" (pendiente) se construirá sobre `WorldProgressManager.IsWorldComplete()`

---

## Pendiente para implementar (futuro)

- [ ] Pantalla de "Mundo completado" con resumen de estrellas, gemas y tréboles
- [ ] Navegación entre mundos en la pantalla de selección
- [ ] Condición de desbloqueo visual en el mapa (mostrar cuántas estrellas faltan)
- [ ] Crear el `WorldData` para el Mundo 1 actual con las escenas existentes

---

## Cómo revertir

```bash
git checkout HEAD -- Assets/Scripts/LevelSelection/MapPointsHolder.cs
git checkout HEAD -- Assets/Scripts/Core/AppBootstrap.cs
# rm Assets/Scripts/Data/WorldData.cs
# rm Assets/Scripts/Data/WorldProgressManager.cs
```
