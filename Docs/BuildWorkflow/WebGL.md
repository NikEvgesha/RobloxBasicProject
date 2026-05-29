# WebGL Baseline

`MechanicsTestbed` is the current WebGL mechanics validation target.

`StarterSandbox` remains the imported starter-scene baseline.

## Game Definition

The active game config lives at:

```text
Assets/Games/MechanicsTestbed/Configs/MechanicsTestbedGame.asset
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
Game: Mechanics Testbed
Game id: mechanics-testbed
Build output: Builds/WebGL/MechanicsTestbed
Template: APPLICATION:Default
Compression: Disabled
Data caching: Enabled
Target size: 960x600
Start scene: Assets/Games/MechanicsTestbed/Scenes/MechanicsTestbed.unity
Scenes:
  Assets/Games/MechanicsTestbed/Scenes/MechanicsTestbed.unity
```

## Applying Build Settings

In Unity, select:

```text
Assets/Games/MechanicsTestbed/Configs/MechanicsTestbedGame.asset
```

Then run:

```text
Roblox Basic Project > Game Definition > Apply Selected To Build Settings
```

This updates Unity Build Settings from the selected `GameDefinition`.

## Build Rule

Generated WebGL builds should go under ignored `Builds/` output folders, not into `Assets/` or `Tools/`.
