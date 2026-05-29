# Roblox Casual UI

## Location

Runtime Unity assets are imported under:

```text
Assets/SharedArt/UI/RobloxCasual/
```

This location is intentional because the pack is shared UI art and prefabs, not game-specific logic.

Imported folders:

- `Sprites` - sliced and single UI sprites;
- `Prefabs` - original pack prefabs for shop, chest cards, lucky wheel, and level-end screens;
- `Demos` - original pack demo canvases.

Original documentation and legal notes from the source package are stored under:

```text
Docs/ThirdParty/RobloxCasualUI/
```

## Usage Rules

- Games may reference these assets directly from `Assets/SharedArt/UI/RobloxCasual`.
- Do not place game-specific UI scripts in this shared art folder.
- If a reusable UI component needs code, put the generic code in `Assets/GameKit` and keep only art assets here.
- If a concrete game customizes these prefabs heavily, make the customized copy under `Assets/Games/{GameName}`.
- Keep the imported `.meta` files with the assets so prefab sprite references remain stable.

## Current Usage

`MechanicsTestbed` uses this pack for:

- HUD panel backgrounds;
- navigation and action button sprites;
- coin and diamond currency icons;
- mobile control sprites.
