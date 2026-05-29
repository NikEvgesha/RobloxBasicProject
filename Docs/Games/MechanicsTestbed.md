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

No promoted `GameKit` mechanics are registered yet.

Shared input action asset:

```text
Assets/GameKit/Runtime/Input/InputSystem_Actions.inputactions
```

## Game-Specific Mechanics

Current prototype systems:

- third-person character movement;
- right-mouse third-person camera orbit;
- PlayerPrefs save baseline;
- soft and hard currency wallet;
- settings values for music, SFX, and language;
- shop prototype grants;
- wheel prototype reward;
- timed hard-currency reward.

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
- hold right mouse button: rotate camera.

Mobile:

- not implemented yet;
- expected future direction is a virtual joystick plus drag camera zone.

## UI Style

Current test style:

- compact dark top HUD and side panel;
- soft currency accent: green;
- hard currency accent: cyan;
- action accent: amber;
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

## Mobile Notes

Keep mobile input experiments local to this game until the final input contract is stable.
