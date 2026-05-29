# Mechanics Testbed

## Game

Name: Mechanics Testbed

Game id: mechanics-testbed

Status: prototype

## Concept

Standalone test game for validating Roblox-style WebGL mechanics before extracting them into `Assets/GameKit`.

## Scenes

```text
Assets/Games/MechanicsTestbed/Scenes/MechanicsTestbed.unity
```

## Config

```text
Assets/Games/MechanicsTestbed/Configs/MechanicsTestbedGame.asset
```

## Used GameKit Mechanics

Hold Interaction:

```text
Assets/GameKit/Runtime/Interaction
```

Shared input action asset:

```text
Assets/GameKit/Runtime/Input/InputSystem_Actions.inputactions
```

## Game-Specific Mechanics

Current prototype systems:

- third-person character movement;
- smoothed right-mouse third-person camera orbit;
- mouse wheel camera zoom;
- mobile virtual joystick;
- mobile sprint and jump buttons;
- mobile right-side camera drag;
- desktop-hidden mobile controls with runtime mobile/handheld visibility;
- generic block-character Animator with idle, move, walk, and run clips;
- ProBuilder environment blockout inspired by a Roblox-style yard from the local reference video;
- PlayerPrefs save baseline;
- soft and hard currency wallet;
- settings values for music, SFX, and language;
- shop prototype grants;
- wheel prototype reward;
- timed hard-currency reward.
- interaction test actions: raycast pickup cube, trigger drop zone, and trigger shop opener.

These stay in:

```text
Assets/Games/MechanicsTestbed/Scripts/
```

Promote only proven reusable parts to `Assets/GameKit`.

## Input

Desktop:

- `WASD` / arrows: move;
- `Shift`: sprint;
- `Space`: jump;
- hold `E`: complete the active interaction prompt;
- hold right mouse button: rotate camera;
- mouse wheel: zoom camera in and out.

Mobile:

- left virtual joystick: move;
- `RUN`: hold sprint;
- `JUMP`: jump;
- drag the right side of the screen: rotate camera.

Mobile controls are hidden on desktop by `MechanicsTestbedMobileControlsVisibility` and are shown on mobile or handheld platforms. Use the component's `forceVisible` option only for editor testing.

## Interaction Test Flow

The scene contains `MT_InteractionTests` to validate the shared `GameKit` hold-interaction system:

- look at `MT_InteractionPickupCube` and hold `E` to pick it up;
- the cube is parented to `MT_Player/CarryAnchor`, colliders are disabled, and the player arms are held up while carrying;
- step into `MT_InteractionDropZone` while carrying and hold `E` to place the cube;
- approach `MT_InteractionShopKiosk` and hold `E` to open the existing shop panel.

Detection sources are intentionally separate from the prompt/input driver:

- pickup cube uses `GameKitInteractionRaycastSource` from `MT_ThirdPersonCamera`;
- drop zone and shop use `GameKitInteractionTriggerSource`;
- all three actions resolve through `GameKitInteractionDriver` on `MT_Player`.

## Character Animation

The prototype player uses a game-local Generic Animator controller:

```text
Assets/Games/MechanicsTestbed/Animations/MechanicsTestbedPlayer.controller
```

Animator parameters are driven by `MechanicsTestbedThirdPersonController`:

- `MoveSpeed`;
- `IsMoving`;
- `IsSprinting`;
- `Grounded`.

Current clips are simple block-rig placeholders for validating state flow:

- `MechanicsTestbedIdle.anim`;
- `MechanicsTestbedMove.anim`;
- `MechanicsTestbedWalk.anim`;
- `MechanicsTestbedRun.anim`.

All four clips use `loopTime = true` and `WrapMode.Loop` so Mecanim states can repeat continuously.

Keep these under `Assets/Games/MechanicsTestbed` until the shared block-character rig and animation contract are stable enough to promote into `Assets/GameKit`.

## UI Style

Current test style:

- shared sprite source: `Assets/SharedArt/UI/RobloxCasual`;
- compact dark top HUD with coin and diamond currency badges;
- casual blue, green, yellow, purple, and red button sprites;
- light status/action panels with dark readable text;
- no nested cards or decorative background-only elements.

This style is game-local until we decide whether to promote it into a shared `GameKit` UI theme.

## Localization

The current scene stores a `ru` / `en` language code in settings only.

Next localization step should define:

- localization data format;
- key naming rules;
- fallback language;
- whether text lives in Unity Localization package, ScriptableObjects, or a custom table.

## WebGL Notes

This game is intended to be the current WebGL mechanics validation target.

## ProBuilder Notes

The scene contains `PB_VideoInspiredArena`, a game-local ProBuilder blockout with:

- studded green baseplate;
- tall tan checker walls with green caps and drips;
- wooden walkway and spawn pad;
- shop/reward/run-upgrade stalls;
- blocky trees and pet display statues;
- reward question cube.

Do not promote this scene geometry into `GameKit`; extract only reusable level-building tools or prefabs later if another game needs them.

## Mobile Notes

Mobile input is implemented as a game-local prototype. Keep it here until the final input contract is stable enough to promote into `Assets/GameKit`.
