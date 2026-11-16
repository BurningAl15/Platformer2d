# Background Parallax and Interior Tilemap Plan

**Review and implementation date:** 2026-11-14  
**Branch/base revision:** `codex/fix-horizontal-platform-riding`, `9e81de0`  
**Validation type:** Static source/scene review and exact raster recomposition. Unity Editor validation remains pending.

## Current setup

`Assets/Scripts/Camera/SimpleCameraController2d.cs` is the existing owner of camera follow and parallax. Every authored gameplay scene from `Level_0` through `Level_10` serializes its `farBackground` and `middleBackground` references on the camera controller.

The current source sprites are:

| Asset | Size | Current visual content |
|---|---:|---|
| `Assets/2D Platformer Assets/Art/LevelArt/Background/back.png` | 384 × 240 | Clouds, horizon, and sea combined in one opaque image |
| `Assets/2D Platformer Assets/Art/LevelArt/Background/middle.png` | 176 × 368 | One transparent plant/foliage silhouette composition |

The controller moves the camera and backgrounds in `Update` (lines 66–91). The far layer follows camera displacement at factor `1.0`, so it stays fixed relative to the screen and contributes no visible parallax. The middle layer follows at `0.5`, which gives it one half of the camera displacement as screen-relative drift. The controller only exposes these two layer transforms. `UpdateFarBackgroundCoverage()` scales the far sprite on X to cover the viewport (lines 105–137); this avoids uncovered edges but can distort the artwork on wider aspect ratios. The camera and parallax are updated in the same `Update`, rather than after the camera's final position for the frame.

## First layered background implementation

The original `back.png` was preserved. Its 384×240 pixel content was separated without redrawing into `OutdoorBase.png` (flat sky and ocean base), `SeaShimmer.png` (transparent water highlights), and cloud art under `Assets/2D Platformer Assets/Art/LevelArt/Background/Parallax/`. The three initial layers use Unity import metadata based on the existing background settings (16 PPU, point filter, no compression, alpha transparency enabled on the transparent layers). Compositing them at full opacity reproduces the original source with **zero differing pixels**.

The existing `middle.png` remains the plant/foliage layer. The outdoor scene composition is now:

| Layer | Follow factor | Purpose |
|---|---:|---|
| `Far` / `OutdoorBase` | 1.0 | Screen-stable sky and flat ocean coverage |
| `SeaShimmer` | 1.0 | Two-second, 16-frame Aseprite rolling-water cycle that warps the surface and highlights, with a subtle DOTween alpha pulse |
| `Clouds_Main` | 0.92 | Main connected cumulus bank, plus a slow DOTween drift of 0.35 world units over 12 seconds, easing back and forth |
| `Clouds_High` | 0.96 | Detached high cloud puffs; moves independently with a slightly stronger camera follow for subtler screen-relative drift |
| `Middle` / existing `middle.png` | 0.5 | Plant silhouettes with stronger depth drift |

`SimpleCameraController2d` now serializes optional cloud and sea-shimmer transforms plus Inspector-adjustable follow/shimmer values; old scenes without these optional references keep null/default compatibility. The sea shimmer alpha tween uses DOTween's existing `SpriteRenderer.DOFade` support and is linked to the camera object lifetime. The camera/parallax update runs in `LateUpdate`, after player updates. The cloud and sea-shimmer renderers match the far renderer's horizontal coverage scale so their cutouts stay aligned across aspect ratios. Each `Level_0`–`Level_10` scene and the reusable `LevelPrefab` now use the base sprite and have sea-shimmer/cloud renderers ordered between Far and Middle. The source assets and existing Middle layer are unchanged.


