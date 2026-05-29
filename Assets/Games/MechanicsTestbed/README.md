# MechanicsTestbed

`MechanicsTestbed` is a concrete game used to validate reusable mechanics before promotion to `Assets/GameKit`.

Keep experiments game-local until at least two games need the same behavior.

## Current Loop

- Third-person desktop movement with `WASD` / arrows.
- `Shift` sprint.
- `Space` jump.
- Hold right mouse button to rotate the camera behind the character.
- Prototype HUD for soft currency, hard currency, settings, shop, wheel, and timed rewards.

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
