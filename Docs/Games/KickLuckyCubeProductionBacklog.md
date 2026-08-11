# Kick Lucky Cube Production Backlog

Last manual product review: 2026-08-05.

This is the source of truth for moving `KickLuckyCubeOverview` from a playable prototype to a production-ready vertical slice. The long implementation history remains in `KickLuckyCube.md`; contributor setup and ownership rules remain in `KickLuckyCubeHandoff.md`.

## Status Legend

- `OPEN`: not accepted yet.
- `IN PROGRESS`: currently owned and being changed.
- `VERIFY`: implemented but still needs the complete acceptance pass.
- `DONE`: acceptance criteria passed in Edit Mode and Play Mode.
- `P0`: blocks the core loop or can corrupt progression.
- `P1`: required architecture or broken production feature.
- `P2`: required visual/UX pass.
- `P3`: polish after the production path is stable.

## Global Definition of Done

Every task must meet all relevant requirements below before it becomes `DONE`:

1. Visible scene and UI content is editable from a prefab in Edit Mode. Runtime code may update dynamic state, but must not author the final layout or visible geometry.
2. Desktop and touch input paths are checked where the feature is interactive.
3. The Unity Console contains no new errors, exceptions, missing references, or duplicate-component warnings.
4. Save-affecting work is checked through a real Stop Play / Start Play cycle without changing unrelated local progress.
5. Dynamic values are tested at zero, insufficient funds, normal values, and large values.
6. Prefabs, scenes, configs, scripts, documentation, and their `.meta` files are changed together.
7. The relevant part of the full route is repeated: spawn -> train -> kick -> reveal -> wave run -> return -> stable/sell -> upgrade.

## Recommended Delivery Order

| Milestone | Goal | Tasks |
| --- | --- | --- |
| A | Restore a reliable core loop | `KLC-PROD-001` to `KLC-PROD-007` |
| B | Complete prefab-first ownership | `KLC-PROD-008` to `KLC-PROD-013` |
| C | Restore and complete production features | `KLC-PROD-014` to `KLC-PROD-020` |
| D | Visual and content polish | `KLC-PROD-021` to `KLC-PROD-024` |
| Carry-over | Finish previously found fake-online issue | `KLC-PROD-025` |
| E | Character, training, and kick-style follow-up | `KLC-PROD-026` to `KLC-PROD-031` |

Do not start broad visual redesign before Milestone A is accepted. Camera, grounding, spawn offsets, and prefab ownership can invalidate visual work placed on unstable runtime objects.

## Responsibility Split

Execution classes:

- `HUMAN`: final work depends primarily on spatial composition, art direction, readability, or subjective feel. The human owns completion and acceptance.
- `AGENT`: Codex owns implementation, prefab/config changes required by the behavior, regression checks, and documentation. Human work is not required before the task can become `DONE`.
- `AGENT -> HUMAN`: Codex first creates the stable logic, prefab contract, authoring controls, preview/validation tool, and functional baseline. The human then performs the final visual or cinematic tuning without changing controller code.

### 1. Human-Owned Tasks

| Task | Human responsibility | Agent support |
| --- | --- | --- |
| `KLC-PROD-013` | Decide which unexplained cube, gates, blockout pieces, and old scene decorations should remain; perform the final spatial cleanup. | Produce reference/dependency reports before deletion and verify the cleaned scene afterward. |
| `KLC-PROD-024` | Approve and finish the final UI composition, hierarchy, spacing, colors, typography, icon readability, and desktop/mobile visual quality. | Provide responsive prefabs, state bindings, overlap/validation checks, screenshots, and fix functional layout defects. |

These tasks should not block the engineering work that prepares their inputs, but only the human can mark their final visual acceptance as `DONE`.

### 2. Agent-Owned Tasks

| Task | Agent deliverable |
| --- | --- |
| `KLC-PROD-001` | Grounded player controller, configurable ground settings, and movement/jump regression checks. |
| `KLC-PROD-002` | Correct cube impact and animal reveal placement for all visual sizes and grades. |
| `KLC-PROD-004` | Deterministic grounded runner spawn before chase control begins. |
| `KLC-PROD-005` | Non-negative, overflow-safe wallet operations, save migration, and boundary tests. |
| `KLC-PROD-006` | Fully working sell-shop open/list/sell/save cycle. |
| `KLC-PROD-007` | Saved selectable kick-strength setting and prefab-backed functional window. |
| `KLC-PROD-008` | Runtime visual-construction audit, migration inventory, prefab validation, and removal of code-owned final visuals. |
| `KLC-PROD-014` | Working prefab-backed leaderboard binding, sorting, and refresh. |
| `KLC-PROD-015` | Working style purchase/equip/persistence flow using editable visual slots. |
| `KLC-PROD-023` | Catalog validator and corrected model/icon/name/animation mappings across every use site. |
| `KLC-PROD-025` | Self-contained fake-online bot actor prefab and runtime allocation/binding. |
| `KLC-PROD-028` | Correct locomotion facing for forward, backward, and strafing movement without breaking the kick direction. |
| `KLC-PROD-030` | Complete wardrobe thumbnail coverage and validation for every selectable clothing item. |

