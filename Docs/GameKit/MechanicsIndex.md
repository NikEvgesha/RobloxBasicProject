# GameKit Mechanics Index

This file is the registry of reusable mechanics.

When a mechanic becomes reusable across games, add it here.

## Template

```text
Name:
Path:
Status: draft | usable | stable | deprecated
Owner:
Used by:
Purpose:
Public API:
Required prefabs:
Required ScriptableObjects:
Input requirements:
WebGL notes:
Mobile notes:
Known limits:
```

## Current Mechanics

Name: Press/Hold Interaction
Path: `Assets/GameKit/Runtime/Interaction`
Status: draft
Owner: Shared GameKit
Used by: `Assets/Games/MechanicsTestbed`, `Assets/Games/KickLuckyCube`
Purpose: Provide a reusable Roblox-style interaction contract where independent sources nominate interaction targets and a central driver handles prompt display, press/hold activation, optional hold progress, and invocation.
Public API: `GameKitInteractionTarget`, `GameKitInteractionActivationMode`, `GameKitInteractionCondition`, `GameKitInteractionDriver`, `GameKitInteractionPromptView`, `GameKitInteractionTriggerSource`, `GameKitInteractionRaycastSource`. `GameKitInteractionTarget.SetPrompt` and `SetPromptText` can update a target's visible prompt at runtime while preserving the same target object.
Required prefabs: none yet.
Required ScriptableObjects: none.
Input requirements: keyboard `E` through the active Input System path, with old-input fallback only when legacy input is enabled.
WebGL notes: uses frame-based raycast/trigger checks and unscaled hold progress; no platform-specific APIs.
Mobile notes: `GameKitInteractionDriver.SetExternalHold` can be wired to a virtual button; press-mode targets fire on the rising edge, hold-mode targets keep using progress.
Known limits: first draft only supports one selected target at a time; prompt view is UGUI-specific; no generated prefab yet.

`Assets/Games/MechanicsTestbed` still contains game-local prototypes for desktop/mobile movement, camera, wallet, settings, UI, wheel, timed rewards, and the concrete pickup/drop/shop test actions. They are intentionally not registered here until they are decoupled and promoted to `Assets/GameKit`.

## Foundation Candidates

The imported foundation under `Assets/Igrodelnya` contains systems that may later become reusable mechanics, such as input, currency, inventory, localization, quests, rewards, roulette, leaderboard, sound, tutorial, and shop UI.

Keep them in place until each system is reviewed and decoupled. Only move a system into `Assets/GameKit` when it no longer depends directly on game-specific scenes, prefabs, UI composition, or assets.

Detailed audit:

```text
Docs/Reviews/FoundationSystemsAudit.md
```

Current promotion priority:

1. Promote isolated UI helpers first, starting with `DynamicGridSpawner`.
2. Define `GameKit` input/save/currency contracts before moving larger systems.
3. Wrap legacy `G` access behind game/foundation adapters.
4. Migrate input adapters because desktop/mobile control is central to the project goal.
