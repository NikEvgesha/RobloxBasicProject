# Kick Lucky Cube

`KickLuckyCube` is a concrete game focused on kicking a lucky cube through rarity zones, escaping a wave as the spawned animal, and building an income stable.

Keep game-specific mechanics, scenes, prefabs, configs, art, and UI in this folder until at least two games need the same behavior.

## Current Target

- Keep the overview scene readable while mechanics are added incrementally.
- Current playable prototype: enter the kick zone to show the lucky cube in the player's hands, press `E` to start the power meter, then press `E` again to kick from that position with the selected power.
- The real lucky cube is visible in Edit Mode for placement, but hides at Play Mode start and appears in the player's `KLC_CarryAnchor` while the player stands in the kick zone.
- The scene starts with a controllable blocky prototype player and a third-person camera behind it.
- Move the prototype player with `WASD` / arrows, jump with `Space`, sprint with `Shift`, orbit with right mouse drag, and zoom with the mouse wheel.
- Prototype player animation clips and Animator live under `Assets/Games/KickLuckyCube/Animations`.
- The cube flies forward based on strength, uses a trail FX, lands on the lower corridor floor, disappears, then starts the animal roulette before the wave chase.
- During the kick flow, the camera follows the flying cube, then the roulette preview, then the spawned animal.
- Run the animal back to the kick start point before the wave catches it; on success, the prototype player carries the animal.
- On successful return, the animal is added to the runtime inventory: first four mobs fill the bottom bar, then extra mobs go into the inventory window.
- The bottom inventory bar keeps the current training tool in slot 1 and up to four mobs in slots 2-5.
- Empty bottom mob slots are hidden during normal gameplay and shown only while the inventory window is open as drop targets.
- Open the temporary inventory window with `I` or the `Bag` button, then drag mobs between the inventory window and the bottom bar.
- The inventory window shows only real item slots; dropping a bottom-bar mob onto empty window space creates the next item slot there.
- Selecting a mob slot shows that mob as a temporary hand preview under `KLC_CarryAnchor`.
- Clicking or pressing the already-selected mob slot clears selection and removes the hand preview.
- Walk to the animal sell kiosk pad to show the `Open sell shop` prompt; press `E` to open the larger sell window with square mob cards in a 3-column scroll grid; the window closes automatically when leaving the sell zone.
- Final inventory persistence and stable placement from inventory are still pending.
- The old tool belt UI is hidden in Play Mode; click slot 1 or press `1` to start/stop training with the selected tool in-hand.
- Stable animals generate soft currency over time; collect it on the green stable button.
- Stable slot animals and pending income save in PlayerPrefs during Play Mode.
- Spend soft on the blue speed station and yellow tool station.
- Train strength by toggling the selected bottom-bar tool: named tool tiers (dumbbell, kettlebell, barbell, and stronger variants) appear in hand, make the player squat, grant tier-based strength every second, and periodically create one `x2` claim circle.
- During tool training, claim the temporary `x2` prompt with `X` or by clicking it; a new circle will not spawn while one is already visible.
- Open Shop from the left-side button to buy speed, tool, and strength boost prototype cards.
- Open Rewards from the left-side button to claim playtime soft/hard rewards.
- Open Wheel from the left-side button to spin for soft, hard, or strength rewards.
- Open the Rebirth window from the left-side Rebirth button after reaching the strength requirement.
- First ProBuilder visual pass is grouped under `KLC_ProBuilderVisuals` in the overview scene.
- The ProBuilder layer is visual-only and should not hold gameplay references or colliders.
- First UI visual pass is grouped under `KLC_UIBackplates`, `KLC_UIVisualPass`, and `KLC_UIWindows`.
- Shop and Settings side buttons open their visual windows; close them through the red close button, backdrop click, or `Escape`.
- Settings rows toggle Music, SFX, and Language UI state; the first audio/localization backend is wired, with final full-text coverage still pending.
- Shop cards now drive first-pass purchase gameplay logic.
- Mobile controls are grouped under `KLC_MobileControls`: joystick, jump button, and hold-interact button, hidden on desktop by default.
- Camera controls: hold right mouse to orbit, use mouse wheel to zoom.
- Fake online ambience is grouped under `KLC_FakeOnline`: neighboring plots, upgraded mobs, and animated bots.
- The fake online layer is visual-only and has no colliders under its root.
- Current location layout pass is grouped under `KLC_LayoutBlockout_Workspace`.
- The location blockout meshes are editable `ProBuilderMesh` objects with scale baked into geometry.
- Move only objects whose names start with `MOVE_`; the floor, walls, and river/zone guides are baseline ProBuilder blockout references.
- `MOVE_PlayerSpawn_BlueCube` is the player spawn marker; `KickLuckyCubePlayerSpawnController` snaps `KLC_PrototypePlayer` to it on scene start.
- `MOVE_KickLine_YellowBar` defines the current kick line; `KLC_KickLineInteractionTrigger` is aligned just before it for press-to-aim and shows the cube-in-hands preview while the player is inside it.
- `KLC_PlayerKickBoundary` is an invisible player-only limiter after the kick line; spawned animals are not clamped by it while returning.
- Plot allocation is grouped under `KLC_PlotAllocationSystem`.
- Edit the reusable plot source at `KLC_PlotTemplate_EditSource`.
- `Template_PlotGround` previews the plot footprint and should match the scaled `MOVE_PlotSlot_*` footprint.
- Current plot template follows the reference base layout: tan room floor, center aisle, two five-slot stable rows, front boards, and green interaction pads.
- The plot template intentionally has no owner tint strip, player spawn pad, sell pad, or decorative boundary walls.
- Reusable root modules are grouped as `Template_FrontBoards`, `Template_CenterAisleGroup`, `Template_BackWallGroup`, and `Template_FloorExpansion`.
- `Template_BaseUpgradeBoard` is the placeholder for buying upward base expansion / extra floors.
- `Template_CollectAllAdRewardBoard` is the placeholder for the ad-gated collect-all reward board.
- Each of the ten `Template_StableSlot_*` objects is a grouped stable module with a matching child `MobAnchor`, `CollectSpot` placeholder, and `UpgradeBoard` booster placeholder.
- `Template_FloorExpansion` contains visual-only posts, ladder, and outline beams for future upper-floor expansion.
- Move `MOVE_PlotSlot_01..04` to define placed plot locations.
- `KLC_PlotSlot_StaticInstances` contains edit-mode plot copies placed from the reusable template on every current `MOVE_PlotSlot`.
- `KLC_PlotTemplate_EditSource` is kept inactive as the reusable edit source; enable it only when editing the template shape.
- Runtime plot auto-allocation is currently disabled to avoid duplicate plots while static scene placement is being tuned.
- `KLC_HubKiosks_Blockout/KLC_ImmediateKiosks_LeftToRight` holds the current left-to-right hub kiosks: animal sell, style shop, speed upgrade, weights training, and leaderboard.
- Sell, speed, and weights kiosks have visible stand pads; style shop uses a hold-interaction anchor without a visible stand pad.
- `KLC_HubKiosks_Blockout/KLC_FutureFeatureSpots` reserves visual-only spots for weather machine, animal exchange, epic mob shop, and rating gift stand.
- Plot template, placed plot copies, and hub kiosk blockout meshes are converted to `ProBuilderMesh`; `TextMesh` labels remain regular text objects.
- Older noisy prototype visual groups are disabled while the layout blockout is being placed.

