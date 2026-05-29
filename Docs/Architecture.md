# Architecture

The repository is organized as a single Unity project with multiple games and one shared constructor.

## Top-Level Layout

```text
Assets/
  GameKit/
    Runtime/
    Mechanics/
    Editor/
    Tests/
  Games/
    _Template/
    {GameName}/
  SharedArt/
  SharedAudio/
Docs/
Tools/
```

## GameKit

`Assets/GameKit` contains reusable systems:

- character controllers;
- camera systems;
- desktop and mobile input;
- interaction contracts;
- shared UI primitives;
- WebGL helpers;
- save/progress abstractions;
- reusable mechanics such as obby, tycoon, pets, combat, vehicles.

`GameKit` should be reusable by any game in this repository.

## Games

Each concrete game lives in:

```text
Assets/Games/{GameName}
```

Recommended structure:

```text
Assets/Games/{GameName}/
  Scenes/
  Prefabs/
  Scripts/
  Art/
  Audio/
  Configs/
  UI/
  README.md
```

Game folders contain only game-specific content: levels, balance, UI composition, game-specific scripts, local assets, and game documentation.

## Direction Of Dependencies

Allowed:

```text
Assets/Games/ObbyRunner -> Assets/GameKit
Assets/Games/PetTycoon  -> Assets/GameKit
Assets/GameKit          -> Assets/SharedArt, when truly generic
```

Forbidden:

```text
Assets/GameKit          -> Assets/Games/ObbyRunner
Assets/Games/ObbyRunner -> Assets/Games/PetTycoon
```

## Game Definition

Each game should eventually have a `GameDefinition` ScriptableObject under:

```text
Assets/Games/{GameName}/Configs/{GameName}Game.asset
```

It should describe:

- game id;
- display name;
- start scene;
- scenes included in the build;
- input profile;
- camera profile;
- UI theme;
- enabled mechanics;
- WebGL build settings;
- version/build id.

The goal is to let tools build a selected game without manually editing Unity Build Settings.

## Promotion Rule

Start game-specific. Promote to `GameKit` only after the mechanic proves reusable.

Before promotion:

- remove direct references to game scenes and prefabs;
- move config into ScriptableObjects or serializable settings;
- expose events/interfaces instead of hardcoded game behavior;
- document the mechanic in `Docs/GameKit/MechanicsIndex.md`.
