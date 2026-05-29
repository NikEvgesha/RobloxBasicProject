# Imported Foundation Systems Audit

Date: 2026-05-29

Scope:

```text
Assets/Igrodelnya
Assets/Plugin/GoogleAPI
Assets/Resources/MirraSDK5
Packages/com.romanlee17.mirrasdk5
```

This audit classifies the imported systems before promoting anything into `Assets/GameKit`.

## Summary

`Assets/Igrodelnya` is useful as an imported foundation, but it is not ready to move wholesale into `GameKit`.

The strongest candidates for early `GameKit` migration are:

- `DynamicGridSpawner`;
- localization runtime data/manager;
- input service after adapter cleanup;
- quest definitions and conditions after enum/event cleanup;
- small UI helpers such as `Fade`, `ArrowLine`, and `ArrowPointer`.

Systems that should stay as foundation or game-specific until they are decoupled:

- bootstrap and global managers;
- MirraSDK/YG provider implementations;
- shop UI, roulette, playtime rewards, leaderboard UI;
- imported TouchControlsKit vendor code.

## Blocking Findings

### P1: Runtime scripts reference UnityEditor

`Assets/Igrodelnya/_Essential/Managers/LoadingManager.cs:4` imports `UnityEditor` from a runtime script.

`Assets/Igrodelnya/_Essential/Managers/SceneSelectorAttribute.cs:2` imports `UnityEditor` outside `#if UNITY_EDITOR`, while the drawer itself is editor-only at `SceneSelectorAttribute.cs:8`.

Risk: WebGL/player builds can fail because `UnityEditor` is unavailable outside the editor.

Status: fixed on 2026-05-29. `LoadingManager` no longer imports `UnityEditor`, and `SceneSelectorAttribute` keeps editor-only imports behind `#if UNITY_EDITOR`.

Recommended fix:

- remove the unused `UnityEditor` import from `LoadingManager`;
- split `SceneSelectorAttribute` into runtime attribute and editor drawer files, or wrap the `UnityEditor` using statement in `#if UNITY_EDITOR`.

### P1: Inventory is not initialized

`Assets/Igrodelnya/Inventory/Inventory.cs:6` declares `_items` but never initializes it.

`Inventory.Add` uses `_items.Add` at `Inventory.cs:20`, and `Inventory.Remove` uses `_items.Remove` at `Inventory.cs:25`.

Known callers:

- `Assets/Igrodelnya/Roulette/Roulette.cs:342`;
- `Assets/Igrodelnya/SpecialShop/SpecialShop.cs:141`.

Risk: any item reward from roulette or shop can throw `NullReferenceException`.

Status: fixed on 2026-05-29. `Inventory` now initializes its internal item list and ignores null add/remove requests.

Recommended fix:

- initialize `_items = new List<Item>()`;
- decide whether inventory should persist, serialize, or remain a runtime-only list;
- add a small EditMode test before promoting it.

## Major Findings

### P2: Global service locator prevents direct GameKit promotion

`Assets/Igrodelnya/G.cs:5` through `G.cs:21` exposes global mutable references for managers, input, currency, save, ads, purchases, and state.

Observed usage count: `G.` appears in 165 script lines under `Assets/Igrodelnya`.

Risk: systems cannot be safely reused per game or tested in isolation.

Recommended migration shape:

- keep `G` only inside imported foundation for now;
- do not add one shared `G` that contains every game;
- if a concrete game needs convenience access, keep a small `{GameName}G` facade under `Assets/Games/{GameName}` only;
- introduce GameKit interfaces per system, for example `ICurrencyWallet`, `IInputReader`, `ISaveStore`;
- write adapters from old managers to new interfaces before moving code into `GameKit`.

### P2: Save backend is platform-coupled

`SaveManager` initializes one serialized `SaveProvider` at `SaveManager.cs:36` and runs `saveProvider.SaveProgress()` every second at `SaveManager.cs:67`.

