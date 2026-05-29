# Starter Sandbox

## Game

Name: Starter Sandbox

Game id: starter-sandbox

Status: prototype

## Concept

Working sandbox for the imported starter scenes from `UnityBasicProject`. Use this area for validating the baseline Unity project, MCP workflow, input, WebGL settings, and early reusable mechanic candidates.

## Scenes

```text
Assets/Games/StarterSandbox/Scenes/Loading.unity
Assets/Games/StarterSandbox/Scenes/Evgesha.unity
```

These scenes are included in Unity Build Settings.

## Used GameKit Mechanics

No promoted `GameKit` mechanics are registered yet.

Shared input action asset:

```text
Assets/GameKit/Runtime/Input/InputSystem_Actions.inputactions
```

## Game-Specific Mechanics

Prototype behavior should stay inside:

```text
Assets/Games/StarterSandbox/
```

Move behavior to `Assets/GameKit` only after it is reusable across multiple games and documented in `Docs/GameKit/MechanicsIndex.md`.

## Input

Desktop: use the shared input action asset as the baseline.

Mobile: use the same shared input path for touch/mobile mappings; document specific UI controls when added.

## WebGL Notes

This is the default validation target for WebGL builds until separate games are created.

## Mobile Notes

Keep mobile-specific experiments local to this game until they become generic enough for `GameKit`.

## Build Notes

Unity Build Settings currently point to the `StarterSandbox` scenes.