For these tasks Codex should complete the behavior and verification before handing the build back for product review. Visual polish can still be applied later through `KLC-PROD-024`.

### 3. Agent Tooling, Then Human Finish

| Task | Agent creates first | Human finishes afterward |
| --- | --- | --- |
| `KLC-PROD-003` | Explicit camera-state sequence plus a choreography config/authoring component with targets, offsets, durations, FOV, easing, preview hooks, and safe reset behavior. | Tune the wave reveal shot, return shot, timing, and final feel in the Scene/Game view. |
| `KLC-PROD-009` | Biome prefab contract, anchors/metadata component, corridor assembly workflow, and collider/reference validator. | Remodel every biome, choose colors/materials, and place large and small decoration inside the biome prefabs. |
| `KLC-PROD-010` | Separate functional world-shop prefab shells, shared authoring contract, interaction/window bindings, and validation. | Redesign each kiosk's ProBuilder geometry, signage, props, and final placement. |
| `KLC-PROD-011` | Fully bound player-base prefab, nested stable-slot contract, serialized anchors, runtime allocator integration, and edit-mode preview/validator. | Rebuild the base geometry and arrange stable pads, boards, paths, and decoration using the exposed anchors. |
| `KLC-PROD-012` | Generated home icon, marker prefab, plot binding, visibility-through-walls behavior, and tunable offset/range/scale settings. | Approve or replace the icon and tune its final size and placement over the redesigned base. |
| `KLC-PROD-016` | Functional speed-shop window/card prefabs, cumulative pricing bindings, responsive layout controls, and preview/test data. | Finish card size, typography, colors, spacing, icon treatment, and desktop/mobile composition. |
| `KLC-PROD-017` | Functional horizontal tool carousel, centered equipped item, large preview, complete price/state bindings, and prefab preview setup. | Finish tool presentation, carousel composition, card styling, and visual emphasis. |
| `KLC-PROD-018` | Exchange booth/window prefabs, current/result card bindings, arrow, signed income comparison, warning states, and atomic transaction logic. | Finish booth geometry and final window/card appearance. |
| `KLC-PROD-019` | Elite-shop prefab/card system with model/icon, income, currency route, price, owned state, and safe purchase binding. | Finish premium presentation, VFX framing, card art, and world-shop appearance. |
| `KLC-PROD-020` | Weather/reward stand prefab contracts, all UI/state bindings, claim marker logic, timers, and edit-mode state previews. | Redesign both stands and tune the final ready/active/claimed visual language. |
| `KLC-PROD-021` | Stable lucky-cube prefab hierarchy with ProBuilder-editable geometry, art/material slots, correct question-mark orientation, collider, carry/flight/impact anchors, and validation. | Refine proportions, bevels, materials, question marks, and final art quality. |
| `KLC-PROD-022` | Barbell animation state setup, grip/foot targets, tunable timing, preview controls, baseline loop/transitions, and validation for different tool widths. | Polish the final body motion, poses, timing, weight, and hand/foot contact. |
| `KLC-PROD-026` | Replace the legacy dumbbell exercise clip, bind both hand props, and add contact/loop validation. | Approve and polish exercise poses, weight, balance, and timing from multiple viewing angles. |
| `KLC-PROD-027` | Rebuild the dumbbell tier prefabs and material progression while preserving catalog ids, prices, strength, ownership, and saves. | Approve silhouettes, size progression, materials, and premium readability in the shop and in the character's hands. |
| `KLC-PROD-029` | Add a short kick showcase camera phase with safe transitions, configurable timing, interruption handling, and a preview route for every style. | Tune the framing and timing so the purchased animation is readable without making repeated kicks tedious. |
| `KLC-PROD-031` | Update the first two skin-tone textures and verify their material bindings under representative lighting. | Approve the final lighter colors and separation between the two free options. |

An `AGENT -> HUMAN` task is not ready for human work until its prefab can be edited safely in Edit Mode, its dynamic state can be previewed, and changing the visual hierarchy no longer requires controller changes.

## Assignment Summary

