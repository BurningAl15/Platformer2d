# Lives economy and difficulty curve

**Date:** 2026-11-20 · **Source:** current code (`DiceRollController`, `LevelManager`, `LivesManager`, `SaveManager`, `PlayerHealth`, `GameOverController`).
**Purpose:** give level design a target for how much each level should cost, so that the lives the game hands out are spent and difficulty keeps meaning something (owner direction: more lives → harder levels).

## 1. What the code does today

### Health inside a level
| Rule | Value |
|---|---|
| Base health | 6 units = 3 hearts (`PlayerHealth.baseMaxHealth = 6`) |
| A hit | −1 unit (half a heart) |
| A fall into a pit | −1 unit and back to the last checkpoint; **no life is spent** |
| Heart fragments | 2 fragments = +1 permanent heart (+2 units). World 1 has 6 fragments → up to **+3 hearts (12 units)** |
| Losing all health | −1 life, respawn at the last checkpoint with full health |

So a life is worth **6 hits** at the start of the world and up to **12 hits** once every heart fragment is found. Later levels must hit harder or more often just to keep the same cost.

### Lives in
| Source | Lives |
|---|---|
| New game / back to the map from Game Over | set to **3** |
| End-of-level dice — only when the run achieves something new (first clear, first gem perfect, time badge, clover found this run, first no-damage clear) | wood d4 **0–3** (avg 1.5) · one clover → blue d6 1,2,2,3,3,4 (avg 2.5) · two clovers → gold d4 3–6 (avg 4.5) · secret d6 4–6 (avg 5) |
| Game Over: rewarded ad (mobile, when ads work) | +2 |
| Game Over: clover | blue die (avg 2.5), a second clover upgrades to gold (avg 4.5) |
| Cap | `SaveManager.MAX_LIVES = 999` (effectively none) |

Gems are a collectible only and buy nothing (owner decision 2026-11-20).

### What running out costs
Back to the map resets lives to 3. **A Game Over never blocks progress; it costs the progress made in that level** (back to its start). That is the real penalty difficulty works with.

## 2. Expected lives

**First pass through World 1 (8 levels)** — a typical player gets one roll per level (first clear), sometimes upgraded with that level's clover:

- about **1.5–2.5 lives per level** → **~15 lives** across the world, plus the starting 3.

**Completionist replays** — every new badge (gems, time, clover, no-damage) gives one more roll: up to ~4 rolls per level → **~50–60 extra lives** across the world. With no cap they pile up and stop mattering.

## 3. Target cost per level (first pass)

Spend roughly what the world hands out, so a first-time player finishes World 1 with a few lives left and feels pressure in the second half.

| Level | Target lives lost (average player, first clear) | Expected max health by then | Notes |
|---|---|---|---|
| 1 | 0–1 | 3 hearts | Teach, no pressure |
| 2 | 1 | 3–3.5 | First real hazards |
| 3 | 1–1.5 | 3.5 | |
| 4 | 1.5–2 | 4 | Health has grown: add damage sources |
| 5 | 2 | 4 | |
| 6 | 2–2.5 | 4.5 | |
| 7 | 2.5–3 | 5 | |
| 8 (boss) | 3 | 5–6 | Climax |
| 9–10 (new) | 3–3.5 each | 6 | Amber Heart finale |
| **Total** | **~17–20** | | ≈ lives earned + start |

How to read "lives lost": deaths, not hits. With 6 units per life at the start, one life ≈ 6 hits; by Level 7–8, one life ≈ 10–12 hits. Falls cost health but never a life on their own.

## 4. Levers for level design

- **Hits per stretch**: count the hits a careful player would still take between checkpoints; that, not hazard count, sets the cost.
- **Checkpoint spacing**: the cost of a Game Over is the distance since the level start, and of a death the distance since the last checkpoint. Levels 5 and 7 currently have one checkpoint over 224 and 138 units (see `LEVEL_AUDIT_WORLD1.md`).
- **Pits vs. damage**: pits only cost health and position. Use damage (enemies, spikes, smashers) when the level should cost lives.
- **Heart fragments**: each one makes the rest of the world cheaper (+2 units). Place them where the extra heart is earned (hidden, optional route).

## 5. Recommendations

1. **Lives cap: 15** (two digits in the HUD, room for a gold roll and a Game Over continue). Completionist rolls above it are wasted, so replays reward collectibles and badges rather than a growing hoard. Owner decision.
2. **Per-level targets** above as the design goal; tune with data (below) rather than guesses.
3. **Assist mode** (planned): extra hearts make levels cheaper; keep it out of no-damage records, as planned.
4. **Telemetry** (with Firebase, once set up): per level and run — lives on entry, deaths, falls, hits taken, restarts, Game Overs, completion. Compare against the table to find walls and walkovers.
5. **Automated tests** to lock the rules above (dice faces, roll conditions, cap, health per fragment, no-damage rule) so balance changes are deliberate.
