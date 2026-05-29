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

No reusable mechanics have been registered yet.

## Foundation Candidates

The imported foundation under `Assets/Igrodelnya` contains systems that may later become reusable mechanics, such as input, currency, inventory, localization, quests, rewards, roulette, leaderboard, sound, tutorial, and shop UI.

Keep them in place until each system is reviewed and decoupled. Only move a system into `Assets/GameKit` when it no longer depends directly on game-specific scenes, prefabs, UI composition, or assets.

Detailed audit:

```text
Docs/Reviews/FoundationSystemsAudit.md
```

Current promotion priority:

1. Fix WebGL build blockers in imported runtime scripts.
2. Fix inventory initialization before item rewards are used.
3. Promote isolated UI helpers first, starting with `DynamicGridSpawner`.
4. Define `GameKit` input/save/currency contracts before moving larger systems.