The earlier five-frame ping-pong that rigidly translated the highlight cutout was rejected by the owner: it looked like a stripe sliding across the screen, not water. It is retained only as historical source in `ArtSource/Background/SeaShimmerCycle.aseprite` and the now-unused `Assets/Resources/SeaShimmerAnimation/` folder. The corrected editable source is `ArtSource/Background/SeaWaves.aseprite`, authored through Aseprite MCP from the separated 384×240 water artwork. Its 16 forward-loop frames deform the water surface and its existing reflections with overlapping, depth-dependent wave motion; the calmer cycle is 2 seconds (125 ms per frame). Unity frames are in `Assets/Resources/SeaWaves/`. The existing `SimpleCameraController2d` loads these sprites once and advances them forward at 8 fps. The far-ocean base, camera ownership, scene hierarchy, and gameplay geometry remain unchanged. The existing DOTween alpha pulse is retained as a secondary shimmer only, now softened to a 0.90 minimum alpha over a 5-second fade.

### Split cloud bank and high puffs

The original transparent `Clouds.png` is retained as the source. Its detached upper cloud groups were separated by pixel connectivity into `CloudsHigh.png`; the remaining connected bank is `CloudsMain.png`. Both are full-size 384×240 transparent sprites with matching pivots and import settings, so they align without cropping or shifting. Recombining them over one another reproduces `Clouds.png` with **zero differing pixels**.

Every authored level (`Level_0`–`Level_10`) and `LevelPrefab` now has sibling visual objects `Clouds_Main` and `Clouds_High` under the existing background group. `SimpleCameraController2d` owns both references. The main bank follows at 0.92 and keeps its slow ambient drift; high puffs follow at 0.96 and do not get a second drift tween. Their sorting orders are -18 and -19 respectively. This is a restrained depth separation, not a new parallax manager or a new camera.

The separately extracted cloud layer also has a restrained ambient drift in the same camera controller: a DOTween float moves it 0.35 world units over 12 seconds, then eases back, while the existing 0.92 camera-follow factor remains. The drift is applied as a delta after camera-relative parallax, so it does not compete with the camera follow update or require extra scene objects. This is an ambient motion pass; the cloud artwork itself is unchanged.

## Recommended outdoor composition

Keep `SimpleCameraController2d` as the owner. Extend its existing serialized layer configuration only when the artwork is ready; do not add a second camera or a parallel parallax manager. A useful initial composition is:

| Back to front | Suggested starting camera-follow factor | Content and role |
|---|---:|---|
| Base panorama | 1.0 | Existing sea/sky image remains the stable base until it is replaced; this is a coverage layer, not a moving parallax layer. |
| Cloud bank / distant horizon silhouettes | 0.85–0.95 | New, separately authored transparent or tileable art; very subtle movement. |
| Distant vegetation | 0.65–0.8 | New lower-contrast foliage silhouettes, placed behind the current plants. |
| Existing middle foliage | 0.4–0.55 | Use `middle.png` as the main middle-distance framing layer. |
| Near foliage or foreground framing | 0.15–0.3 | New sparse silhouettes, used only where they do not obscure play or HUD readability. |

These factors describe how much each layer follows camera movement: `1.0` stays screen-stable, while lower values move less with the camera and therefore drift more across the screen. They are starting values for a visual pass, not final tuning. Keep vertical movement restrained; large vertical drift in the sea, horizon, or foliage will make the scene feel detached from the world.

The current `back.png` cannot provide independent cloud and sea movement because those elements are baked together. Reusing or repeating the same full image several times will preserve that limitation and create obvious duplication. For a real improvement, author separate cloud, horizon/sea, distant-foliage, and near-foliage layers in the existing pixel-art style. Preserve source pixel scale and use point filtering. Avoid stretching a single sprite to fit every phone ratio; compose coverage from appropriately sized artwork or tileable pieces, and inspect both 16:9 and tall mobile framing.

For stable motion, first let the camera reach its final frame position, then apply each layer's displacement using the camera delta for that frame. The current per-frame `Update` ordering is a candidate jitter/lag source; verify camera and player interpolation in Play Mode before changing lifecycle timing.

## Outdoor tilemaps versus gameplay geometry