| Execution class | Count | Tasks |
| --- | ---: | --- |
| Human | 2 | `013`, `024` |
| Agent | 13 | `001`, `002`, `004`, `005`, `006`, `007`, `008`, `014`, `015`, `023`, `025`, `028`, `030` |
| Agent -> Human | 16 | `003`, `009`, `010`, `011`, `012`, `016`, `017`, `018`, `019`, `020`, `021`, `022`, `026`, `027`, `029`, `031` |

Recommended active ownership rule: one task has one active editor at a time. While Codex owns an `AGENT -> HUMAN` task, the human should avoid editing that task's target prefabs. After the handoff checkpoint, Codex should avoid changing their static hierarchy unless the human requests a structural fix.

## Milestone A: Core Loop And Economy

### KLC-PROD-001: Keep The Player Grounded

- Status: `DONE` (2026-08-02)
- Priority: `P0`
- Owner: Gameplay engineering
- Source: manual review item 1

Problem: the player can move as if flying instead of following the ground.

Acceptance criteria:

- player spawn resolves to the active walkable floor;
- gravity is applied whenever the controller is not grounded;
- normal movement follows flat ground, ramps, small steps, and biome transitions without gaining height;
- jumping still works and reliably returns to the ground;
- opening UI, starting a kick, returning from the animal run, and loading a save cannot leave the player airborne;
- ground layers, slope limit, step offset, gravity, and ground-probe distance are serialized settings rather than magic constants.

### KLC-PROD-002: Fix Animal Landing Below The World

- Status: `DONE` (2026-08-02)
- Priority: `P0`
- Owner: Gameplay engineering
- Source: manual review item 7

Problem: the revealed animal can move below the floor after the lucky cube lands.

Acceptance criteria:

- cube impact resolves against the intended corridor floor, not decorative meshes or triggers;
- animal root position is calculated from the resolved surface and the normalized visual bounds;
- silhouette, reveal animation, and final runner all remain above the floor;
- the check passes for the smallest and largest animal prefabs in the catalog and in every grade;
- a missing floor hit fails safely and logs a clear diagnostic instead of spawning under the scene.

### KLC-PROD-003: Add Configurable Wave Camera Direction

- Status: `IN PROGRESS` (authoring config and preview ready 2026-08-02; human shot tuning remains)
- Priority: `P0`
- Owner: Gameplay/camera engineering
- Source: manual review item 8

Problem: camera state breaks during the wave introduction and the sequence cannot be tuned by a designer.

Required sequence:

```text
Cube lands -> animal reveal -> camera frames the wave rising -> camera returns and faces the runner toward home -> player control starts -> chase starts
```

Acceptance criteria:

- the sequence uses explicit states instead of unrelated delayed callbacks;
- input is locked only during the cinematic states;
- wave focus target, runner focus target, offsets, duration, easing, FOV, look-ahead, and pause durations are serialized in a dedicated choreography config/component;
- the camera never passes through the wave and does not spring when control returns;
- the run timer and wave movement begin only after the camera has returned to the runner;
- interruption, death, scene reload, and successful return restore the normal player camera deterministically.

### KLC-PROD-004: Ground The Runner Before Chase Start

- Status: `DONE` (2026-08-02)
- Priority: `P0`
- Owner: Gameplay engineering
- Depends on: `KLC-PROD-002`, `KLC-PROD-003`
- Source: manual review item 9

Problem: after the wave rises, the animal is teleported above the road.

Acceptance criteria:

- the runner root is snapped to the lane floor before its controller and camera are enabled;
- the ground offset is derived from visual bounds/collider data and is stable for every animal size;
- no cinematic transform continues writing the runner position after gameplay starts;
- movement and jump begin from the same grounded position shown at the end of the wave cinematic.

### KLC-PROD-005: Prevent Negative Or Overflowed Currency

- Status: `DONE` (2026-08-02)
- Priority: `P0`
- Owner: Economy engineering
- Source: manual review item 24

Problem: soft currency was observed as approximately `-1.6B`.

Acceptance criteria:

- identify whether the cause is unchecked spending, integer overflow, malformed offline income, or save migration;
- all purchases use atomic `CanAfford`/`TrySpend` operations and never subtract when funds are insufficient;
- wallet arithmetic uses a range suitable for the intended economy; if moving beyond `int`, save old integer values and migrate to a stable string-backed `long` representation;
- additions and multipliers use checked or saturating arithmetic and cannot wrap below zero;
- soft and hard currency are clamped to valid domain values when loading corrupted legacy data, with one diagnostic warning;
- tests cover zero, exact price, one below price, repeated clicks, very large offline income, rebirth/VIP multipliers, and the numeric display formatter.

### KLC-PROD-006: Restore The Animal Sell Shop

- Status: `DONE` (2026-08-02)
- Priority: `P0`
- Owner: Gameplay/UI engineering
- Depends on: `KLC-PROD-005`
- Source: manual review item 14