## Core Loop Draft

1. Train strength with a selected tool.
2. Spend soft currency on animal speed upgrades before kicking.
3. Enter the kick zone so the lucky cube appears in the player's hands.
4. Kick the lucky cube from the selected position and power value.
5. Camera follows the cube through rarity zones; the cube lands on the lower corridor floor and disappears.
6. Roulette cycles shadow silhouettes from the landed rarity pool and selects one random animal.
7. A controllable animal spawns from the roulette result.
8. A wave starts behind the animal only after the roulette finishes.
9. Return to the kick start point before the wave catches the animal.
10. Add the returned animal to the bottom bar or inventory window.
11. Drag mobs between the full inventory and the four mob slots on the bottom bar.
12. Sell inventory mobs for currency through the sell kiosk or place them in a stable. Stable placement from inventory is pending.
13. Stable animals generate soft currency over time.
14. Spend soft on animal speed or the next strength tool.
15. Toggle the current tool to train strength per second, claim optional `x2` prompts, and kick farther.
16. Rebirth resets progression for a higher money multiplier.

## Boundaries

This game may use shared systems from `Assets/GameKit`, but `Assets/GameKit` must not reference this folder.

Promote only proven reusable systems into `Assets/GameKit` and document them in `Docs/GameKit/MechanicsIndex.md`.