Tilemaps can participate in parallax when their **visual-only** transforms are separate from gameplay geometry. A serialized scan of `Level_0`–`Level_10` found:

- `Level_1`–`Level_10` each have one `Environment/LevelPrefab/Tilemap` with both `TilemapRenderer` and `TilemapCollider2D`. These are gameplay surfaces and must remain world-aligned.
- `Level_0` has `Environment-Parallax/Collider-Tilemap` with a collider and a separate `Over-Tilemap` with a renderer only. Both sit below the same `Environment-Parallax` parent, so that shared root must not be translated as a parallax layer. `Over-Tilemap` may be visual-only, but its actual tile content and sorting need Editor review before assigning it another role.

The current serialized levels do not contain a dedicated, known-safe interior background Tilemap. For tomb rooms, create a separate decorative background Grid/Tilemap behind the play space, with `TilemapRenderer` and no `TilemapCollider2D`, `Rigidbody2D`, or gameplay scripts. Keep the authored collision Tilemap stationary. If the level transitions outdoors-to-tomb, toggle/fade the visual sets at an authored section trigger or doorway, taking the full camera viewport into account; that section switching behavior is not implemented by this parallax pass.

When decorative tile art is useful outdoors, author it on a separate background Grid/Tilemap with a dedicated transform and sorting layer. It should have a `TilemapRenderer` but no `TilemapCollider2D`, `Rigidbody2D`, or gameplay scripts. Keep collision, ground, one-way platforms, foreground occluders, and trigger tilemaps fixed in world space. A visual copy of a gameplay tilemap is still a copy: it must not inherit colliders or gameplay ownership.

## Tombs and other enclosed sections

Treat an interior as a distinct visual set, not as the outdoor background moved behind a cave-shaped tilemap:

1. Keep the existing world/collision Tilemaps stationary and authoritative.
2. Add a separate, non-colliding interior background Tilemap or layered sprites behind the play space. Use tomb wall/stone art with a stable pattern and controlled variation so camera movement does not expose empty gaps.
3. Remove or hide the outdoor cloud/sea/foliage set while the camera is inside. If a level moves between outdoor and interior spaces, switch or fade the two visual sets at a deliberate doorway/section boundary; do not switch based only on camera position without accounting for the full viewport.
4. Use little or no parallax on enclosed walls. If depth is needed, move only decorative, non-colliding far details subtly; keep wall edges, platforms, foreground arches, and all colliders fixed.
5. Keep foreground framing as a separate visual-only layer rendered in front of the player only where the level layout requires it.

For a mostly enclosed level, the simpler and safer composition is a static background Tilemap plus fixed gameplay/foreground Tilemaps. Reserve layered parallax for the genuinely open sections. A dedicated room-background switching component is not justified until the levels actually need outdoor/interior transitions; first reuse scene-authored groups and existing activation mechanisms where practical.

## Implementation boundary and next owner action

The first outdoor pass uses exact source-art separation rather than regenerated cloud artwork, and animates the sea highlights without moving the water plane or any Tilemap. Additional distant and foreground vegetation would benefit from distinct sprites; avoid duplicating the same full panorama. The tomb wall/background tile set is also still to be authored. The current controller remains the single owner of camera and parallax; do not add another manager.

## Validation boundary

Static evidence covers the controller source, original and derived sprites, serialized camera references and layer ordering in `Level_0`–`Level_10`, the reusable LevelPrefab, and the named Level_0 Tilemap objects. The derived static layers composite back to the source exactly. Aseprite MCP validates the corrected 16-frame loop with no duplicate or missing frames and no seam duplicate; its generic timing/contact-edge warnings are expected for a flowing water texture rather than a grounded character. A composite preview was checked against the flat ocean base. Unity Editor/Pipeline was unavailable, so Unity import, compilation, Play Mode, mobile aspect-ratio review, and device test remain pending. The 0.92 cloud-follow factor, 0.35-unit cloud drift, and 0.90 shimmer minimum alpha still need visual tuning in Editor.