Acceptance criteria:

- the world interaction and menu route both open the sell window;
- inventory animals appear once with matching model/icon/name/value;
- selling removes exactly one selected animal and grants exactly the displayed amount;
- insufficient/empty inventory states are explicit and do not throw;
- the window closes through its close button, backdrop, and `Escape`;
- sale and wallet changes survive a real restart.

### KLC-PROD-007: Add Selectable Kick Strength

- Status: `DONE` (2026-08-02)
- Priority: `P1`
- Owner: Gameplay/UI engineering
- Source: manual review item 10

Problem: the button next to the strength HUD does not provide control over kick distance.

Acceptance criteria:

- the existing strength-settings button opens an editable prefab window;
- the player can select an integer strength from `1` to current maximum strength using a slider plus numeric value controls;
- selected strength is visible in the HUD and is clamped after rebirth or any maximum-strength reduction;
- selected strength is persisted;
- predicted distance and actual cube flight use the selected value consistently;
- maximum mode is explicit and easy to restore;
- the power timing bar continues to work as the execution multiplier and does not silently overwrite the selected maximum.

## Milestone B: Prefab-First Ownership

### KLC-PROD-008: Audit Runtime-Created Visuals

- Status: `DONE` (2026-08-02)
- Priority: `P1`
- Owner: Architecture/UI engineering
- Source: manual review item 3

The audit now reports `25` classified construction sites: `21` invisible managers, `4` prefab-backed dynamic data containers, and no visual-construction or unclassified sites. Manager-only bootstrap objects may remain, but visible content must be prefab-backed.

Acceptance criteria:

- produce an inventory of every visible runtime-created UI, world label, VFX root, shop object, animal decoration, and ProBuilder object;
- classify each item as `manager only`, `dynamic data instance from prefab`, or `must migrate`;
- remove runtime construction of final `Image`, `Text`, `TMP_Text`, `Renderer`, `TextMesh`, layout, and decorative hierarchy where an authored prefab should own it;
- controllers only instantiate prefabs, bind references/events, and write dynamic values/state;
- add an Editor validation command/test that reports visible runtime construction added to game controllers;
- update the inventory whenever a migration task is completed.

### KLC-PROD-009: Make Every Corridor Biome A Prefab

- Status: `IN PROGRESS` (first-pass voxel ProBuilder prefabs for locations 01-30 are connected and validated; themed `22m` walls, `12.6m` rivers, and runtime two-of-three bridge selection are active; final human art and traversal review remains)
- Priority: `P1`
- Owner: World/level design with engineering support
- Depends on: `KLC-PROD-008`
- Source: manual review item 4
- Canonical content brief: `Docs/Games/KickLuckyCubeLocations.md`

Acceptance criteria:

- each biome/location is a separate named prefab under a clear `Prefabs/World/Biomes` folder;
- each prefab owns ground visuals, large landmarks, small decoration, materials, and optional VFX;
- gameplay floor, start/end anchors, rarity metadata, and decoration root are exposed through one biome component;
- corridor assembly references prefab assets and does not regenerate final biome art from code;
- changing a biome prefab updates all intended instances without editing the main scene;
- biome colliders are checked against player, runner, cube raycasts, camera, and wave.

### KLC-PROD-010: Make Every World Shop A Separate Prefab

- Status: `IN PROGRESS` (eight shop contracts and extraction workflow ready 2026-08-02)
- Priority: `P1`
- Owner: World/UI design with engineering support
- Depends on: `KLC-PROD-008`
- Source: manual review item 5

Acceptance criteria:

- speed, training tools, sell, styles, exchange, elite mobs, weather, rewards, and other world kiosks each use a separate prefab;
- each prefab contains visual geometry, signage, interaction anchor, and explicit UI-window binding;
- shared kiosk pieces may use nested prefabs, but one shop cannot depend on another shop's hierarchy;
- shop visuals can be replaced without modifying gameplay controller code;
- no kiosk is rebuilt by the broad world-polish command after it becomes authored.

### KLC-PROD-011: Make The Player Base Fully Editable

- Status: `IN PROGRESS` (base anchors, contract and extraction workflow ready 2026-08-02)
- Priority: `P1`
- Owner: World design with gameplay engineering support
- Depends on: `KLC-PROD-008`
- Source: manual review item 12

Acceptance criteria:

- the complete player plot is an authored prefab with nested stable-slot prefabs;
- stable animal anchors, collect pads, upgrade boards, sell/utility anchors, owner label, and home-icon anchor are serialized references;
- visual geometry may be moved or replaced without name-based child searches breaking gameplay;
- runtime plot allocation instantiates the prefab and binds persistent player slot ids separately from bot slot ids;
- the prefab has a safe edit-mode preview that does not duplicate persistent gameplay objects in Play Mode.

