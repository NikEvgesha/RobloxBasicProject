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
