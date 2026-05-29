# WebGL Baseline

`StarterSandbox` is the default WebGL validation target until a concrete game becomes the active target.

## Game Definition

The baseline game config lives at:

```text
Assets/Games/StarterSandbox/Configs/StarterSandboxGame.asset
```

It describes:

- game id and display name;
- start scene;
- scenes included in Build Settings;
- shared input profile;
- WebGL output expectations;
- mobile input expectations.

## Current Baseline

```text
Game: Starter Sandbox
Game id: starter-sandbox
Build output: Builds/WebGL/StarterSandbox
Template: APPLICATION:Default
Compression: Disabled
Data caching: Enabled
Target size: 960x600
Start scene: Assets/Games/StarterSandbox/Scenes/Loading.unity
Scenes:
  Assets/Games/StarterSandbox/Scenes/Loading.unity
  Assets/Games/StarterSandbox/Scenes/Evgesha.unity
```

## Applying Build Settings

In Unity, select:

```text
Assets/Games/StarterSandbox/Configs/StarterSandboxGame.asset
```

Then run:

```text
Roblox Basic Project > Game Definition > Apply Selected To Build Settings
```

This updates Unity Build Settings from the selected `GameDefinition`.

## Build Rule

Generated WebGL builds should go under ignored `Builds/` output folders, not into `Assets/` or `Tools/`.
