# Agent Rules

These rules are mandatory for automated agents and recommended for human contributors.

## Repository Model

This is a Unity monorepo:

- Shared reusable code goes in `Assets/GameKit`.
- Concrete games go in `Assets/Games/{GameName}`.
- Shared assets go in `Assets/SharedArt` or `Assets/SharedAudio`.
- Imported SDK, MCP, plugin, and vendor folders should stay where they are unless a task explicitly asks to migrate them.
- Build and automation helpers go in `Tools`.
- Project and mechanic documentation goes in `Docs`.

## MCP Rules

- This repository uses `.codex/config.toml` for the Unity MCP connection.
- Prefer AI Game Developer `Custom` mode with the local MCP server at `http://localhost:26124`.
- Custom mode with `Authorization Token: none` does not need an MCP bearer token.
- If Cloud mode or required authorization is enabled, use `ROBLOX_BASIC_PROJECT_GAME_DEV_AUTH_TOKEN` for this project.
- Do not use the shared `GAME_DEV_AUTH_TOKEN` variable here.
- If Codex Desktop needs a user-level MCP entry, use a unique server name such as `ai-game-developer-roblox-basic-project`.
- Do not overwrite existing `ai-game-developer` entries in the user-level Codex config.
- Never commit MCP auth tokens or credentials.

## Dependency Rules

- `Assets/GameKit` must never reference files, scripts, prefabs, scenes, or configs from `Assets/Games`.
- `Assets/Games/{GameName}` may reference `Assets/GameKit`.
- One game folder should not reference another game folder.
- If two games need the same behavior, extract that behavior into `Assets/GameKit`.
- If a mechanic is generic, document it in `Docs/GameKit/MechanicsIndex.md`.
- If a mechanic is game-specific, document it in `Docs/Games/{GameName}.md`.

## Branch Rules

- `master` stays empty except for its initial empty commit.
- Work happens on `develop` or short-lived branches created from `develop`.
- Use `feature/{mechanic-or-system}` for reusable systems.
- Use `game/{game-name}-{task}` for game-specific work.
- Use `fix/{issue}` for focused fixes.
- Do not create a long-lived branch per game.

## Unity Rules

- Keep Unity serialization in text mode.
- Commit `.meta` files together with their assets.
- Avoid editing unrelated scenes, prefabs, and ProjectSettings.
- Prefer prefabs, ScriptableObjects, and additive scenes over large monolithic scenes.
- Avoid hard references from `GameKit` to concrete game content.
- Before moving a game-specific system into `GameKit`, remove direct dependencies on game scenes, UI, and assets.
- Do not create assets without their `.meta` files.
- Keep hidden `.gitkeep` files only for intentionally empty folders.

## Documentation Rules

Update documentation in the same change when adding or changing:

- a reusable mechanic;
- a game-specific mechanic;
- a build workflow;
- input behavior;
- WebGL or mobile behavior;
- repository structure.

## Expected Change Shape

For a new reusable mechanic:

1. Add runtime code under `Assets/GameKit`.
2. Add optional editor tools under `Assets/GameKit/Editor`.
3. Add tests or test notes under `Assets/GameKit/Tests`.
4. Update `Docs/GameKit/MechanicsIndex.md`.

For a new game:

1. Copy `Assets/Games/_Template` to `Assets/Games/{GameName}`.
2. Add game scenes under `Assets/Games/{GameName}/Scenes`.
3. Add game scripts under `Assets/Games/{GameName}/Scripts`.
4. Add game configs under `Assets/Games/{GameName}/Configs`.
5. Add documentation under `Docs/Games/{GameName}.md`.
