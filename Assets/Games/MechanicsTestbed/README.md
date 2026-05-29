# MechanicsTestbed

`MechanicsTestbed` is a concrete game used to validate reusable mechanics before promotion to `Assets/GameKit`.

Keep experiments game-local until at least two games need the same behavior.

## Current Loop

- Third-person desktop movement with `WASD` / arrows.
- `Shift` sprint.
- `Space` jump.
- Hold right mouse button to smoothly rotate the camera behind the character.
- Mouse wheel zooms the third-person camera in and out.
- Mobile: left virtual joystick, sprint/jump buttons, right-side drag camera zone.
- Generic block-character Animator with idle, move, walk, and run states.
- Prototype HUD for soft currency, hard currency, settings, shop, wheel, and timed rewards.
- ProBuilder video-inspired arena blockout with checker walls, studs, stalls, trees, pets, and reward cube.

## UI Style

Use a compact Roblox-like game HUD:

- dark translucent panels: `#111827` / `#1F2937`;
- bright readable accents: soft `#22C55E`, hard `#38BDF8`, action `#F59E0B`;
- white text on dark surfaces;
- small top bar for persistent state;
- square-ish buttons and panels, no oversized marketing hero layout.

## Boundaries

The scripts under `Assets/Games/MechanicsTestbed/Scripts` are game-specific prototypes.

Do not reference this folder from `Assets/GameKit` or another game.

The current character animation assets live under `Assets/Games/MechanicsTestbed/Animations`.
Keep them game-local until a reusable Roblox-style block rig contract is defined in `Assets/GameKit`.

## ProBuilder Arena

`PB_VideoInspiredArena` in the main scene is a game-local environment art/blockout test.

It intentionally stays in this game until we decide what, if anything, should become reusable level-kit tooling.