### KLC-PROD-012: Add A Home Marker Prefab

- Status: `IN PROGRESS` (marker prefab/binding and distance controls ready 2026-08-02)
- Priority: `P2`
- Owner: UI/art plus gameplay binding
- Depends on: `KLC-PROD-011`
- Source: manual review item 11

Acceptance criteria:

- create a readable home icon sprite and editable world/UI marker prefab;
- show exactly one marker above the currently assigned player base;
- the marker follows random plot allocation and is never attached to a bot plot;
- it remains readable through walls without obscuring nearby UI;
- distance scaling, vertical offset, visibility range, and optional distance text are serialized.

### KLC-PROD-013: Clean Prototype Blockout Objects

- Status: `OPEN`
- Priority: `P2`
- Owner: Level design/user
- Source: manual review item 2

Acceptance criteria:

- identify the unexplained center cube, gates, obsolete markers, duplicated blockout geometry, and old generated roots;
- label every retained helper clearly and hide editor-only helpers at runtime;
- delete only objects confirmed to have no gameplay references;
- record any functional anchors that must remain before visual cleanup;
- final hub entry view contains no unexplained prototype primitives.

## Milestone C: Production Features And Shops

### KLC-PROD-014: Restore The Leaderboard

- Status: `DONE` (2026-08-02)
- Priority: `P1`
- Owner: Gameplay/UI engineering
- Source: manual review item 13

Acceptance criteria:

- the leaderboard prefab is present, active, readable, and bound after scene load;
- the local player and fake entries are sorted by the documented score rule;
- name, rank, score, and refresh cadence update without recreating text objects;
- empty/corrupted save data has a safe fallback;
- the result is checked in both Edit Mode preview and Play Mode.

### KLC-PROD-015: Restore And Complete The Style Shop

- Status: `DONE` (2026-08-02)
- Priority: `P1`
- Owner: Gameplay/UI engineering
- Depends on: `KLC-PROD-005`
- Source: manual review item 16

Acceptance criteria:

- world interaction and menu route open the style shop;
- cards show style preview, name, price, owned/equipped state, and lock reason;
- purchase, equip, re-equip, insufficient-funds, and persistence paths work;
- equipped style affects the intended lucky-cube visual in Edit-friendly prefab slots;
- repeated clicks cannot double-charge or create negative currency.

### KLC-PROD-016: Redesign The Speed Shop

- Status: `IN PROGRESS` (functional prefab and Edit Mode preview workflow ready 2026-08-02)
- Priority: `P2`
- Owner: UI design with gameplay engineering support
- Depends on: `KLC-PROD-005`
- Source: manual review item 17

Acceptance criteria:

- speed shop window and repeated purchase option are editable prefabs;
- current level, current speed, next gain, `+1/+5/+10` cumulative prices, and affordable state are readable;
- unavailable purchase options are hidden or clearly disabled according to the agreed UX;
- infinite level pricing remains correct and does not overflow;
- opening and rebuilding the list does not create duplicate listeners or unnecessary per-frame allocations;
- desktop and narrow/mobile layouts remain usable.

### KLC-PROD-017: Redesign The Training Tool Shop

- Status: `IN PROGRESS` (functional prefab and Edit Mode preview workflow ready 2026-08-02)
- Priority: `P1`
- Owner: UI/design and gameplay engineering
- Depends on: `KLC-PROD-005`
- Source: manual review item 18

Acceptance criteria:

- tool cards are shown as a large horizontal carousel/list with left/right scrolling;
- the equipped tool is centered or automatically scrolled into view when the shop opens;
- every card clearly shows model/icon, name, strength per tick, price, lock state, owned state, and equipped state;
- the selected tool has a large preview and equip/buy action;
- buying and equipping are separate deterministic states and persist after restart;
- prices remain visible for kettlebells/weights and are never covered by other elements.

### KLC-PROD-018: Improve The Exchange Booth And Window

- Status: `IN PROGRESS` (world/UI contracts and preview workflow ready 2026-08-02)
- Priority: `P2`
- Owner: UI/world design with gameplay engineering support
- Source: manual review item 19

Acceptance criteria:

- exchange booth uses its own editable world prefab;
- window clearly presents `current mob -> resulting mob` with matching icons/models;
- income comparison shows signed absolute and multiplier change, including downgrade warning;
- cost/charge, availability, confirm, and cancel states are readable;
- exchange is atomic and preserves inventory/save integrity.

### KLC-PROD-019: Improve The Elite Mob Shop

