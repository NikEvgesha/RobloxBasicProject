# Kick Lucky Cube Authoring Tools

Updated: 2026-08-11

Open the main workspace from:

```text
Tools > Kick Lucky Cube > Authoring Workspace
```

Use `Install / Repair Tools` after pulling changes or replacing a core prefab. It is idempotent: it creates missing folders/configs/contracts and rebinds known scene objects without rebuilding the world.

## Safety Rules

- Do not use `Apply World Polish` for normal manual editing. It is a broad legacy generator.
- Edit one scene root or prefab at a time.
- Use `Create Connected Prefab` only after selecting one complete root.
- The corridor preview replaces only `KLC_AuthoringBiomeInstances`; it does not delete current corridor art.
- Do not delete original scene art until the connected prefab instance has been checked in Play Mode.
- Run `Validate All` before handing a prefab back to engineering.

## Camera Choreography

Config asset:

```text
Assets/Games/KickLuckyCube/Resources/KickLuckyCube/KickLuckyCubeCameraChoreographyConfig.asset
```

The sequence is explicit:

```text
Roulette -> Spawn runner -> Wave Reveal shot -> Runner Ready shot -> Ground runner -> Start chase
```

Each shot exposes focus offset, distance, pitch, yaw basis/offset, transition time, hold time, FOV, easing, and snap-on-enter. Select a wave/runner target in the Hierarchy and use `Preview Wave Shot` or `Preview Runner Shot` to align Scene View without changing the game camera or scene transforms.

`Direction To Home` is the normal yaw basis for the runner-ready shot. The chase and wave movement do not start until both configured shots finish.

Normal follow/orbit smoothing is configured on `KLC_OverviewCamera` through `KickLuckyCubeThirdPersonCamera`. Use `followSharpness` for target tracking, `targetSwitchSharpness` for player/cube/roulette/runner transfers, `orbitRotationSharpness` for right-mouse rotation, and the separate collision retract/restore sharpness fields for obstruction response. Do not add another position interpolation outside this component.

## Biomes And Corridor

The scene contains a non-visual functional root named `KLC_CorridorGameplayData_DoNotDelete`. It owns the 30 trigger-only rarity/location zones used to select the correct animal pool after a kick, plus the corridor authoring and preview anchors. It contains no final biome art. Do not delete it while cleaning decorative scene objects. `Install / Repair Tools` recreates this data root, the player-only kick-line boundary, and the editable `KLC_ExtendedCorridorFloor_30Zones` ProBuilder fallback floor if any of them are missing.

`KLC_ExtendedCorridorFloor_30Zones` is the temporary authoritative collider while the 30 final biome prefabs are incomplete. Without it, a long kick can use the legacy floor renderer bounds as a fictitious landing height and the runner will drop when it reaches the remaining short MeshCollider. A final biome sequence may replace this fallback only after every location supplies continuous validated gameplay-floor collision.

1. Select one complete biome root in the scene.
2. Click `Biome > Add / Auto-bind`.
3. Assign/fix gameplay floor, rarity zone, start/end anchors, landmarks, details, and VFX roots.
4. Click `Create Connected Prefab` and save under `Prefabs/World/Biomes`.
5. Repeat for each biome.
6. Select `KLC_CorridorGameplayData_DoNotDelete` and fill its `KickLuckyCubeCorridorAuthoring` sequence with 30 biome prefabs.
7. Click `Build Non-destructive Preview` to inspect spacing and orientation.

The biome inspector draws floor and start/end guides. Decoration may be non-colliding, but the assigned gameplay floor must have a collider.

## World Shops

The overview scene has authoring contracts on eight roots: Sell, Styles, Speed, Training Tools, Exchange, Elite Mobs, Weather, and Rating Gift. Each contract stores visual root, interaction anchor, sign anchor, window prefab, interaction binding, and optional ready/active/claimed preview groups.

Select a complete kiosk root and use `World shop > Create Connected Prefab`. Save under `Prefabs/World/Shops`. The inspector buttons switch preview states without entering Play Mode.

## Player Base And Home Marker

`KLC_PlotTemplate_EditSource` has a `KickLuckyCubePlayerBaseAuthoring` contract and explicit Home Marker, Owner Label, Interaction, and Decoration anchors. Stable slots must become children of the final player-base prefab before its contract can pass validation.

Use `Player base > Create Connected Prefab` and save under `Prefabs/World/PlayerBase`. The runtime allocator still assigns player/bot ownership and persistence after instantiation.

The home marker now targets `HomeMarkerAnchor` when available. Its Inspector controls offset, icon size, screen-edge clamp, visible distance, and distance-based scale. The overlay marker remains visible through world walls.

## UI Windows

The workspace opens the functional prefabs for Speed, Training Tools, Exchange, Elite Mobs, Weather, Offline Reward, Sell, and Style shops directly in Prefab Mode.

Every listed root has `KickLuckyCubeUiAuthoringPreview`:

- select a preview state such as Affordable, Owned, Equipped, Locked, Cooldown, or Claimed;
- configure show/hide object groups for that state;
- configure explicit TMP text samples, or use `Fill Sample Values` for common price/level/income/status placeholders;
- edit static hierarchy and layout in the prefab; controllers continue to write runtime values only.

## Lucky Cube

`KLC_LuckyCube.prefab` has a `KickLuckyCubeLuckyCubeAuthoring` contract with geometry/edge ownership, six question-mark face roots, gameplay collider, style renderers, and Carry/Trail/Impact VFX anchors. The selected inspector displays anchor gizmos and validates the six face roots.

## Training Animation

`KLC_PrototypePlayer` has a `KickLuckyCubeTrainingAnimationAuthoring` contract. Assign the final preview clip, tool root, two grip anchors, hand transforms, and foot-contact transforms. The inspector can:

- sample any normalized point of the assigned clip with Unity Animation Mode;
- display left/right grip error in meters;
- align the tool to the current hand positions;
- draw hand grip and foot-contact gizmos;
- stop the animation preview without entering Play Mode.

The current blockout player does not yet contain dedicated hand/foot bones, so these references intentionally remain validation issues until the human rig/animation pass.

## Validation

Run:

```text
Tools > Kick Lucky Cube > Validation > Validate Authoring Contracts
```

The validator checks camera config, authored prefab contracts, UI preview components, Lucky Cube anchors, scene shop bindings, corridor sequence, player-base anchors/slots, and training rig references. Warnings are the concrete remaining human assignments, not silent runtime fallbacks.
