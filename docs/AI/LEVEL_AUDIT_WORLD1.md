# World 1 level audit

**Date:** 2026-11-19 · **Branch:** `codex/fix-horizontal-platform-riding` (`a547a87`)
**Method:** each `Level_N.unity` was opened in the Editor and its components counted by type and position (start = player spawn, goal = `Goal`). Lengths are in world units along X. Nothing was played or changed.

## Per level

| Level | Length (X) | Height span | Gems | Checkpoints (x) | Enemies | Main obstacles | New in this level |
|---|---:|---:|---:|---|---|---|---|
| 0 (tutorial) | 197 | 16 | 13 | 41, 70, 108, 150 | 1 patrol, 1 tutorial enemy | 1 moving platform, spawners | Basics |
| 1 | 173 | 33 | 17 | 75, 109 | 5 patrol | 6 vertical moving platforms, 5 damage zones | Patrol enemies, vertical platforms |
| 2 | 211 | 37 | 27 | 25, 66, 144 | 2 patrol, 2 flying | 13 smashers, 1 bouncer, spawners | Flying enemies, smashers, bouncer, spawners (4 at once) |
| 3 | 115 | 31 | 14 | 46, 85 | none | 18 falling logs, 6 smashers, 16 spawners | Falling logs |
| 4 | 234 | 50 | 29 | 78, 148 | none | 36 falling logs, 10 smashers, 8 bouncers | Switch |
| 5 | 224 | 49 | 19 | 86 | none | 54 falling logs, 18 rotating + 10 moving edges, 2 pendulums | Rotating/moving edges and pendulum, both within the first 34 units |
| 6 | 103 up (vertical) | 169 | 30 | 20, 37 | none | 7 moving platforms, 23 moving/rotating edges | Vertical climb |
| 7 | 138 | 56 | 25 | 44 | none | 36 falling logs, 15 edges, 6 smashers | Mix |
| 8 (boss) | 116 | 74 | 10 | 19 | Tank boss | Switch, edges | Boss |
| 9 | — | — | 0 | 77, 83, 91 | none | none | Empty template |
| 10 | — | — | 0 | 77, 83, 91 | 3 flying | 7 bouncers, 4 moving platforms | Unfinished |

## Findings

1. **The clover and the heart fragment sit next to the goal in every level** (clover at goal −3 / +2 units; heart fragment at the goal in levels 1–6). They are not hidden, so the clover economy, the "hidden clover" achievement and the dice upgrades have no exploration challenge.
2. **Enemies disappear after level 2.** Levels 3–7 have none. `RunningEnemy` and `Dart`, plus the `FallingSpike`, `HoldingDamageBall` and `HangingHazzard` hazards, exist in code but are used in no level.
3. **Falling logs dominate:** 18 → 36 → 54 → 36 in levels 3, 4, 5 and 7; the same obstacle leads four levels.
4. **Mechanic introduction is uneven:** level 2 introduces four new things at once; level 5 starts with rotating edges and a pendulum in its first 34 units, without a safe place to learn them.
5. **Checkpoint spacing:** levels 5 and 7 have a single checkpoint over 224 and 138 units; the tutorial has four.
6. **Length curve:** level 3 (115) follows level 2 (211) and precedes level 4 (234).
7. **The player's abilities never change** across the eight levels (run, jump, double jump, stomp).

## Quick wins (no full redesign)

- Move each clover and heart fragment to an optional, harder route or a secret area (one per level).
- Add a checkpoint near the middle of levels 5 and 7.
- Place the unused enemies (runner, dart) and hazards on flat stretches of levels 3–7.

## Possible "one level, one idea" order for World 1

L1 patrols and basic jumps · L2 flying enemies and vertical platforms · L3 smashers · L4 falling logs and bouncers · L5 switches and spawners · L6 rotating edges (vertical) · L7 pendulums and a final mix · L8 boss.