- Status: `IN PROGRESS` (world/UI contracts and preview workflow ready 2026-08-02)
- Priority: `P2`
- Owner: UI/world design with gameplay engineering support
- Depends on: `KLC-PROD-005`
- Source: manual review item 20

Acceptance criteria:

- each elite mob card shows matching model/icon, name, grade/effects, income, price, purchase currency, and owned state;
- IAP-supported and hard-currency fallback platforms show the correct price route;
- purchased mobs enter the normal inventory/save/catalog discovery path;
- no purchase can charge twice or exceed inventory rules without a clear message.

### KLC-PROD-020: Redesign Weather And Reward Stands

- Status: `IN PROGRESS` (world/UI contracts and state preview workflow ready 2026-08-02)
- Priority: `P2`
- Owner: UI/world design with gameplay engineering support
- Source: manual review items 15 and 21

Weather acceptance criteria:

- weather machine has an authored world prefab with clear idle, ready, active, and cooldown states;
- its prefab-backed window shows requirement, consumed mob/cost, resulting rarity effect, duration, remaining time, and unavailable reason;
- active weather is readable without opening the window.

Reward stand acceptance criteria:

- reward stand has an authored world prefab and a prefab-backed claim window;
- the window can open before and after claiming and clearly shows reward, timer/condition, and claimed state;
- an exclamation marker appears only while a reward is claimable;
- claiming is one-time where intended, persists after restart, and cannot be triggered twice by rapid input.

## Milestone D: Visual And Content Polish

### KLC-PROD-021: Rebuild The Lucky Cube Visual

- Status: `IN PROGRESS` (editable hierarchy contract and anchors ready 2026-08-02)
- Priority: `P2`
- Owner: 3D/art design
- Source: manual review item 6

Acceptance criteria:

- `KLC_LuckyCube.prefab` owns all final geometry, materials, edges, question marks, trail anchors, carry orientation, and impact VFX anchors;
- all six question marks face outward and read correctly;
- Edit Mode, carry, kick, flight, landing, silhouette, and style variants use the same prefab hierarchy;
- cube scale and collider are stable and do not change animal landing calculations;
- runtime code does not rebuild decorative cube geometry.

### KLC-PROD-022: Improve The Barbell Training Animation

- Status: `IN PROGRESS` (clip sampling, grip alignment and contact validation ready 2026-08-02)
- Priority: `P2`
- Owner: Animation/gameplay engineering
- Source: manual review item 22

Acceptance criteria:

- hands stay attached to the bar through the full loop;
- feet remain grounded and the body does not distort or slide;
- idle -> equip -> train -> unequip transitions are clean and loop correctly;
- animation speed follows the training cadence without restarting every frame;
- the x2 prompt and normal gain ticks align with readable animation beats;
- fallback behavior is defined for tools with different grip widths.

### KLC-PROD-023: Validate Animal Names, Icons, Models, And Animation

- Status: `DONE` (2026-08-02)
- Priority: `P1`
- Owner: Content/gameplay engineering
- Source: manual review item 23

Acceptance criteria:

- one catalog id resolves exactly one display name, icon, prefab, rarity, income value, and animation setup;
- roulette silhouette, reveal, controlled runner, carried animal, stable, inventory, album, sell shop, exchange, and elite shop all use that same entry;
- add an Editor validation command/test that fails on missing, duplicate, or mismatched catalog references;
- manually verify all 22 base animals and representative Golden/Diamond/Fire variants;
- generated fallback icons are replaced or explicitly approved rather than silently showing another animal.

### KLC-PROD-024: Final UI Consistency And Responsive Pass

- Status: `OPEN`
- Priority: `P2`
- Owner: UI design with engineering support
- Depends on: `KLC-PROD-007`, `KLC-PROD-014` to `KLC-PROD-020`
- Source: manual review items 10 and 15-21 collectively

Acceptance criteria:

- all windows use the agreed outlines, type hierarchy, button states, close behavior, currency presentation, and icon treatment;
- no panel, card, label, or button overlaps at desktop and representative phone aspect ratios;
- static hierarchy/layout lives in prefabs; code controls only dynamic values and state;
- every interactive icon has a recognizable action and a text/tool-tip fallback where needed;
- all visible user-facing text is ready for the TMP/localization pass;
- Game View and Edit Mode prefab previews match except for dynamic data.

## Carry-Over Finding

### KLC-PROD-025: Instantiate Active Fake-Online Bot Actors

- Status: `DONE` (2026-08-02)
- Priority: `P2`
- Owner: Gameplay/world engineering
- Source: 2026-08-01 full-cycle verification

Problem: bot plots and stable animals are allocated, but `KickLuckyCubeFakeOnlineBot` actors remain under disabled legacy `KLC_FakePlot_*` objects.

