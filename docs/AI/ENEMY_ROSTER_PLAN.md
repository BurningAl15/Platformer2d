# Enemy roster plan

**Date:** 2026-11-19 · **Status:** documented only (owner decision). No enemies were added or moved.
Related: [`LEVEL_AUDIT_WORLD1.md`](LEVEL_AUDIT_WORLD1.md).

## Principle

Variety comes from behaviours, not creature count: each enemy should ask the player for a different response. A new creature that only patrols is the same enemy with a new sprite. Aim for about four enemies per world (existing ones plus one or two new behaviours) and a boss, which gives roughly 14–16 behaviours across four worlds.

## Current roster

| Enemy | Prefab | Script | Behaviour | Player response |
|---|---|---|---|---|
| Opossum / rat | `Rat-Enemy`, `Rat-Cave`, `Rat-Cave-Left` | `EnemyController` / `RunningEnemy` | Patrols or runs | Stomp timing |
| Frog | `Frog-Enemy` | `EnemyController` | Hops between `LeftPoint` and `RightPoint` (~9 units) | Read the hop rhythm |
| Eagle | `Eagle-Container` | `FlyingEnemyController` | Flies between `Point1` and `Point2` (~16 units) | Dodge the dive |
| Bat | `Bat-Container-Vertical`, `Bat-Container-Vertical-Horizontal` | `FlyingEnemyController` | Vertical or mixed flight | Route around it |
| Dart trap | `Dart-ToLeft`, `Dart-ToRight` | `Dart` | Shoots horizontally | Pass between shots |
| Tank boss | `BossBattle` | `BossTankController` | Boss with mines and a switch | Boss fight |

Unused in any level: `RunningEnemy`, `Dart`, `FallingSpike`, `HoldingDamageBall`, `HangingHazzard`.

## Code foundation

New enemies need only their behaviour: `SpawnFactory` is the single creation and reuse point, and `EnemyParent` (implements `ISpawnable`) handles death feedback, collider restore and the `Died` event. A new enemy is a subclass of `EnemyParent` plus a prefab.

## Proposed new archetypes

| Archetype | Teaches | World |
|---|---|---|
| Hedgehog (spikes up) | Not everything can be stomped; avoid it or wait until it uncurls | 1 |
| Seed-spitting plant (arcing shots) | Zone control; time the crossing | 1 |
| Spider on a thread | Vertical, rhythmic danger | 2 (caves) |
| Mole that pops out of the ground | The floor is not always safe | 2 |
| Shell crab | Stomp it, then kick the shell to break blocks or hit enemies | 2 (puzzles) |
| Stone guardian | Wakes and charges on sight; stunned when it hits a wall | 3 (ruins) |
| Wall lizard | Runs along walls and ceilings | 3 |
| Bird formations | Coordinated air patterns | 4 (peak) |
| Charging goat | Knocks the player back; dangerous near drops | 4 |

Cheap variety from existing art: colour variants with a different behaviour (for example a red frog that hops twice as high, or a purple bat that chases), and enemy + obstacle combinations (a frog on a moving platform, an eagle beside a pendulum).

## Art

New creatures must match the Sunny Land pack style. Options: packs by the same artist or in the same style, or commissioned art. Colour variants can be derived from the current sprites.

## Placement findings (levels 3–7)

Placing the existing frog, eagle and bat in levels 3–7 was evaluated and **postponed**: the route stretches are already dense with obstacles, mostly falling logs (e.g. L3 bridge x 27–93: 32 obstacles; L7 corridor x 42–106: 25; L5 stretches: 8–12 each). The few empty flat stretches are off-route (ceilings, edges without gems). Adding enemies on top would raise difficulty and clutter rather than variety. The owner plans to extend some levels first; when they are extended, prefer swapping part of the repeated falling logs for enemies.
