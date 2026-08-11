# Kick Lucky Cube Runtime Visual Ownership

Updated: 2026-08-02

Latest source scan: `25` construction sites: `21` manager-only, `4` prefab-backed dynamic data instances, `0` known migration sites, and `0` unclassified sites.

The design rule is: authored hierarchy and appearance live in prefabs; runtime code may create invisible managers and instantiate prefab-backed dynamic data. Run `Tools > Kick Lucky Cube > Validation > Audit Runtime-Created Visuals` after changing a gameplay controller. The Play Mode command applies the same deterministic source-policy gate while the complete UI/mechanic probe exercises the prefab contracts.

`PrefabUtility.GetCorrespondingObjectFromSource` is not used as a Play Mode ownership test. Unity does not preserve that editor link consistently for runtime clones, and the old check incorrectly reported authored scene objects and prefab clones as code-generated visuals.

## Allowed Ownership

| Classification | Rule | Current examples |
|---|---|---|
| Manager only | Invisible bootstrap/controller object; no Renderer, Graphic, TextMesh, ParticleSystem, or Light hierarchy | commerce, SFX, leaderboard, inventory, shops, save and allocation controllers |
| Dynamic data instance from prefab | Code creates an identity/container, then instantiates an authored model/card/effect prefab into it | runner, carried/stable mob, repeated shop/album/inventory cards |
| Must migrate | Code constructs final text, images, primitive meshes, particles, lights, layout, or decoration | none |

## Completed Migrations

| Runtime owner | Authored result |
|---|---|
| `KickLuckyCubeCurrencyFxController` | requires the gameplay canvas and `KLC_CurrencyGainFlyText` prefab |
| `KickLuckyCubeHomeIconMarker` | uses the authored home marker under the gameplay canvas |
| `KickLuckyCubePlotAllocator` | uses `KLC_BotStableLabel.prefab` and the self-contained fake-online actor prefab |
| stable placement/slot/upgrade scripts | use separate collect, status, and upgrade world-label prefabs |
| `KickLuckyCubeToolTrainingController` | uses `KLC_StrengthToolHandPreview.prefab`; tool shape/color are serialized prefab data |
| `KickLuckyCubeUiPrefabFactory` / `KickLuckyCubeUiTheme` | require authored templates and components and fail clearly when a contract is incomplete |
| `KickLuckyCubeWaveChaseController` | binds the authored speed label, collider, and four vignette edges |
| `KickLuckyCubeWorldTextOutline` | binds authored `Outline_0` to `Outline_3` children instead of cloning text objects |
| animal reveal/grade visuals | use animal fallback, roulette, and Normal/Golden/Diamond/Fire VFX prefabs |
| leaderboard | binds authored scene labels or `KLC_LeaderboardBoardVisual.prefab` |

## Validation Contract

The source audit searches game scripts for `new GameObject`, `CreatePrimitive`, and visual `AddComponent` calls. Every hit must be classified. `MustMigrate` and `Unclassified` are release-blocking results.

The audit intentionally does not claim that every existing scene object is already a linked prefab instance. Corridor biome, world-kiosk, and player-plot conversion are separate level/design tasks (`KLC-PROD-009` to `KLC-PROD-013`). This document covers visuals created by runtime gameplay code.

Do not add a new file to `MustMigrate`. Create the prefab first and let the controller instantiate it, bind events/references, and update dynamic values only.