Acceptance criteria:

- one self-contained actor prefab is instantiated and configured for every allocated bot plot;
- bot patrol, training, and kick anchors come from the allocated plot prefab;
- no bot actor remains dependent on disabled legacy preview roots;
- bots are ambience-only, do not modify player persistence/economy, and do not block gameplay colliders.

## Milestone E: Character, Training, And Kick-Style Follow-Up

Recommended internal order: `KLC-PROD-028`, `KLC-PROD-026`, `KLC-PROD-027`, `KLC-PROD-029`, `KLC-PROD-030`, `KLC-PROD-031`. The reversed player facing is a gameplay defect and should be fixed before judging animation framing.

### KLC-PROD-026: Replace The Legacy Dumbbell Exercise Animation

- Status: `DONE` (2026-08-05)
- Priority: `P2`
- Owner: Animation/gameplay engineering
- Source: manual review 2026-08-05, item 1

Problem: the dumbbell exercise still uses the old animation and does not match the current Blockbench mannequin or the intended leg-training presentation.

Acceptance criteria:

- replace the legacy clip with a new leg-focused dumbbell exercise authored on the shared mannequin rig;
- both dumbbells remain aligned to their left/right hand attachment points for the complete motion;
- feet remain planted, balance looks believable, and limbs do not intersect the torso or equipment;
- idle -> equip -> exercise -> unequip transitions are clean and the exercise loop has no visible snap;
- verify the clip on male and female bodies and with the smallest and largest dumbbell tiers;
- inspect the motion from front, side, rear, and gameplay-camera views before acceptance.

Implementation/verification: the approved Blockbench `animation.KLC_Dumbbell_LateralLunge` clip is now the `DumbbellTraining` Animator state. Odd tiers attach authored left/right props to `SOCKET_Dumbbell_L/R`; Play Mode verified the state, both socket bindings, authored renderers, and clean return to Idle.

### KLC-PROD-027: Rebuild The Dumbbell Progression Models

- Status: `DONE` (2026-08-05)
- Priority: `P2`
- Owner: Content art/gameplay engineering
- Depends on: `KLC-PROD-017`
- Source: manual review 2026-08-05, item 2

Problem: the equipped and shop-preview dumbbells still use the old models instead of the planned material, size, and silhouette progression.

Acceptance criteria:

- every dumbbell tier uses its intended authored model and material, progressing from crude stone to premium magical ore;
- size and silhouette increase remain readable without breaking hand grip spacing or exercise clearance;
- the shop card, large preview, equipped hand props, and exercise animation resolve the same catalog entry;
- catalog ids, price, strength value, owned state, equipped state, and existing saves remain compatible;
- no placeholder cube, legacy dumbbell renderer, missing material, or pink shader appears in Play Mode;
- the full buy -> equip -> train -> restart -> reload path is verified for representative early, middle, and final tiers.

Implementation/verification: `KLC_StrengthTools.bbmodel` supplies 15 alternating dumbbell/barbell roots from stone through Arcane Godstone, with authored textures/materials and model-rendered shop thumbnails. Unity validation found all 15 roots and 15 icons; Play Mode verified both dumbbell hand sockets and the barbell prop socket without legacy primitive renderers.

### KLC-PROD-028: Correct Player Locomotion Facing

- Status: `DONE` (2026-08-05)
- Priority: `P1`
- Owner: Gameplay/animation engineering
- Source: manual review 2026-08-05, item 3

Problem: during normal movement the visible player faces opposite the travel direction and appears to walk backward.

Acceptance criteria:

- forward input makes the character face and travel forward relative to the intended movement/camera convention;
- backward movement and strafing use deliberate facing behavior rather than an accidental 180-degree model offset;
- Idle and Walk animation states do not mirror or reverse the mannequin unexpectedly;
- kick targeting, cube contact direction, training attachments, camera orbit, and bot facing remain correct after the fix;
- verify keyboard and touch/mobile movement in at least four cardinal directions and while rotating the camera;
- the fix lives at one documented orientation boundary and does not add compensating 180-degree rotations across unrelated systems.

Implementation/verification: the sole correction is `blockbenchForwardCorrectionEuler` on the imported visual boundary in `KickLuckyCubePlayerController`. Play Mode verified a 180-degree local visual correction and that the active face geometry points along the player root's forward/travel direction; kick, camera, and prop directions remain on the unchanged root convention.

### KLC-PROD-029: Showcase The Equipped Kick Style Before Contact

- Status: `DONE` (2026-08-05)
- Priority: `P2`
- Owner: Camera/animation/gameplay engineering
- Depends on: `KLC-PROD-015`, `KLC-PROD-028`
- Source: manual review 2026-08-05, item 4