`DummySaveProvider` throws `NotImplementedException` for coin save/load at `DummySaveProvider.cs:61` and `DummySaveProvider.cs:65`.

`MirraSDKSaveProvider` parses saved doubles and dates directly at `MirraSDKSaveProvider.cs:74` and `MirraSDKSaveProvider.cs:133`.

Risk:

- local/debug save cannot cover currency flows;
- malformed save values can break load;
- backend-specific details leak into core gameplay.

Recommended migration shape:

- define a small generic save contract in `GameKit`;
- make MirraSDK/YG/PlayerPrefs adapters game or platform layer code;
- make save cadence configurable instead of fixed one-second polling.

### P2: YG save provider is currently stale

`Assets/Igrodelnya/_Essential/Managers/Provider/YG/YG2SaveProvider.cs` is behind `YG_SDK_ENABLED`, so it is not compiled by the current project defines.

If enabled, it declares overrides such as:

- `SaveScore` at `YG2SaveProvider.cs:42`;
- `LoadCurrency` at `YG2SaveProvider.cs:63`;
- `SaveColor` at `YG2SaveProvider.cs:103`;
- `GetLevelStatuses` at `YG2SaveProvider.cs:167`.

These methods are not present in the current `SaveProvider` contract.

Risk: enabling YG can introduce compile errors.

Recommended fix:

- either remove/replace the stale YG provider;
- or update `SaveProvider` and all providers to one consistent contract.

### P2: Bootstrap is scene- and SDK-coupled

`GameBootstrap` instantiates fixed manager prefabs and loads a `GameScene` enum value.

`LoadingManager` waits for MirraSDK providers at `LoadingManager.cs:40`, sends Mirra analytics at `LoadingManager.cs:79` and `LoadingManager.cs:86`, then drives lobby/game scene switching.

Risk: the startup path is not yet driven by `GameDefinition`, so multiple games cannot share a clean build/start pipeline.

Recommended migration shape:

- keep current bootstrap for `StarterSandbox`;
- create a future `GameDefinitionBootstrap` that reads selected scenes and platform profile from `GameDefinition`;
- keep SDK-specific initialization behind provider adapters.

### P2: Localization editor tooling has stale paths

`LocalizationAutoBinder` still points to old paths:

- `Assets/Igrodelnya2.0/Localization/LocalizationData.asset` at `LocalizationAutoBinder.cs:13`;
- `Assets/Igrodelnya2.0/Localization/Reports` at `LocalizationAutoBinder.cs:14`;
- `Assets/Scenes` at `LocalizationAutoBinder.cs:134`.

Risk: running the binder can miss current game scenes or write reports into the wrong path.

Recommended fix:

- update binder search roots to `Assets/Games` and current `Assets/Igrodelnya/Localization`;
- make roots configurable before using it on new games.

### P2: Quest system is data-driven but game-enum driven

Positive: quests use `QuestDefinition` ScriptableObjects, condition components, and a small static event bridge.

Current coupling:

- `QuestDefinition` depends on `QuestID`, `QuestKeyTypeTitle`, and `QuestKeyTypeDescription` at `QuestDefinition.cs:8`, `QuestDefinition.cs:11`, and `QuestDefinition.cs:12`;
- quest titles/descriptions access `LocalizationManager.Instance` directly at `QuestDefinition.cs:31` and `QuestDefinition.cs:43`.

Risk: new games need enum edits for quest IDs/text keys, which makes reusable quests harder.

Recommended migration shape:

- replace quest enums with string IDs or ScriptableObject references;
- inject or pass localization through a service instead of singleton access;
- keep condition components as early `GameKit` candidates after the event API is generalized.

### P2: Input is valuable but tied to old managers

Positive: `PlayerInput` already supports desktop keyboard/mouse and mobile touch paths.

Current coupling:

