# Creating a level

Start from the template and register the new level in the places listed below. Steps 1–3 make it playable; steps 4–8 make it part of the world.

## 1. Copy the template

`Assets/Scenes/Templates/Level_Template.unity` is built from Level 1 with its content removed. It already holds the current base:

| Object | What it is |
|---|---|
| `LevelPrefab/UI-Canvas` | Current HUD prefab: hearts, lives, timer, gems, amber shards, pause (with Restart), Game Over, dice, settings. |
| `LevelPrefab/Main Camera` | Camera follow, parallax layers and screen shake. |
| `LevelPrefab/Level-Managers` | `LevelManager` (uses `WorldData_World1`) and the other level managers. |
| `LevelPrefab/Player` | The fox. Its start position is `InitialPoint`. |
| `LevelPrefab/Environment` | Tilemap grid; the template has a flat floor to replace with the level. |
| `LevelPrefab/Checkpoints-group` | One checkpoint; duplicate it along the level. |
| `LevelPrefab/Goal` | End of the level. |
| `LevelPrefab/DeathZone*` | Fall volumes; stretch them under the whole level. |
| `Collectibles (examples - move or delete)` | One of each collectible, to move or delete. |

Duplicate it to `Assets/Scenes/Levels/Level_N.unity` (Project window: select → Ctrl/Cmd+D → move and rename). The scene name must be exactly `Level_N`.

## 2. Build the level

- Paint terrain in `LevelPrefab/Environment` with the existing tile palette.
- Put level content (enemies, hazards, platforms, gems) under a root object named `Environment`, as the other levels do.
- Gems are counted automatically; no total to set.

## 3. Collectibles (Level Kit)

| Prefab | Path | Rules |
|---|---|---|
| Gem | `Assets/Prefabs/Gem.prefab` | Any number. All of them = perfect gems badge. |
| Clover | `Assets/Prefabs/TrebolPickup.prefab` | One per level, hidden on an optional route (not next to the Goal). |
| Heart fragment | `Assets/Prefabs/HeartFragmentPickup.prefab` | At most one per level, hidden. |
| Amber shard | `Assets/Prefabs/LevelKit/AmberShard.prefab` | Any number. Each placed shard gets a unique id automatically, also when duplicated. Counted in the HUD and on the map. |
| Amber Heart | `Assets/Prefabs/LevelKit/AmberHeart.prefab` | Only in the final level of a world (currently Level 10). It is disabled in the template: enable it there. Collecting it is saved and announced; the level still ends at the Goal. |

Enemies are in `Assets/Prefabs/Enemies/` and hazards in `Assets/Prefabs/Environment/`. See [`AI/ENEMY_ROSTER_PLAN.md`](AI/ENEMY_ROSTER_PLAN.md).

## 4. Builds

- **`Assets/Editor/BuildAutomation.cs` → `RequiredScenes`**: player builds use this list, not the Editor Build Settings. Levels 9 and 10 are already listed; add any new level here.
- **Editor Build Settings** (File → Build Profiles / Build Settings): add the scene so it also loads when testing in the Editor. Levels 9 and 10 are not there yet.

## 5. World data

`Assets/Data/WorldData_World1.asset`:

- `levelSceneNames`: add `Level_N` in order.
- `targetTimes`: add the target time in seconds at the same index (it drives the time star and the HUD timer colour).

## 6. Level selector map

In `Assets/Scenes/LevelSelection.unity`:

- Add a `MapPoint` for the level (copy an existing one): `isLevel` on, `currentLevel = N`, `levelName`, `totalGems`, and its `up/down/left/right` neighbours (also update the previous point's link).
- Add it to `MapPointsHolder` → `_mapPoints`, in order.
- Paint the level sign tile on the `Over` tilemap under the point. The overworld sheet has signs 1–10; the gold copy for no-damage clears is picked automatically.

## 7. Texts

In the `Game UI` localization table, add `LEVEL_NAME_N` (en and es). Without it the map shows the `MapPoint.levelName`.

## 8. World length

`Assets/Scripts/Systems/Achievements.cs` → `WorldLevelCount` is **8** and defines "last level of World 1", "all clovers" and "all perfect" goals. When levels 9 and 10 are playable, set it to **10** (doing it earlier makes those achievements unreachable).

## Check before committing

- Play the level from the map (not only from the scene) so the managers from MainMenu are loaded.
- Reach the clover, the heart fragment and every amber shard.
- Review the scene diff: saving a level can rewrite unrelated UI-Canvas overrides; commit only the intended changes.
