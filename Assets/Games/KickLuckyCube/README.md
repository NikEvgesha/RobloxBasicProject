# Kick Lucky Cube

`KickLuckyCube` is a concrete game focused on kicking a lucky cube through rarity zones, escaping a wave as the spawned animal, and building an income stable.

Keep game-specific mechanics, scenes, prefabs, configs, art, and UI in this folder until at least two games need the same behavior.

## Current Target

- Keep the overview scene readable while mechanics are added incrementally.
- Current playable prototype: hold `E` at the kick line to kick the lucky cube.
- The real lucky cube is visible in Edit Mode for placement, but hides at Play Mode start and appears when the player kicks from the line.
- The scene starts with a controllable blocky prototype player and a third-person camera behind it.
- Move the prototype player with `WASD` / arrows, jump with `Space`, sprint with `Shift`, orbit with right mouse drag, and zoom with the mouse wheel.
- Prototype player animation clips and Animator live under `Assets/Games/KickLuckyCube/Animations`.
- The cube flies forward based on strength, lands in a rarity zone, spawns an animal, and starts the wave chase.
- Run the animal back to the return line before the wave catches it; on success, the prototype player carries the animal.
- Sell carried animals for soft currency or place them in stable slots.
- Stable animals generate soft currency over time; collect it on the green stable button.
- Stable slot animals and pending income save in PlayerPrefs during Play Mode.
- Spend soft on the blue speed station and yellow tool station.
- Select an owned tool in the bottom tool belt, train strength on the green training station, then kick farther into deeper rarity zones.
- After a training hold, claim the temporary `x2` prompt with `X` or by clicking it for bonus strength.
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
- Plot allocation is grouped under `KLC_PlotAllocationSystem`.
- Edit the reusable plot source at `KLC_PlotTemplate_EditSource`.
- Move `MOVE_PlotSlot_01..06` to define possible plot locations.
- At Play Mode start, one plot slot is randomly assigned to the player and the remaining eligible slots are filled with bot plots from the same template.
- Current plot allocation spawns visual plot instances only; gameplay triggers should be bound to the assigned player slot after final slot placement is approved.
- Older noisy prototype visual groups are disabled while the layout blockout is being placed.

## Core Loop Draft

1. Train strength with a selected tool.
2. Spend soft currency on animal speed upgrades before kicking.
3. Kick the lucky cube from the start line.
4. Cube flies through rarity zones and lands in one zone.
5. The landed cube spawns a controllable animal from that zone's rarity pool.
6. A wave starts behind the animal.
7. Return to the kick line before the wave catches the animal.
8. Carry the animal back as the player.
9. Sell it for currency or place it in a stable.
10. Stable animals generate soft currency over time.
11. Spend soft on animal speed or the next strength tool.
12. Train strength with the current tool, claim optional `x2` prompts, and kick farther.
13. Rebirth resets progression for a higher money multiplier.

## Boundaries

This game may use shared systems from `Assets/GameKit`, but `Assets/GameKit` must not reference this folder.

Promote only proven reusable systems into `Assets/GameKit` and document them in `Docs/GameKit/MechanicsIndex.md`.
