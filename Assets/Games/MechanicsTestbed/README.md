# MechanicsTestbed

`MechanicsTestbed` is a concrete game used to validate reusable mechanics before promotion to `Assets/GameKit`.

Keep experiments game-local until at least two games need the same behavior.

## Current Loop

- Third-person desktop movement with `WASD` / arrows.
- `Shift` sprint.
- `Space` jump.
- Hold `E` to complete the active interaction prompt.
- Hold right mouse button to smoothly rotate the camera behind the character.
- Mouse wheel zooms the third-person camera in and out.
- Mobile: left virtual joystick, sprint/jump buttons, right-side drag camera zone.
- Mobile controls hide automatically on desktop/WebGL desktop and show on mobile/handheld platforms, unless mobile simulation is enabled.
- Generic block-character Animator with idle, move, walk, and run states.
- Prototype modal HUD for soft currency, hard currency, settings, shop, wheel, and timed rewards.
- Currency and pickup VFX: a short particle burst around the player, then small tokens fly back with trails.
- Interaction test flow: look at a cube to pick it up, carry it above the player, place it in a green zone, and open the shop from a kiosk.
- ProBuilder video-inspired arena blockout with checker walls, studs, stalls, trees, pets, and reward cube.

## UI Style

Use a compact Roblox-like game HUD:

- shared art source: `Assets/SharedArt/UI/RobloxCasual`;
- white outlined currency badges over the game view;
- square blue, green, yellow, purple, and red icon buttons from the casual UI kit;
- modal windows with dimmed backdrop, close button, and outside-click close behavior;
- coin and diamond icons for soft and hard currency;
- editor-only cheat window opened with `~`;
- square-ish buttons and panels, no oversized marketing hero layout.

## Boundaries

The scripts under `Assets/Games/MechanicsTestbed/Scripts` are game-specific prototypes.

Do not reference this folder from `Assets/GameKit` or another game.

The current character animation assets live under `Assets/Games/MechanicsTestbed/Animations`.
Keep them game-local until a reusable Roblox-style block rig contract is defined in `Assets/GameKit`.

## ProBuilder Arena

`PB_VideoInspiredArena` in the main scene is a game-local environment art/blockout test.

It intentionally stays in this game until we decide what, if anything, should become reusable level-kit tooling.
