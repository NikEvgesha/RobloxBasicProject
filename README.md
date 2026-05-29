# Roblox Basic Project

Unity-monorepo for Roblox-style WebGL games.

This repository uses one Git repository for a shared game constructor and many concrete games:

```text
Assets/
  GameKit/        shared reusable systems and mechanics
  Games/          concrete games, one folder per game
  SharedArt/      assets reused by multiple games
  SharedAudio/    audio reused by multiple games
Docs/             architecture, workflow, mechanics, game notes
Tools/            build tools and automation helpers
```

## Branches

- `master` is intentionally empty. It exists as a clean entry branch for people who are new to the repository.
- `develop` is the active Unity development branch.
- `main` can be created later for stable released project states.
- `feature/*` is used for reusable mechanics or systems.
- `game/*` is used for work on a concrete game.
- `fix/*` is used for bug fixes.

Do not use long-lived branches as separate games. A game should live in `Assets/Games/{GameName}`.

## Core Rule

`Assets/GameKit` must not depend on `Assets/Games`.

Games may depend on `GameKit`. Games should not directly depend on each other. Move shared behavior into `GameKit` before reusing it in several games.

## Getting Started

From a fresh clone:

```powershell
git switch develop
```

Create a new game by copying the structure from `Assets/Games/_Template` into `Assets/Games/{GameName}` and adding game-specific documentation under `Docs/Games/{GameName}.md`.

Read the detailed rules:

- `AGENTS.md`
- `Docs/Architecture.md`
- `Docs/GitWorkflow.md`
- `Docs/GameKit/MechanicsIndex.md`