Problem: the current gameplay camera does not clearly show the purchased kick animation, so premium styles have weak visual value.

Acceptance criteria:

- starting a kick briefly frames the full character and cube from a readable three-quarter or style-specific angle before contact;
- the wind-up, main pose, contact, and initial cube launch remain visible without hiding hands, feet, or the cube;
- all six styles can be previewed through a deterministic developer/shop preview route without purchasing them repeatedly;
- showcase duration and camera offsets are configurable, with a shorter unobtrusive treatment available for the default kick;
- control locking, interruption, pause, repeated input, camera collision, and camera restoration are handled safely;
- the sequence remains readable at desktop and mobile-like aspect ratios and does not materially slow repeated progression.

Implementation/verification: every style now has camera yaw/distance/pitch/FOV/lead-in data, actual kicks use the same showcase motion, and every shop card has a deterministic `Preview` action. Play Mode invoked the real shop button and verified preview locking, unchanged ownership/equipped state, no cube launch, and complete camera restoration.

### KLC-PROD-030: Add Missing Wardrobe Icons

- Status: `DONE` (2026-08-05)
- Priority: `P2`
- Owner: UI/content engineering
- Source: manual review 2026-08-05, item 5

Problem: some clothing choices do not have useful icons, making different shop items difficult to identify before equipping or buying them.

Acceptance criteria:

- every selectable torso, legs, boots, gloves, hair, headwear, and mask entry has a unique readable thumbnail;
- icons are rendered from the actual current item, use consistent framing/background/lighting, and clearly distinguish male, female, and unisex variants where relevant;
- free, locked, owned, selected, and equipped overlays do not hide the item silhouette;
- missing or duplicate icon references fail an Editor catalog validation check instead of silently using an unrelated fallback;
- icons remain sharp and uncropped in desktop and narrow/mobile layouts;
- manually compare every icon with the equipped result for all wardrobe categories.

Implementation/verification: `KickLuckyCubeContentThumbnailGenerator` renders 23 transparent thumbnails from the current imported mannequin and selected item. Runtime UI validation walked all seven wardrobe categories and matched every card to its exact sprite; catalog validation rejects missing and duplicate icon references.

### KLC-PROD-031: Lighten The First Two Skin Tones

- Status: `DONE` (2026-08-05)
- Priority: `P2`
- Owner: Character art/content engineering
- Source: manual review 2026-08-05, item 6

Problem: the first and second skin-tone choices are currently too dark and need lighter base colors while remaining visibly distinct from one another.

Acceptance criteria:

- lighten skin-tone options 1 and 2 in their source textures rather than applying a runtime-only color override;
- preserve intentional face, ear, side, and shadow variation without creating seams across body parts;
- keep both options visibly distinct and leave the third existing natural tone unchanged unless a binding correction is required;
- verify male and female bodies in underwear and under representative torso, legs, gloves, hair, headwear, and mask combinations;
- inspect both tones under neutral preview lighting and representative gameplay lighting without clipping or washed-out highlights;
- free/default ownership and saved skin-tone selections continue to load correctly.

Implementation/verification: source textures 1 and 2 were changed to `#F6C39A` and `#CD9374` (including their side/shadow variants); tone 3 remains `#5C372B`. Runtime skin materials are now packaged under `Resources`, and Play Mode verified switching among the distinct light, warm, and deep material families without a runtime-only tint.

## Release Verification Gate

After all `P0` and `P1` tasks are `DONE`, run this complete acceptance route:

1. Start with a backed-up existing save and separately with a clean save.
2. Verify grounded player movement, correct facing, jump, camera orbit, zoom, UI opening, and mobile simulation.
3. Select a non-maximum kick strength, kick, verify cube/animal ground contact, watch the complete wave cinematic, then run and jump back home.
4. Place the animal, collect income, upgrade the stable, take it back, and sell another animal.
5. Buy/equip an early and premium dumbbell, inspect their models and exercise motion, train, claim x2, buy speed levels, buy/equip and clearly view a kick style, inspect wardrobe icons/skin tones, exchange a mob, activate weather, claim a reward, and inspect the elite shop.
6. Stop Play Mode, restart, and verify wallet, selected kick strength, tools, styles, animals, stable income, rewards, weather, and cooldowns.
7. Repeat critical UI checks at desktop and narrow mobile-like aspect ratios.
8. Run EditMode tests, catalog/prefab validation, a WebGL development build, and a browser smoke test.

Do not call the vertical slice production-ready while any `P0` task is open, currency can become negative, visible content is still authored only at runtime, or the complete route cannot be repeated after a save reload.