- reads `G.Control.UseTouchControl` at `PlayerInput.cs:130`, `PlayerInput.cs:214`, and `PlayerInput.cs:253`;
- reads TouchControlsKit axes at `PlayerInput.cs:216` and `PlayerInput.cs:255`;
- reads raw `Keyboard.current` and `Mouse.current` at `PlayerInput.cs:185`, `PlayerInput.cs:186`, `PlayerInput.cs:233`, and `PlayerInput.cs:259`;
- persists itself globally at `PlayerInput.cs:93`.

Recommended migration shape:

- keep `PlayerInput` as imported foundation;
- create a `GameKit` input contract backed by the shared `InputSystem_Actions.inputactions`;
- implement adapters for keyboard/mouse and TouchControlsKit UI.

## System Classification

| System | Path | Current status | GameKit decision |
|---|---|---:|---|
| Bootstrap/load managers | `Assets/Igrodelnya/_Essential/Boot`, `_Essential/Managers` | usable foundation, high coupling | Do not promote yet |
| Provider contracts | `_Essential/Managers/Provider` | useful shape, platform-specific | Promote contracts only after cleanup |
| MirraSDK providers | `_Essential/Managers/Provider/MirraSDK` | active platform layer | Keep outside `GameKit` |
| YG providers | `_Essential/Managers/Provider/YG` | disabled/stale | Review before enabling |
| Input | `Assets/Igrodelnya/Input` | useful, coupled to `G` and TouchControlsKit | Candidate after adapter split |
| TouchControlsKit | `Assets/Igrodelnya/Import/VictorsAssets/TouchControlsKit-Lite` | vendor package | Keep as vendor |
| Currency | `Assets/Igrodelnya/Currency` | usable, depends on save/audio/global events | Candidate after save decoupling |
| Inventory | `Assets/Igrodelnya/Inventory`, `Assets/Igrodelnya/Items` | not safe yet | Fix before any reuse |
| Localization runtime | `Assets/Igrodelnya/Localization` | good candidate | Promote runtime after editor path cleanup |
| Localization editor | `Assets/Igrodelnya/Localization/Editor` | useful, stale paths | Fix paths before use |
| DynamicGrid | `Assets/Igrodelnya/DynamicGrid` | isolated UI utility | Good early `GameKit` candidate |
| Quest system | `Assets/Igrodelnya/QuestSystem` | promising, enum-coupled | Candidate after ID/event cleanup |
| Roulette | `Assets/Igrodelnya/Roulette` | game/economy UI | Keep game-specific for now |
| Playtime rewards | `Assets/Igrodelnya/PlaytimeReward` | game/economy UI | Keep game-specific for now |
| SpecialShop/GemsShop | `Assets/Igrodelnya/SpecialShop`, `Assets/Igrodelnya/Currency` | platform/economy UI | Keep game-specific for now |
| Leaderboard | `Assets/Igrodelnya/Leaderboard` | Mirra/platform UI | Keep outside `GameKit` |
| Fade | `Assets/Igrodelnya/Fade` | small reusable UI helper | Candidate after null checks |
| ArrowLine/ArrowPointer | `Assets/Igrodelnya/ArrowLine`, `Assets/Igrodelnya/Tutorial` | small reusable guidance helpers | Candidate after dependency cleanup |
| Tutorial | `Assets/Igrodelnya/Tutorial` | game-flow coupled | Keep game-specific until event contract exists |

## Recommended Next Steps

1. Fix WebGL build blockers: remove runtime `UnityEditor` imports and split the scene selector drawer.
2. Fix `Inventory` initialization and add a minimal test for add/remove.
3. Update localization editor paths for the new monorepo layout.
4. Define first `GameKit` service contracts: input, save, currency.
5. Promote only one small isolated utility first, preferably `DynamicGridSpawner`, to prove the migration process.
6. After that, migrate input adapters because desktop/mobile control is central to the project goal.
