# Git Workflow

## Branches

`master`

Intentionally empty. It is used as a clean entry branch. Do not add Unity project files to this branch.

`develop`

Active Unity development branch. New work should start here.

`main`

Reserved for stable release states. Create it when the first stable baseline is ready.

`feature/*`

Reusable mechanics and shared systems.

Examples:

```text
feature/mobile-input
feature/interaction-system
feature/obby-checkpoints
```

`game/*`

Concrete game work.

Examples:

```text
game/obby-runner-first-map
game/pet-tycoon-shop-ui
```

`fix/*`

Focused fixes.

Examples:

```text
fix/webgl-loading-screen
fix/mobile-camera-drag
```

## Do Not Use Game Branches As Products

Do not keep one long-lived branch per game.

Wrong:

```text
ObbyRunner branch contains the whole ObbyRunner game
PetTycoon branch contains the whole PetTycoon game
```

Right:

```text
Assets/Games/ObbyRunner
Assets/Games/PetTycoon
```

Branches are for work in progress. Game folders are for products.

## Merge Direction

Normal flow:

```text
develop -> feature/* -> develop
develop -> game/*    -> develop
develop -> fix/*     -> develop
develop -> main      when stable
```

Do not merge `develop` into `master`. `master` is intentionally empty.

## Unity Merge Hygiene

- Keep changes small.
- Do not edit unrelated scenes or prefabs.
- Commit `.meta` files with their assets.
- Prefer one focused scene or prefab change per commit.
- Use prefabs and ScriptableObjects to reduce scene conflicts.
- Review `.unity`, `.prefab`, and `.asset` diffs before merging.
