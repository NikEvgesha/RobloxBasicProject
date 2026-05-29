# Architecture

The repository is organized as a single Unity project with multiple games and one shared constructor.

## Top-Level Layout

```text
Assets/
  GameKit/
    Runtime/
      Input/
    Mechanics/
    Editor/
    Tests/
  Games/
    _Template/
    StarterSandbox/
    {GameName}/
  SharedArt/
    UI/
      Sprites/
  SharedAudio/
Docs/
Tools/
```

## Prepared Working Layout

The project is currently prepared for work with these concrete areas:

```text
Assets/GameKit/Runtime/Input/
  InputSystem_Actions.inputactions

Assets/GameKit/Runtime/Configuration/
  GameDefinition.cs

Assets/Games/StarterSandbox/
  Configs/
    StarterSandboxGame.asset
  Scenes/
    Loading.unity
    Evgesha.unity
  Art/
  Audio/
  Configs/
  Prefabs/
  Scripts/
  UI/

Assets/Games/MechanicsTestbed/
  Configs/
    MechanicsTestbedGame.asset
  Scenes/
    MechanicsTestbed.unity
  Scripts/
    game-local prototypes for movement, camera, wallet, settings, HUD, rewards

Assets/SharedArt/UI/Sprites/
  shared UI frames, icons, language flags, and currency sprites
```

`StarterSandbox` contains the scenes imported from the original `Assets/Scenes` root. Use it as the first editable sandbox game, not as reusable kit code.

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

## Imported Foundation

The current Unity project was imported from:

```text
E:\GitFork\UnityBasicProject
```

Existing foundation, SDK, MCP, and vendor content may remain in folders such as:

```text
Assets/Igrodelnya
Assets/OpalStudio
Assets/Plugin
Assets/Plugins
Assets/Resources
Assets/Settings
Assets/TextMesh Pro
Assets/TutorialInfo
Assets/ProBuilder Data
```

Do not move these folders just to fit the new structure. Move or wrap them only during a focused migration when the dependency direction is clear.

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

## Global Access

`Assets/Igrodelnya/G.cs` is a legacy service locator for the imported foundation. Keep it working while the foundation is being audited, but do not treat it as the long-term architecture for new reusable systems.

Do not create a large shared `G` that contains every game. That would make the monorepo compile-time coupled across products.

Rules:

- `Assets/GameKit` must not depend on `G`, `StarterSandboxG`, or any other game/global facade.
- `Assets/Igrodelnya` may keep using the current `G` until each system is migrated or wrapped.
- `Assets/Games/{GameName}` may define a small `{GameName}G` facade only for game-local scene/UI composition.
- A game-local facade must not reference another game folder.
- Shared behavior should move behind `GameKit` interfaces or ScriptableObject config instead of becoming another static global.

Preferred migration path:

```text
Legacy G -> game/foundation adapter -> GameKit interface -> reusable implementation
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

The first baseline config is:

```text
Assets/Games/StarterSandbox/Configs/StarterSandboxGame.asset
```

The current mechanics validation config is:

```text
Assets/Games/MechanicsTestbed/Configs/MechanicsTestbedGame.asset
```

## Promotion Rule

Start game-specific. Promote to `GameKit` only after the mechanic proves reusable.

Before promotion:

- remove direct references to game scenes and prefabs;
- move config into ScriptableObjects or serializable settings;
- expose events/interfaces instead of hardcoded game behavior;
- document the mechanic in `Docs/GameKit/MechanicsIndex.md`.
