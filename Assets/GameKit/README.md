# GameKit

Shared reusable systems for Roblox-style Unity WebGL games.

Code placed here must be reusable by multiple games and must not depend on `Assets/Games`.

Recommended folders:

```text
Runtime/    runtime systems used by games
Mechanics/  reusable gameplay mechanics
Editor/     Unity editor tools
Tests/      tests or test scenes
```

Promote code into `GameKit` only when it is generic enough to use in more than one game.

## Current Runtime Areas

- `Runtime/Interaction` - draft hold-to-interact system with target contracts, prompt driver, trigger source, and raycast source.
