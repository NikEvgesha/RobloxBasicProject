# Kick Lucky Cube

## Game

Name: Kick Lucky Cube

Game id: kick-lucky-cube

Status: playable prototype / overview blockout

Contributor handoff and day-one workflow:

```text
Docs/Games/KickLuckyCubeHandoff.md
```

Production readiness, open defects, priorities, and acceptance criteria:

```text
Docs/Games/KickLuckyCubeProductionBacklog.md
```

The production backlog is the source of truth when an older smoke result in this history conflicts with the latest manual product review.

## Concept

Roblox-style WebGL game where the player trains strength, kicks a lucky cube from the current player position down a long rarity corridor, then controls the spawned animal while escaping a wave back to the kick start point.

The animal can be sold immediately or placed in a stable to generate soft currency over time.

## Scenes

```text
Assets/Games/KickLuckyCube/Scenes/KickLuckyCubeOverview.unity
```

## Current Prototype

Implemented in the overview scene:

- prototype player placeholder near the kick area;
- shared `GameKit` press/hold interaction prompt for `E`;
- lucky cube hidden at Play Mode start, visible in Edit Mode for placement, and shown in the player's `KLC_CarryAnchor` while the player stands in the kick zone;
- main lucky cube visual is an authored prefab at `Assets/Games/KickLuckyCube/Prefabs/KLC_LuckyCube.prefab` with black edge/corner geometry and block-built question marks on every face;
- lucky cube flight from the kick position selected by the first `E` press;
- flight trail FX on the lucky cube while it is in the air;
- third-person camera target switches to the flying cube during flight;
- distance calculation from current strength and the selected kick power meter value;
- kick power meter with a red/yellow/green strength background, a green fill bar, and moving marker;
- kick power selection hides the world interaction button after the first `E` press, can be confirmed by pressing `E` again or by clicking/tapping outside UI, and cancels if the player steps away after the first `E` press;
- smaller lucky cube scale for the active prototype scene;
- lower landing on the active `Floor_FlatGreenGrass` gameplay floor collider, followed by hiding the cube after impact;
- no persistent landing marker/platform is shown after cube impact;
- rarity zone detection by corridor depth;
- animal roulette phase after landing: imported catalog animal visuals cycle as black texture silhouettes from the landed rarity pool, hide names until reveal, slow down, then select one random animal;
- third-person camera target switches to the roulette preview until selection ends;
- animal spawn from the selected landed-rarity pool result;
- runner phase for the spawned animal, with camera-relative controls matching the prototype player, jump support, side-boundary clamping, and ground snapping to the real `Floor_FlatGreenGrass` play surface;
- third-person camera blends from the roulette preview to the spawned animal when control transfers after selection;
- chasing wave visual behind the animal, started only after the roulette selection and a short wave-rise camera intro that frames the selected animal instead of looking directly through the wave;
- wave speed scales up from the reached location and kick distance, with a named speed grade shown as a world label above the wave;
- red screen-edge danger vignette that intensifies as the wave approaches the animal;
- runtime wave visual is a prefab at `Assets/Games/KickLuckyCube/Prefabs/KLC_WavePreview_BehindRunner.prefab`; it is rebuilt as editable ProBuilder geometry with an S-shaped water wall, stacked blue gradient bands, foam ribbons, and a floor danger shadow in front of the wave so the player can read when the wave is about to catch them; the chase controller snaps the wave Y position to the floor under its start point, sinks it slightly to avoid a visible ground gap, and adds an invisible trigger blocker that the third-person camera uses to stay in front of the wave instead of clipping through it;
- return success state respawns the prototype player at the animal's finish point, stores the returned animal through the inventory, and selects it as the active held mob;
- runtime inventory UI with slot 1 reserved for the selected training tool and slots 2-5 reserved for up to four visible mobs;
- inventory window opened by `I` or the `Bag` button, with drag/drop movement between full inventory slots and bottom mob slots;
- inventory hotbar/storage mobs are saved in PlayerPrefs and restored in Play Mode;
- empty bottom mob slots are hidden in normal play and shown as drop targets only while the inventory window is open;
- the inventory window shows only occupied slots; dropping a bottom-bar mob onto empty inventory-window space creates the next occupied slot there;
- selecting a mob slot stops active tool training and shows a temporary mob preview in the player's `KLC_CarryAnchor`; starting kick power selection hides that hand preview so the lucky cube is the only held object during the kick;
- clicking or pressing the currently-selected mob slot clears selection and removes the hand preview;
- sell kiosk pad opens a larger inventory sell shop with owned mobs shown as square cards in a 3-column scroll grid;
- stable placement from inventory into player plot slots;
- per-slot stable upgrade boards that spend soft currency and boost that slot's income multiplier;
- fail/reset state when the wave catches the animal;
- sell kiosk pad for inventory mobs, with a carried-animal fallback kept only for older prototype wiring;
- stable placement slots for carried animals;
- stable passive soft income;
- green stable collect button;
- local wallet with soft/hard balances and PlayerPrefs-backed saves in Play Mode;
- PlayerPrefs-backed progression saves for strength, animal speed, speed upgrade level, owned tool tier, and selected tool tier;
- PlayerPrefs-backed stable slot saves for placed animals, pending soft, upgrade levels, and capped offline income;
- legacy `KLC_KickHud` status text with current strength, tool level, animal speed, predicted distance, landing result, run phase state, and economy state exists for debugging but is hidden by default while the prefab HUD pass is active;
- progression stations for training strength, buying animal speed, and buying the next strength tool;
- bottom inventory bar that replaces the old tool belt in Play Mode and lets slot 1 toggle active tool training after clearing the active mob only when training can start;
- active tool training shows randomized strength gain bursts around the player, flies them to the strength HUD, moves the held tool with the squat animation, and grants strength once per second;
- moving while tool training is active immediately cancels training and hides the held tool;
- `x2` training bonus prompt is shown as a circle-only `x2` button that stays pending until clicked, pressed with `X`, or until tool training stops;
- runtime Training Equipment shop can unlock the next strength tool tier and equip already-owned tools through the `Tools` button, `T`, or the weights-training kiosk stand pad;
- runtime Speed Upgrades shop opens from the speed kiosk stand pad or `Y` and buys affordable `+1`, `+5`, or `+10` animal speed level bundles;
- runtime Style shop opens with `E` from the style kiosk interaction anchor and buys/equips kick styles for hard currency;
- Classic Kick is free and selected by default; Roundhouse, Tornado Kick, Bicycle Kick, Scorpion Kick, and Lightning Spiral persist as owned/equipped styles and each applies a `1.10x` multiplier to effective kick strength before distance and power are calculated;
- runtime Future Feature spots support weather rarity boosts, selected-mob exchange charges with confirmation UI, hard-currency elite mob purchases, and a one-time rating gift mob;
- runtime Animal Catalog defines the shared mob list used by kick spawns, elite shop animals, rating gift animals, and album discovery; 15 regular kick mobs now generate 60 grade variants across Normal, Golden, Diamond, and Fire, while the former top 3 mobs are elite hard-shop exclusives;
- runtime Mob Album opens from a square UI button, closes through `X`, `Escape`, or backdrop click, and shows every catalog mob as a dark silhouette until it has been obtained at least once;
- runtime Leaderboard board displays a live prototype score list based on strength, soft currency, and owned mobs; its world-text visual is loaded from `Assets/Games/KickLuckyCube/Resources/KickLuckyCube/World/KLC_LeaderboardBoardVisual.prefab`;
- working Shop card purchases for speed upgrades, strength tools, and one-shot strength boosts;
- working playtime Rewards window with soft/hard claims;
- offline earnings popup opens on startup/resume after at least 30 minutes away, shows the stable income accumulated by all player-owned animals, and lets the player claim normally or claim `x2` through rewarded ads/VIP ad skip;
- 30-day VIP pass prototype grants `x5` soft income while active, immediately gives `300` hard currency, disables ad friction for reward multipliers, and falls back to a hard-currency purchase price on platforms without IAP support;
- working Lucky Wheel window with cooldown and soft/hard/strength rewards;
- Settings-driven audio mute backend and first UI localization binding for EN/RU labels;
- mobile control layer with virtual joystick and hold-interact button, hidden on desktop by default;
- third-person follow camera that starts behind the player, follows the flying cube, follows the roulette preview, then follows the spawned animal during the run phase;
- rebirth window and multiplier economy;
- fake online ambient plots with animated bots and pre-upgraded stable mobs.

## ProBuilder Visual Layer

The first visual pass lives in the scene under:

```text
KLC_ProBuilderVisuals
```

This group is a game-specific ProBuilder layer placed on top of the functional blockout. It is mostly visual:

- bridge planks/banks can be sampled by animal ground snapping without enabling the hidden ProBuilder visual layer;
- `Floor_FlatGreenGrass` is the authoritative shared walk surface for the player/animal path and has a MeshCollider for cube landing and runner floor sampling;
- old `GUIDE_` floor helpers are ignored by landing and runner ground checks so they do not create invisible walk planes;
- side walls have MeshColliders for physical readability, while animal movement is also clamped between their inner bounds;
- active ramps/hills/slopes should either have a collider, include `Ramp`, `Hill`, `Slope`, or `Bridge_` in the renderer name, or carry `KickLuckyCubeGroundSurface` so runners can climb them;
- interaction triggers and gameplay components stay on the original functional objects;
- the group should stay hidden during normal prototype play unless it is being edited directly.

Connected biome prefabs use a fixed `48.4m` footprint. The final `12.6m` is owned by the prefab's `Transition_RiverTraversal` instead of extending the corridor. `KickLuckyCubeRiverTraversalZone` exposes water tuning and randomly enables two of three authored `KickLuckyCubeRiverBridge` objects when Play Mode starts. Animals in uncovered water move at `0.9x` speed and visually sink by `0.22m`; active bridge bounds suppress both effects. The moved `KLC_CorridorGameplayData_DoNotDelete` root is authoritative for world-space placement: its location anchor, preview root, and biome instances stay at local `Y=0`.

Each connected biome also owns a `Walls` root with left/right ProBuilder walls. The walls are `22m` high, span the complete location footprint, align their inside faces to `X +/-39`, use biome-specific wall materials, and provide physical side colliders. World height still comes only from `KLC_CorridorGameplayData_DoNotDelete`.

All 30 corridor slots now reference individual first-pass biome prefabs under `Assets/Games/KickLuckyCube/Prefabs/World/Biomes`. Their visible static geometry is editable ProBuilder geometry. `Assets/Games/KickLuckyCube/Editor/KickLuckyCubeBiomeBatchBuilder.cs` is the batch bootstrap used for locations `06-30`; it is intended for initial generation and automated recovery, not routine rebuilding after manual prefab art edits.

Current ProBuilder coverage:

- launch platform trims, gate, kick arrows, and lucky cube dress-up;
- editable generated main-location polish under `KLC_ProBuilderVisuals/KLC_MainLocationProBuilderPolish_v1`: plaza frames, center runway, kick gate, floor arrow, studs, and side bollards;
- editable generated corridor polish under `KLC_ProBuilderVisuals/KLC_KickCorridorProBuilderPolish_v1`: per-location side tickers, 3-location overhead gates, colored top trims, and floor arrows;
- rarity gates and colored zone accents;
- river banks, bridge planks, rails, and posts;
- stable building shell, roof, fences, hay blocks, and coin markers;
- sell/style/speed/training/leaderboard kiosks with ProBuilder counters, frames, awnings, readable voxel icons, display blocks, and future-feature stand canopies;
- training rack/tool placeholders;
- active service prop layer for stable/sell/training readability;
- ProBuilder S-shaped wave wall, foam crests, and floor danger shadow;
- corridor fence details.
- `Tools/Kick Lucky Cube/Apply World Polish` rebuilds the generated ProBuilder polish groups, refreshes the 30-location decor, and applies the one-time lucky cube question-mark mirror fix through `KLC_LuckyCube_QuestionFlipMarker`.

Current verification:

```text
KLC_ProBuilderVisuals/KLC_MainLocationProBuilderPolish_v1 -> 31 ProBuilderMesh objects
KLC_ProBuilderVisuals/KLC_KickCorridorProBuilderPolish_v1 -> 145 ProBuilderMesh objects
KLC_VoxelPolish_v2 shop/future-stand roots -> 8 roots, 96 ProBuilderMesh objects
KLC_ProceduralDecor_v2 biome roots -> 30 roots, 760 ProBuilderMesh objects
KLC_LuckyCube_QuestionFlipMarker -> present in scene and KLC_LuckyCube.prefab
Visual bridge MeshColliders -> 32
Main corridor floor guide MeshCollider -> 1
Biome prefab slots/contracts/walls/rivers/materials -> 30/30
Biome Edit Mode previews at local Y=0 -> 30/30
Runtime rivers with exactly two active bridges -> 30/30
Runtime water modifier / active-bridge suppression -> 30/30
```

Next visual priorities:

1. Replace rough floor labels with diegetic 3D signs.
2. Tune the generated shop icons and stand silhouettes by hand now that they are editable ProBuilder meshes.
3. Add zone-specific animal silhouettes per rarity.
4. Improve stable/sell/training areas with final scale and interaction-readable silhouettes. First active service-prop pass done.
5. Reduce or hide old blockout primitives only after every gameplay reference is moved to dedicated anchors/triggers.

## UI Visual Layer

The first UI visual pass lives in the overview scene under:

```text
KLC_UIVisualPass
KLC_UIWindows
```

This pass uses the imported casual Roblox UI toolkit from `Assets/SharedArt/UI` and keeps the existing gameplay HUD objects alive. Persistent HUD backplates should live in the relevant editable UI prefab instead of a separate scene hierarchy layer.
Reusable runtime UI frame prefabs live under `Assets/Games/KickLuckyCube/Resources/KickLuckyCube/UI/Templates` so generated dynamic windows still start from editable prefab assets instead of bare code-created rectangles:

```text
KLC_UiBackdropFrame.prefab
KLC_UiWindowFrame.prefab
KLC_UiViewportFrame.prefab
KLC_UiCardFrame.prefab
KLC_UiButtonFrame.prefab
KLC_UiIconFrame.prefab
```

`KickLuckyCubeUiPrefabFactory` is the current bridge for runtime UI. It selects the closest frame prefab by object name, then controllers fill dynamic text, icons, sizes, layout, and events. New UI should either be authored as a concrete prefab/window in the scene or use these frame prefabs through the factory; do not add new local `new GameObject(name, typeof(RectTransform))` helpers inside feature controllers.

Concrete editable runtime UI element prefabs live under `Assets/Games/KickLuckyCube/Resources/KickLuckyCube/UI/Elements`. `KickLuckyCubeUiPrefabFactory` first looks for a prefab matching the requested UI object name, then falls back to a family prefab for dynamic lists such as `SpeedUpgradePlus_*`, `ToolTier_*`, `Style_*`, `EpicMob_*`, `KLC_AlbumCard_*`, `KLC_SellShopCard_*`, `KLC_InventorySlot_*`, `KLC_PowerBand_*`, and `KLC_WaveDangerVignette_*`, then finally falls back to the generic frame templates. Controllers should reuse existing `Text`, `Button`, `Image`, `CanvasGroup`, and custom effect components from those prefabs instead of adding duplicates, so designers can edit the prefab assets directly without entering Play Mode. Floating gain numbers, HUD/effect roots, and monetization popup shells are also prefabs now: `KLC_CurrencyGainFlyText`, `KLC_StrengthGainFlyText`, `KLC_CurrencyFlyTextLayer`, `KLC_StrengthFlyTextLayer`, `KLC_PlayerHomeIcon_Runtime`, `KLC_WaveDangerVignette`, `KLC_TrainingBonusPrompt`, `KLC_OfflineRewardBackdrop_Runtime`, `KLC_OfflineRewardWindow_Runtime`, `KLC_PrivilegePassCard`, and `KLC_PrivilegePassShopCard_Runtime`.

The kick power indicator is prefab-backed by `PowerBar_Back.prefab`. The scene keeps `PowerBar_Back` as a prefab instance, while `KickLuckyCubeKickController` only updates fill amount, marker position, and label text at runtime. Width, height, background sprite/color, colored power bands, marker art, and decorative separators should be changed in the prefab instead of hardcoded in the controller.

The first prefab-first window migration is active for `KLC_SpeedShopWindow_Runtime`, `KLC_StrengthToolShopWindow_Runtime`, `KLC_AnimalAlbumWindow_Runtime`, `KLC_InventoryWindow_Runtime`, `KLC_SellShopWindow_Runtime`, `KLC_StyleShopWindow_Runtime`, `KLC_ExchangeWindow_Runtime`, and `KLC_EpicMobShopWindow_Runtime`. Repeated item families are also editable as separate prefabs: `SpeedUpgradePlus`, `ToolTier`, `StyleCard`, `KLC_AlbumCard`, `KLC_SellShopCard`, `KLC_InventorySlot`, and `EpicMob`. These prefabs are now the right place for layout, spacing, image sizes, button shape, and static child hierarchy changes. Controllers may still write dynamic text, prices, icons, interactable state, and state colors. Do not keep sample rows inside dynamic-list window prefabs; keep the window shell and the reusable card prefab separate.

The lower HUD is no longer the legacy `KLC_ToolBelt`. The edit-mode scene and the runtime controllers should use the editable element prefabs `KLC_InventoryHotbar_Runtime`, `KLC_InventorySlot`, `KLC_InventoryToggleButton`, `KLC_AnimalAlbumOpenButton`, `KLC_StrengthToolShopOpenButton`, `KLC_SpeedShopOpenButton`, and `KLC_BottomLeftStatsVisual`. These objects belong under `KLC_PrototypeCanvas`; the runtime currency/fly-text canvas must not receive permanent UI buttons. The current layout pass follows the Roblox reference placement: large action buttons in a left-side grid, status/strength/economy in a bottom-left stack, the interaction prompt above the bottom-center hotbar, and only compact strips near the top edge.

`KLC_BottomLeftStatsVisual` is the first pass of the Roblox-reference bottom-left block. Each visible row is now a clickable semi-transparent tile with a black outline, because the row itself is the button. Rows keep their own readable main sprites: `KLC_Icon_KickingFoot`, `KLC_Icon_SpeedBoot`, `KLC_Icon_StrengthFlexArm`, `ItemIcon_Coin`, and `ItemIcon_Gem_Pentagon_Purple`. A small shared generated action indicator, `KLC_Icon_ActionTap`, sits on the right side of each tile to communicate that the whole tile is clickable. Prefer sprite assets from `Assets/SharedArt/UI` and game-local generated icons from `Assets/Games/KickLuckyCube/UI/Icons`; do not rebuild these icons from manual UI rectangles. The visible rows are rebirth count, current run speed, kick strength, soft currency, and hard currency. Runtime soft/hard gain effects target `KLC_BottomLeftSoftValue` and `KLC_BottomLeftHardValue`; runtime strength gain effects target `KLC_BottomLeftStrengthValue`. Row click routing is: speed opens Speed Upgrades, strength opens the kick mastery/settings window, hard opens the elite/epic mob shop, and soft/rebirth currently open the generic shop until the unified shop tab routing for clicked currency and kick purchases is implemented. The `(MAXIMUM)` label currently remains visual-only and will become the selected kick-power setting in a later functional pass.

TextMeshPro migration has started with the persistent lower HUD. `KickLuckyCubeUiTheme` now supports both legacy `UnityEngine.UI.Text` and `TMPro.TMP_Text`, and `KickLuckyCubeUiPrefabFactory.GetOrCreateTmpLabel` is the preferred path for newly-authored runtime labels. `KLC_BottomLeftStatsVisual` is the first migrated editable prefab and should use `TextMeshProUGUI` for its labels. Other windows may still use legacy `Text` until they are migrated in focused passes. Before a WebGL release, verify the TMP font asset/fallback contains every EN/RU localization character used by the game.

- `KLC_KickHud` still receives status text from gameplay scripts;
- `KLC_EconomyHud` can still receive legacy economy text from gameplay scripts, but it is hidden in the current bottom-left visual pass;
- `KLC_InteractionPrompt` still uses the shared `GameKitInteractionPromptView`.
- runtime-generated UI should use `KickLuckyCubeUiTheme` for windows, cards, buttons, outlines, text, and shared colors instead of hardcoding unrelated local palettes;
- HUD text, mobile controls, training bonus prompts, floating gain numbers, and world-space labels should also use `KickLuckyCubeUiTheme` helpers so buttons and labels stay visually consistent;
- `KickLuckyCubeRuntimeBootstrap` guarantees one runtime `AudioListener` on `Camera.main` or the first scene camera if the scene starts without one;

Current UI coverage:

- compact kick/status backplate on the left side above the action grid;
- top-center objective strip;
- bottom-left clickable stat/currency tiles with semi-transparent backplates and black outlines;
- left-side square icon grid for Album, Shop, Rebirth, Tools, Rewards, Wheel, and a compact Settings button;
- bottom inventory hotbar with a training-tool slot, compact visible mob slots, and the `Bag` button;
- restyled hold-`E` interaction prompt;
- runtime home icon above the player's plot, drawn in its own high-order screen-space overlay canvas with a black outline, so it remains visible through walls and clamps to the screen edge when the home is off-camera;
- runtime currency gain numbers burst around the player and fly to the wallet HUD after any soft/hard wallet gain;
- runtime future-feature status pills show current Weather and Exchange charge state;
- runtime Mob Album uses PlayerPrefs-backed discovered flags, refreshes when a newly obtained mob is marked discovered, and uses the same bottom square-button style as the other runtime feature buttons;
- runtime windows for Album, Inventory, Sell, Speed, Training Equipment, Cube Styles, and Future Feature panels now share the same bright Roblox-casual window/card/button/text treatment;
- runtime Offline Earnings popup and VIP pass cards are prefab-backed; code updates dynamic text, button state, claim amounts, and purchase state only;
- scene-backed Shop, Settings, Rewards, Rebirth, and Lucky Wheel controls also apply the shared theme on startup;
- square menu buttons use the `menu_*` sprite family in `Assets/SharedArt/UI/Sprites`: `menu_album`, `menu_shop`, `menu_inventory`, `menu_rewards`, `menu_rebirth`, `menu_wheel`, `menu_settings`, `menu_tools`, and `menu_speed`;
- lower tool belt, mobile buttons, kick power meter, economy HUD, x2 training prompt, home icon, currency/strength fly texts, and stable/wave world labels use the same shared button/text/outline treatment;
- visual Shop and Settings windows under `KLC_UIWindows`;
- reusable local window open/close controller for game-specific UI panels.

The Shop and Settings side buttons are wired to visual windows:

- the windows open from the left-side `Shop` and `Settings` buttons;
- each window closes through the red close button, backdrop click, or `Escape`;
- `KLC_EventSystem` uses `InputSystemUIInputModule`;
- there is no `StandaloneInputModule` in the overview scene;
- Shop cards buy speed upgrades, strength tools, and strength boosts with soft currency;
- the dedicated Speed Upgrades window uses whole-card purchase buttons; `+1` is always shown and disabled as `No money` when unaffordable, while `+5` and `+10` appear only when the player can afford the summed one-level costs;
- speed purchase cards show only the speed gain amount, not a final run-speed preview;
- Settings contents are UI-backed; audio/localization now have prefab-friendly runtime hooks, but the final authored mixer, music loops, and full text coverage are still pending.
- the audio backend applies Music/SFX toggles to explicitly configured sources and to scene `AudioSource` objects marked with `KickLuckyCubeAudioChannel`;
- when no routed audio source exists, the fallback only mutes `AudioListener.volume` if both Music and SFX are off, so disabling SFX does not accidentally mute future music-only playback;
- `KickLuckyCubeSfxController` currently generates short procedural prototype sounds at runtime for money gain, purchases, stable place/take, stable upgrades, and denied clicks; these are placeholder SFX until final authored clips are chosen;
- the localization backend applies EN/RU text to selected UI labels from the Settings language toggle, and prefab text can opt in with `KickLuckyCubeLocalizedText` on `Text`, `TMP_Text`, or world `TextMesh` objects.
- first prefab-localization pass is applied to the main user-facing UI prefabs: side buttons, Animal Album, Epic Mob Shop, Exchange, Inventory, Sell Shop, Speed Shop, Strength Tool Shop, Style Shop, Weather Machine, and shared sell/style cards.

The Rewards side button is wired to a working playtime window:

- the window opens from the left-side `Rewards` button;
- the window closes through the red close button, backdrop click, or `Escape`;
- rewards unlock by playtime and can grant hard and soft currency;
- reward elapsed time and claimed mask are saved in PlayerPrefs during Play Mode;
- current prototype rewards reset after the configured reset window.

The Wheel side button is wired to a working Lucky Wheel window:

- the window opens from the left-side `Wheel` button;
- the window closes through the red close button, backdrop click, or `Escape`;
- a spin grants one weighted reward: soft currency, hard currency, or strength;
- the next spin time is saved as a UTC Unix timestamp, so stopping Play Mode, restarting the game, or reloading WebGL cannot extend an expired cooldown;
- the current prototype cooldown is short for testing and should be retuned for production pacing.

Settings prototype behavior:

- Music and SFX rows toggle between `ON` and `OFF`;
- Language cycles between `EN` and `RU`;
- settings state is saved to PlayerPrefs in Play Mode;
- the current scene has no final audio mixer or localization table yet, so these toggles currently define the UI contract for the later audio/localization pass.

Latest UI wiring probe:

```text
Shop window: closed -> open -> closed
Settings window: closed -> open -> closed
Settings controls: 3 clickable rows
Settings toggle check: Music OFF, SFX OFF, Language RU
Settings reset check: Music ON, SFX ON, Language EN
EventSystem: InputSystemUIInputModule, no StandaloneInputModule
Active clickable UI overlap audit: 0 overlaps on 1920x1080 Play Mode probe
Album backdrop probe: open -> backdrop click -> closed
```

Latest UI prefab smoke:

```text
Date: 2026-06-30
Result: PASS
Checked full window prefabs and repeated card prefabs under Resources/KickLuckyCube/UI/Elements.
Confirmed InventoryWindow has authored children, SellShopWindow has no sample dynamic sell rows, and controllers reuse existing layout/button components.
```

The Rebirth side button is wired to a working window:

- the window opens from the left-side `Rebirth` button;
- the window closes through the red close button or backdrop click;
- first rebirth requires 1500 strength;
- each next rebirth requirement is 30x higher;
- rebirth resets strength only while keeping speed upgrades and owned strength tools;
- soft gain multiplier becomes `x2`, then `x3`, then `x4`, and so on.

## Fake Online Ambient Layer

The older standalone fake online pass lives in the overview scene under:

```text
KLC_FakeOnline
```

The current runtime player/bot base allocation is handled by `KickLuckyCubePlotAllocator` under `KLC_PlotAllocationSystem`. This allocator picks one `MOVE_PlotSlot_*` for the player each Play Mode run, spawns the player plot as `KLC_PlayerPlot_Instance`, fills the remaining slots with bot plots such as `KLC_BotPlot_Astrozto`, and hides the edit-mode `KLC_PlotSlot_StaticInstances` preview root while playing.

Allocated bot plots are visual/ambient only:

- fake bots do not spend player currency;
- fake mobs do not generate player income;
- bot stable visuals are spawned directly from the animal catalog and do not receive `KickLuckyCubeStableSlot`, interaction triggers, or PlayerPrefs save keys;
- the old `KLC_FakeOnline` layer can stay disabled, be deleted, or be regenerated without changing the real core loop.

Current fake online coverage:

- 1 randomly assigned player plot from the available `MOVE_PlotSlot_*` locations;
- up to 4 bot plots, capped by the remaining available plot slots;
- 3 visual-only bot stable mobs per allocated bot plot, with catalog visuals, grade effects, and short labels;
- static plot copies remain edit-mode references only.

Latest Play Mode check:

```text
playerSlot=MOVE_PlotSlot_02
spawned runtime plots=4
bot plots=3
bot stable visual roots=9
player persistent stable slots=10
first player stable id=Player.Template_StableSlot_01
static preview root active=false
```

Default tuning:

```text
balance source = Assets/Games/KickLuckyCube/Resources/KickLuckyCube/KickLuckyCubeBalanceConfig.asset
distance = AnimationCurve(strength -> meters), front-loaded then heavily diminishing
minimum distance = 10
maximum distance = 1463
starter strength = 120
speed upgrade = +0.1 runner speed per level
speed level cost = ceil(35 * 1.06 ^ (level - 1))
rebirth requirement = 1500 * 30 ^ rebirthCount
regular mob income = 30 + progressionIndex * 6
```

Expected test values:

```text
0 strength -> Common
120 strength -> early multi-zone landing; current main flow picks a random pool animal through the roulette
1400 strength -> around location 9
220000 strength -> around location 18
2600000000 strength -> around location 30 before power-meter bonus
Cat stable income -> 30 soft/s
Cat sell value -> 540 soft
```

Latest balance probe:

```text
0 strength -> 13.0m
120 strength -> 141.8m
1400 strength -> 394.2m
220000 strength -> 829.8m
2600000000 strength -> 1410.6m before power-meter bonus
speed level 1 cost -> 35 soft
speed level 10 cost -> 60 soft
buy +10 speed levels from 0 -> 466 soft
tool 1 -> +2/s, free
tool 15 -> +231857/s, 555000000 soft
regular kick mob income -> 30-384 soft/s
rebirth requirements -> 1500, then 45000
```

To test manually, open `KickLuckyCubeOverview`, enter Play Mode, and click bottom slot 1 or press `1` to start/stop tool training. The player should hold the tool, squat, gain strength each second, show strength gain text bursting from different points around the player and flying to the strength HUD, and show at most one pending `x2` circle every 5 seconds. Walking while training should immediately cancel training. Then stand near the kick interaction area; the lucky cube should appear in the player's hands.
Press `E` once to start the power meter. The interaction prompt disappears during selection; press `E` again or click/tap anywhere outside UI to kick with the current meter value. If the player moves after starting the meter, the meter is cancelled.
The camera follows the cube during flight; the cube leaves a trail, lands on the lower corridor floor, then disappears.
After landing, the animal roulette cycles through shadow silhouettes from the landed rarity pool and slows down on one selected animal.
Only after that selection does the wave rise intro play: the camera frames the selected animal while the wave appears behind it, the speed label is shown, then the camera blends back behind the selected animal and the chase starts. Run back toward the kick start point before the wave reaches it; the red vignette should intensify as the wave gets close.
After a successful return, the mob is added through the inventory system and immediately selected as the active held mob. If one of the four bottom mob slots is empty it lands there; otherwise it goes to the inventory window but still becomes active.
Open the temporary inventory with `I` or the `Bag` button and drag mobs between the window and the bottom bar.
During normal gameplay, unused bottom mob slots are hidden. While the inventory is open, all four bottom mob slots are visible as drop targets.
The inventory window itself does not display empty slots; dropping a mob onto empty window space creates a new visible item slot.
Selecting a mob shows it in the player's hands and stops tool training if it was active. Selecting that same mob again clears the selection and removes the hand preview.
Walk to the animal sell kiosk pad, press `E` on `Open sell shop`, then sell owned mobs from square shop cards. The shop closes automatically when the player leaves the sell zone.
To place a mob into the player plot, select it in the bottom bar or inventory, stand on an empty stable `CollectSpot`, and press `E` on `Поставить`; the mob leaves the inventory and appears on that slot's `MobAnchor`.
To take a placed mob back, stand on an occupied stable `CollectSpot` and press `E` on `Забрать`; the mob returns to inventory, preferring an empty bottom hotbar slot.
Click that slot's `UpgradeBoard` with the mouse to buy the next slot booster level; the boosted multiplier affects only the animal placed on that exact slot. The board displays only the current level and a compact next price, with a black outline for readability.
Walk to the speed kiosk stand pad, press `E` on `Open speed shop`, then buy the next speed level from the runtime card grid. The shop closes automatically when the player leaves the stand pad.
Placed animals generate pending soft from saved real UTC time, so income continues accumulating while the player is offline and is restored on the next launch. Stand on that slot's `CollectSpot` to claim the slot income automatically, or use the green collect-all board for the combined income. The pending soft amount is shown above the green `CollectSpot`, not in the mob name label.

Prototype controls:

```text
WASD / arrows: move the prototype player, and later the animal runner relative to the current camera direction
Space: jump while controlling the prototype player
Shift: sprint while controlling the prototype player
Rotate the camera, then use WASD / arrows to steer the animal in the same relative direction as the player
E: press once to start kick power selection, press again to kick while the power meter is active; press near sell/speed kiosks to open their shops; press near stable collect spots to place/take mobs; hold for collect-all and progression stations
Mouse left click: upgrade an occupied stable slot from its `UpgradeBoard`
Hold E near style kiosk: open kick-style shop
E near future spots: start weather, exchange selected mob, open epic mob shop, or claim rating gift
I: open / close the temporary inventory window
1: start / stop training with the selected fixed tool slot
2-5: select bottom-bar mob slots
X: claim the pending x2 training bonus circle
T: open / close the runtime Training Equipment shop
Y: open / close the runtime Speed Upgrades shop
Right mouse drag: rotate third-person camera
Mouse wheel: zoom third-person camera
```

Camera:

- `KLC_OverviewCamera` uses `KickLuckyCubeThirdPersonCamera`;
- on scene start it snaps behind `KLC_PrototypePlayer`;
- `KLC_PrototypePlayer` uses `KickLuckyCubePlayerController`;
- in Play Mode the player visual is replaced with the imported Steal Brainrot `SadovnicOBJ` prefab from `Assets/Games/KickLuckyCube/Resources/KickLuckyCube/StealBrainrot/Models/Player`;
- the old blocky `KLC_PlayerVisual` / `AvatarRoot` children are kept only as editor/fallback visuals when the imported prefab is not available;
- during animal run phase it follows the current spawned animal;
- after run/fail/sell/stable flow it falls back to the prototype player target.
- target movement is smoothed once at the focus point; the camera transform is no longer interpolated a second time, which avoids delayed spring-like movement behind the player or animal;
- right-mouse orbit uses `orbitRotationSharpness`, while automatic rotation uses `rotationSharpness`;
- collision retracts quickly with `collisionRetractSharpness` and restores the requested zoom distance gradually with `collisionRestoreSharpness`, avoiding a distance jump after leaving a wall or the wave blocker.

Location layout blockout:

- clean placement workspace is `KLC_LayoutBlockout_Workspace`;
- move only objects with names starting with `MOVE_`;
- blockout mesh objects are editable `ProBuilderMesh` objects with scale baked into geometry;
- `MOVE_PlayerSpawn_BlueCube` is the player spawn marker; `KickLuckyCubePlayerSpawnController` snaps `KLC_PrototypePlayer` to it on scene start;
- `MOVE_KickLine_YellowBar` defines the current kick line; `KLC_KickLineInteractionTrigger` is aligned just before it for hold-to-kick;
- `KLC_PlayerKickBoundary` is an invisible player-only limiter after the kick line, so spawned animals can still return through the area;
- plot allocation is grouped under `KLC_PlotAllocationSystem`;
- reusable plot source is `KLC_PlotTemplate_EditSource`;
- `Template_PlotGround` previews the plot footprint and should match the scaled `MOVE_PlotSlot_*` footprint;
- current plot template follows the reference base layout: tan room floor, center aisle, two five-slot stable rows, front boards, and green interaction pads;
- obsolete plot decorations are intentionally removed from the template: owner tint strip, player spawn pad, sell pad, and boundary walls;
- reusable root modules are grouped as `Template_FrontBoards`, `Template_CenterAisleGroup`, `Template_BackWallGroup`, and `Template_FloorExpansion`;
- `Template_BaseUpgradeBoard` is the placeholder for upward base expansion / extra floors;
- `Template_CollectAllAdRewardBoard` is the placeholder for the ad-gated collect-all reward board;
- each of the ten `Template_StableSlot_*` objects is a grouped stable module with a paired child `MobAnchor`, `CollectSpot` placeholder, and `UpgradeBoard` booster placeholder;
- the current player plot is auto-bound in Play Mode by `KickLuckyCubeStablePlotRuntimeBinder`: its ten direct `Template_StableSlot_*` children receive stable logic, a trigger on `CollectSpot`, an `E` interaction for place/take, automatic income collection while standing on the collect spot, and a mouse-click `UpgradeBoard` for per-slot boosters;
- the player's current plot also gets a runtime home icon through `KickLuckyCubeHomeIconMarker`; it follows the plot in a dedicated UI overlay canvas, stays visible through walls and other 3D blockers, and clamps to the screen edge when the home is behind the camera;
- empty stable collect spots show `Поставить` only while the player is carrying/selecting a mob; occupied stable collect spots show `Забрать` and return the mob to inventory, preferring free bottom hotbar slots;
- standing on an occupied stable `CollectSpot` automatically claims pending soft currency without pressing `E`;
- each occupied `CollectSpot` shows the pending soft amount as a short white label with black outline fixed inside the green pad; it does not billboard toward the camera and shows `0` while the slot has a mob but no pending income;
- stable `UpgradeBoard` objects are mouse-click targets, not `E` prompts, and their runtime text is limited to level plus compact price; the runtime label size is derived from the board mesh height so it stays proportional to the stable board;
- spawned and stabled mob capsules use a black inverse-hull outline for clearer Roblox-style readability;
- stable mob anchor placeholder visuals are hidden at runtime; only actual placed mobs should be visible on slots;
- occupied stable slots normalize placed mobs to a target world height from renderer bounds, independent of the non-uniform `MobAnchor` scale, face placed mobs toward the center aisle of their own plot by default, and show a white TextMesh with black outline above the mob; the label contains only mob name, stable level, and income per second, faces the camera from an unscaled runtime label root, and hides at distance to reduce visual clutter;
- `Template_FloorExpansion` contains visual-only posts, ladder, and outline beams for future upper-floor expansion;
- `MOVE_PlotSlot_01..04` define placed plot locations;
- `KLC_PlotSlot_StaticInstances` contains edit-mode plot copies placed from the reusable template on every current `MOVE_PlotSlot`; it is hidden automatically in Play Mode after runtime plots are allocated;
- `KLC_PlotTemplate_EditSource` is kept inactive as the reusable edit source and should be enabled only when editing the template shape;
- `KLC_RuntimePlotInstances` receives the runtime-allocated `KLC_PlayerPlot_Instance` and `KLC_BotPlot_*` copies from `KLC_PlotTemplate_EditSource`;
- `KickLuckyCubePlotAllocator` runs in Play Mode through `allocateOnRuntimeLoad`, so the scene does not depend on the old manual `allocateOnStart` checkbox;
- the player plot keeps persistent stable ids in the `Player.Template_StableSlot_*` format regardless of which physical `MOVE_PlotSlot_*` was assigned this run;
- bot plots are non-persistent visual ambience, spawn catalog-based stable mob visuals, and instantiate one `KLC_FakeOnlineBotActor.prefab` using activity anchors copied from the allocated plot; none of these objects use player economy or save keys;
- `KLC_HubKiosks_Blockout/KLC_ImmediateKiosks_LeftToRight` holds the current left-to-right hub kiosks: animal sell, style shop, speed upgrade, weights training, and leaderboard;
- sell, speed, and weights kiosks have visible stand pads; style shop uses a hold-interaction anchor and opens the runtime kick-style shop;
- `KLC_HubKiosks_Blockout/KLC_FutureFeatureSpots` holds runtime-bound prototype spots for weather machine, animal exchange, epic mob shop, and rating gift stand;
- `KLC_LeaderboardBoardVisual.prefab` is the editable world-text visual for the hub leaderboard; `KickLuckyCubeLeaderboardController` only updates the `KLC_Leaderboard_Header` and `KLC_Leaderboard_Line_01..05` text values at runtime, so spacing, text positions, and outline styling should be changed in the prefab first;
- plot template, placed plot copies, and hub kiosk blockout meshes are converted to `ProBuilderMesh`; `TextMesh` labels remain regular text objects;
- `00_BaseEnvelope_DoNotMoveAsAGroup` holds the flat green grass floor and tan boundary walls;
- `02_ZoneAndRiverGuides` holds non-final zone, corridor, and river guides;
- `02_ZoneAndRiverGuides/KLC_Generated30LocationLogic` contains generated logic-only guides for locations 6-30;
- `KLC_ExtendedCorridorFloor_30Zones`, `KLC_ExtendedLeftWall_30Zones`, and `KLC_ExtendedRightWall_30Zones` extend the playable kick/run corridor to the 30-location endpoint while final biome art is still pending;
- `02_ZoneAndRiverGuides/KLC_BiomeBlockout_30Locations` contains the current rough visual biome pass for all 30 locations;
- `02_ZoneAndRiverGuides/KLC_ZoneGateSignage_30Locations` contains the disabled first draft of zone gate/sign groups for all 30 locations; this pass is intentionally inactive because the repeated beams and labels create too much visual noise down the corridor. Reuse only selectively for a few milestone gates or rebuild as sparse diegetic signs.
- `03_ServicePropVisuals/KLC_ServicePropVisuals_StableSellTraining` contains active editable ProBuilder readability props for animal sell, weights training, and stable plots; children are named `MOVE_ServiceProp_*`, have no colliders, and should stay visual-only so kiosk, stable, and collect triggers remain owned by the existing gameplay objects;
- biome blockout children are named `MOVE_Biome_XX_*` and are safe to reposition, reshape, replace, or delete during location art iteration;
- biome blockout meshes are visual `ProBuilderMesh` objects with colliders removed, so gameplay collision still comes from the existing corridor floor/walls and logic zones;
- the biome blockout root is slightly lowered on Y so colored floor plates sit closer to the existing corridor floor;
- central black path-edge guide walls were removed from the biome blockout; use the outer corridor walls and river gaps for current spatial read;
- river visuals are grouped as `MOVE_RiverGap_XX_To_YY` between physical locations;
- biome materials live under `Assets/Games/KickLuckyCube/Art/Materials/Biomes` and can be tuned without changing gameplay logic;
- older noisy prototype visual groups are disabled while this layout pass is being placed.

Mobile controls:

```text
Virtual joystick: move animal runner
Jump button: queue a jump for either the prototype player or the controlled animal runner
Green E button: starts press-mode targets on tap and holds hold-mode targets through GameKitInteractionDriver.SetExternalHold
Visibility: hidden on desktop/editor by default, shown on mobile/handheld platforms or when simulation is enabled
```

## Core Loop

1. Player stands where they want the kick to start.
2. Player trains strength with a selected tool.
3. Player buys speed upgrades for the future animal runner with soft currency.
4. Player kicks the lucky cube.
5. Cube flies forward based on strength.
6. Cube passes through corridor zones and changes rarity as it reaches deeper zones.
7. Cube lands in one zone and spawns an animal from that zone's rarity pool.
8. Camera shows a wave appearing behind the animal.
9. Player controls the animal and runs back to the kick start point.
10. If the wave catches the animal, the run fails.
11. If the animal returns in time, the original player respawns at that point and the wave disappears.
12. The returned animal is stored through inventory, becomes the active held mob, then can be sold through the sell kiosk or later placed in a stable.
13. Stable animals generate soft currency every second.
14. Player stands on the green collect button to claim stable income.
15. Player spends soft currency on animal speed and strength tools.
16. Rebirth resets strength for a permanent money multiplier.

## Rarity Corridor

Current corridor direction:

- the corridor is expanded to 30 logical locations;
- every location is doubled along corridor depth only: guide spacing is `48.4m`, while corridor width is unchanged;
- location 1 starts at `7m`; location 30 ends at `1450.6m`;
- max kick distance is currently `1463m`, leaving a short safety margin after location 30;
- zones 1-5 keep the original rarity labels for early readability, while zones 6-30 are Legendary-rarity logical guides whose `zoneIndex` drives the deeper animal pools.

The scene contains deterministic low-poly decoration on both sides of the runner lane for all 30 locations. The editor command `Tools/Kick Lucky Cube/Apply World Polish` rebuilds that decoration, shop trim and wave foam idempotently. `KickLuckyCubeCorridorLayout` is the single runtime source for corridor count, start, spacing, playable length and maximum distance.

Each zone is separated by a river gap. The deeper the cube lands, the better the animal pool.

Rough biome order:

The canonical object, prop, VFX, and animal brief for every biome is maintained in `Docs/Games/KickLuckyCubeLocations.md`. Use that document when building the 30 connected biome prefabs; the table below is only the compact order reference.

| Location | Biome |
| ---: | --- |
| 1 | Start Meadow |
| 2 | Pine Forest |
| 3 | Flower Grove |
| 4 | Desert |
| 5 | Canyon |
| 6 | Oasis |
| 7 | Swamp |
| 8 | Dark Marsh |
| 9 | Jungle |
| 10 | Bamboo Valley |
| 11 | Snowfield |
| 12 | Ice Cliffs |
| 13 | Crystal Cave |
| 14 | Mushroom Forest |
| 15 | Autumn Grove |
| 16 | Savanna |
| 17 | Badlands |
| 18 | Volcano |
| 19 | Lava River |
| 20 | Ash Wastes |
| 21 | Beach |
| 22 | Pirate Cove |
| 23 | Candy Land |
| 24 | Toy City |
| 25 | Neon City |
| 26 | Sky Isles |
| 27 | Magic Ruins |
| 28 | Moon Crater |
| 29 | Cosmic Rift |
| 30 | Divine Garden |

Kick mob grades:

- Normal: base body color;
- Golden: gold tint, grade glow/VFX;
- Diamond: blue tint, grade glow/VFX;
- Fire: red tint, grade glow/VFX.

Animal income grows linearly across the full kick progression. The formula is:

```text
income = 30 + progressionIndex * 6
```

`progressionIndex` starts at `0` for Normal Cat and continues through all 60 regular kick variants. Color grades are visual/tier stages and do not multiply income directly.

Normal-stage income ladder:

| Mob | Income |
| --- | ---: |
| Cat | 30/s |
| Dog | 36/s |
| Chicken | 42/s |
| Bee | 48/s |
| Pudding | 54/s |
| Finn | 60/s |
| Bessie | 66/s |
| Paca | 72/s |
| Champ | 78/s |
| Svinina | 84/s |
| Chill | 90/s |
| Wolfle | 96/s |
| Bailey | 102/s |
| Stripey | 108/s |
| Gigi | 114/s |

Full regular grade-stage ranges are Normal `30-114/s`, Golden `120-204/s`, Diamond `210-294/s`, and Fire `300-384/s`.

Kick location pools are generated from all regular kick mobs in grade-stage order: all Normal variants, then Golden, then Diamond, then Fire. Every physical location gets 3 variants:

- location 1 gets the first 3 variants from the current progression order;
- location 2 gets the top variant from location 1 plus 2 new stronger variants;
- location 3 gets the top variant from location 2 plus 2 new stronger variants;
- later locations continue the same `previous top + 2 new` rule until the catalog runs out, then the last 3 strongest variants are reused.

With the current 15 base kick mobs and 4 grades, this creates 60 regular kick variants and 30 progression location pools. The strongest regular variant is currently Fire Gigi at `384/s`.

The former top 3 regular mobs are removed from lucky-cube progression and sold only through the Elite Mob Shop for hard currency:

| Elite mob | Cost | Income |
| --- | ---: | ---: |
| Golden Elite Prism Thumper | 350 hard | 900/s |
| Diamond Elite Aurora Penguin | 750 hard | 1400/s |
| Fire Elite Inferno Jet | 1500 hard | 2200/s |

Weather boosts still raise the landed rarity, but animal selection uses the stronger of the physical `zoneIndex` and boosted rarity tier, so a weather-boosted landing can pull from a deeper location pool.

Wave speed grades:

- the wave location index is resolved from the same `7m + 48.4m * locationIndex` corridor grid;
- every 3 locations advance the displayed speed grade;
- the label above the wave shows `WAVE`, speed grade, location index, and numeric speed;
- the current grade sequence is Slow, Steady, Fast, Very Fast, Danger, Wild, Extreme, Insane, Mythic, Impossible;
- the current wave formula starts from runner speed, adds `0.01 * kickDistance`, adds `1.15` speed for each 3-location tier, and caps at `34m/s`.

## Progression

Strength:

- progression balance is centralized in `Assets/Games/KickLuckyCube/Resources/KickLuckyCube/KickLuckyCubeBalanceConfig.asset`;
- kick distance uses a non-linear `strength -> distance` curve: early strength quickly reaches the first locations, while late-game distance gains require much more strength;
- default target gates are tuned around reaching location 30 after about 90 days at roughly 2 hours of daily play, with early rewards arriving much faster than late rewards;
- current prototype trains from bottom slot 1: the selected named tool appears in-hand, the player squats, the tool moves with the squat, and tier-based strength is added every second;
- the green `Train Strength` station remains as an older prototype fallback;
- tools currently have 15 configured tiers; early tools are cheap and visible quickly, while later tools become long-term goals;
- each tool level increases strength gained per second/hold according to the balance config;
- walking away from the training start position cancels tool training immediately;
- every 5 seconds of active tool training can show one persistent `x2` UI prompt; if one is already visible, no new circle is spawned;
- claiming `x2` via click/tap or `X` gives the pending bonus and triggers the same strength gain fly-text effect;
- the runtime Training Equipment shop buys only the next locked tool tier, then lets owned tiers be re-equipped;
- tools can have different training animations;
- the weights-training kiosk stand pad opens the same runtime Training Equipment shop with `E`;
- future pass should replace placeholder tool cards with final visuals.

Speed:

- current prototype opens a runtime Speed Upgrades shop from `KLC_Kiosk_03_SpeedUpgrade_StandPad` or `Y`;
- the old blue `Buy Speed` station now opens the same shop when it is active instead of buying directly;
- speed levels are uncapped;
- the shop can show up to three purchase buttons: `+1`, `+5`, and `+10` levels;
- the `+1` button is always shown as the baseline upgrade option, but is disabled when the wallet cannot afford it;
- `+5` and `+10` purchase buttons are hidden when the wallet cannot afford that full bundle;
- `+5` and `+10` prices are calculated as the sum of buying each next level one by one from the current level;
- each next single level costs `ceil(35 * 1.06 ^ (level - 1))` soft by default;
- each bought level currently adds `+0.1` runner speed, so the same long-term speed progression is split into many smaller visible upgrades;
- the speed shop closes automatically when the player leaves the speed kiosk stand pad.

Stable income:

- stable animals generate soft currency per second;
- every stable slot starts at booster level 1; each upgrade increases only that slot's income multiplier by 20%;
- slot upgrade levels are uncapped; upgrade costs start at 150 soft and scale by 1.65x per level;
- each occupied stable slot is claimed automatically by standing on its own green `CollectSpot`;
- each occupied stable slot has a larger placed mob, a black outline, and a distance-gated billboard label above the mob;
- stable upgrade boards show only the current level and next price, and are bought by mouse click;
- the green collect-all board resolves runtime stable slots dynamically and claims the combined pending income;
- placed stable animals, pending income, upgrade level, and last save time are stored in PlayerPrefs in Play Mode with player-owned slot ids like `Player.Template_StableSlot_01`, independent of which physical plot location is assigned this run;
- fake/bot/template stable slots do not write player stable save keys and are ignored by player collect/weather checks;
- offline stable income is calculated from the saved UTC timestamp by real elapsed seconds; the default prototype setting has no offline cap, so next-day returns still accumulate income.
- when the player was away for 30 minutes or more, `KickLuckyCubeOfflineRewardController` shows the offline earnings popup and claims the combined pending income from all player persistent stable slots; the normal claim uses the wallet multiplier, while the ad/VIP path doubles the base offline claim before wallet multipliers are applied;
- stable saves explicitly flush `PlayerPrefs.Save()` after place/take/collect/upgrade/clear so WebGL/mobile builds do not lose the last action;
- stable slot teardown preserves the saved inventory payload even if Unity destroys the runtime mob GameObject before `KickLuckyCubeStableSlot.OnDestroy`, so stopping Play Mode no longer clears placed mobs;
- prototype saves, including inventory, can be cleared in the Unity Editor through `Tools/Kick Lucky Cube/Clear Prototype Save`.

Inventory / tool bar:

- bottom slot 1 shows the active named training tool, its owned tier/status, and toggles tool training;
- bottom slots 2-5 hold up to four returned mobs;
- unused bottom mob slots are hidden unless the inventory window is open;
- when the inventory window is closed, visible mob slots are compacted left-to-right and number shortcuts select by visible order: `1` always toggles training, then `2-5` select the first through fourth visible mob;
- when the inventory window is open, bottom mob slots keep their physical slot positions so drag/drop placement remains explicit;
- overflow mobs go into the temporary inventory window but can still become the active held mob;
- mobs can be dragged between inventory-window slots and bottom-bar mob slots, or dropped onto empty window space to create the next inventory slot;
- selected mobs are shown in `KLC_CarryAnchor`; selecting the same mob again deselects it, and selecting any mob stops active tool training;
- the sell kiosk opens a runtime sell window with 3-column square mob cards and removes sold mobs from the bottom bar/inventory while adding their sell value to the wallet;
- selected inventory mobs can be placed into an empty stable `CollectSpot`; if placement fails, the mob is returned to the hotbar/inventory;
- inventory persistence is active, and stable slot persistence stores the full inventory animal payload.

Character customization / skin shop:

- `KickLuckyCubeCharacterSkinShopController` bootstraps a runtime `SKINS` button and a wardrobe window that also toggles with `K`;
- the wardrobe has nine independent tabs: body, skin tone, torso, legs, boots, gloves, hair, headwear, and mask;
- male and female bodies, three natural skin tones, and both gender-default clothing/hair sets are free;
- green/blue skin, the unisex clothing variants, all three hats, all three masks, and the extra curly hair use hard currency;
- owned parts can be mixed across slots; changing body type updates only free gender-default clothing/hair and preserves paid equipped pieces;
- purchases and current selections persist independently per item/slot, and equipping an owned item never charges the wallet again;
- `KickLuckyCubeCharacterAppearance` binds the exact Blockbench group names and exported mesh-name families from `KLC_PlayerMannequin.bbmodel`, toggles one real renderer set per slot, and switches natural skin texture/material families; green and blue remain explicit fantasy tints;
- `KickLuckyCubeBlockbenchPlayerImporter` rebuilds the Unity FBX material remaps, extracted textures, runtime skin resources, and Idle/Walk/Classic Kick/DumbbellTraining/BarbellTraining Animator states from the committed Blockbench source; the player controller migrates the old `SadovnicOBJ` scene settings and applies the one documented Blockbench forward-axis correction at runtime;
- the appearance binder is safe when the Blockbench export has not been attached yet: selection/economy/UI still work, and bindings refresh when visual children appear;
- every runtime `KickLuckyCubeFakeOnlineBot` receives a deterministic random loadout, including premium pieces, without touching player ownership or currency;
- the nine generated transparent tab sprites live under `Resources/KickLuckyCube/UI/SkinTabs`; 23 model-rendered item thumbnails live under `Resources/KickLuckyCube/UI/SkinItems`; `KLC_SkinGrid.prefab` is the authored layout container used by both tab and item grids;
- `KickLuckyCubeContentThumbnailGenerator` regenerates wardrobe/tool sprites from the actual current FBX models, and catalog validation rejects missing or duplicate wardrobe sprite references.

Save contract:

- `KickLuckyCube.Wallet.Soft64` / `Hard64`: invariant string-backed long soft and hard currency; old integer `Soft` / `Hard` keys migrate once and are removed;
- `KickLuckyCube.CharacterCustomization.Selected.*`: one equipped item id per appearance slot;
- `KickLuckyCube.CharacterCustomization.Owned.*`: paid appearance ownership flags; free items do not require ownership keys;
- `KickLuckyCube.Kick.SelectedStrength` / `UseMaximum`: persisted selectable kick strength and explicit maximum mode;
- `KickLuckyCube.KickStyle.SelectedId`: stable id of the equipped kick style;
- `KickLuckyCube.KickStyle.Owned.*`: ownership flags for premium kick styles purchased with hard currency;
- `KickLuckyCube.PlayerStats.*`: strength, animal speed, speed level, owned tool tier, and selected tool tier;
- `KickLuckyCube.Inventory.State`: bottom hotbar and storage inventory mobs;
- `KickLuckyCube.Stable.Player.Template_StableSlot_*`: placed stable mob payload, pending soft, upgrade level, and last UTC save time;
- `KickLuckyCube.Rewards.*`: playtime reward elapsed time, claimed mask, and reset start time;
- `KickLuckyCube.Rebirth.*`: rebirth count, which restores the soft gain multiplier at startup;
- `KickLuckyCube.OfflineReward.LastSeenAt`: last UTC timestamp used to decide whether the offline earnings popup should appear after a 30 minute absence;
- `KickLuckyCube.Commerce.PrivilegeExpiresAt`: UTC expiry timestamp for the 30-day VIP privilege pass;
- `KickLuckyCube.Settings.*`: music, SFX, and language;
- `KickLuckyCube.Wheel.NextSpinAtUnix`: wheel cooldown stored as a UTC Unix timestamp;
- `KickLuckyCube.Wheel.NextSpinAt`: obsolete session-time cooldown key; it is deleted during migration and by the prototype save-clear tool;
- `KickLuckyCube.Future.*`: weather timer, exchange charges/timer, epic mob purchases, and rating gift claim;
- `KickLuckyCube.AnimalCollection.Discovered.*`: album discovery flags.

Latest persistence probe:

```text
Stable slots -> 14 total, 10 player persistent, 0 duplicate save ids
Player stable ids -> Player.Template_StableSlot_01 ... Player.Template_StableSlot_10
Non-player stable slots with save id -> 0
Stable in-session load probe -> temporary Probe Cat placed, AnimalJson saved, slot reloaded occupied, then probe keys cleared
Stable full restart probe -> temporary Full Cycle Probe 2 placed, Stop Play, Start Play, reloaded occupied from PlayerPrefs, then probe keys restored
Fresh Unity errors after final compile -> 0
```

Animal catalog / album:

- `KickLuckyCubeAnimalCatalog` is the current shared runtime source for 15 regular kick mobs, elite-shop exclusives, and rating gift mobs;
- every catalog mob has a stable `catalogId`, display name, rarity, body color, sell value, income per second, speed multiplier, source, optional imported visual prefab path, and optional icon path;
- regular kick mobs are expanded into four runtime grade variants through `KickLuckyCubeAnimalGrade`: Normal, Golden, Diamond, and Fire, for 60 kick variants total;
- regular kick mob income is linear across the full progression through the balance config: `30 + progressionIndex * 6`, so the current regular total range is `30-384` soft/s;
- regular kick mob sell value is derived from income through the balance config and currently equals 18 seconds of that mob's income;
- `Elite Prism Thumper`, `Elite Aurora Penguin`, and `Elite Inferno Jet` are removed from regular kick progression and sold as high-income hard-currency elite mobs;
- `KickLuckyCubeAnimalCatalog.CreateLocationOptions(locationIndex)` returns the 3-mob progression pool for that location using grade-stage ordering plus the `previous top + 2 new` rule;
- legacy serialized prototype animal ids/names such as `uncommon_boar` / `Boar` are resolved to the current imported catalog entries before spawning or loading visuals;
- imported Steal Brainrot animal visuals, animal icons, Zoo animations, Brainrot specials, and the player prefab are stored under `Assets/Games/KickLuckyCube/Resources/KickLuckyCube/StealBrainrot`;
- all 22 catalog entries currently resolve a prefab, an icon, an `Animator`, and a controller; the Play Mode gallery probe confirmed all 22 controller timelines advance;
- keep source `.vox` files outside `Resources`: an identically named `cat.vox` and `cat.prefab` share the same Resources key and Unity may return the raw VOX object without its Animator. Zoo VOX sources now live under `Assets/Games/KickLuckyCube/ArtSource/StealBrainrot/ZooVox`, preserving their GUIDs and prefab mesh references;
- Brainrot specials and Zoo models without a matching source pet icon use generated prefab-preview sprites in `Resources/KickLuckyCube/StealBrainrot/Sprites/Pets` so inventory, album, and shop cards do not show another animal's icon; `Champ` and `Chill` currently use generated `champ` / `chill` previews instead of the generic horse/sheep icons;
- `.vox` animal models require the copied `Assets/VoxelImporter` dependency; do not delete it while these imported animals are in use;
- `KickLuckyCubeAnimalVisualFactory` creates spawned, held, and stable mob visuals from catalog prefab paths, strips imported colliders, normalizes world height, starts imported `Animator` components from a deterministic grounded pose with `AlwaysAnimate`, and adds subtle procedural motion; colored grade effects instantiate editable prefabs under `Resources/KickLuckyCube/Animals`, and a missing catalog model uses the authored `KLC_AnimalFallbackVisual.prefab` with a clear diagnostic;
- `KickLuckyCubeInventoryAnimal` and `KickLuckyCubeSpawnedAnimal` carry `catalogId` and `grade` while preserving old saves by falling back to name+rarity matching and defaulting missing grade data to Normal;
- `KickLuckyCubeAnimalCollection` stores discovered flags in PlayerPrefs under `KickLuckyCube.AnimalCollection.Discovered.*`;
- mobs are marked discovered when obtained through return-to-line, inventory add, stable placement, epic shop, rating gift, or exchange result;
- `KickLuckyCubeAnimalAlbumController` creates a runtime square `Album` button and a scrollable album window with `X`, `Escape`, and backdrop-click close behavior;
- inventory slots, sell cards, epic shop cards, and the album use catalog icon sprites when available;
- undiscovered mobs appear as dark silhouettes of their icon with hidden names/stats, while discovered mobs show name, rarity, income, sell value, and source.

Shop:

- Speed cards buy the next uncapped animal speed level with soft currency;
- runtime Speed Upgrades shop exposes uncapped `+1`, `+5`, and `+10` speed bundles through `Y` / the speed kiosk pad;
- Tool cards buy the next strength tool tier with soft currency;
- runtime Training Equipment shop also exposes strength tool unlock/equip flow through `T` / `Tools` / the weights-training kiosk pad;
- strength tools now use a fixed 15-tier visual catalog that alternates odd tiers as a pair of dumbbells and even tiers as a barbell; strength-per-second and soft-currency costs remain unchanged;
- the tier material progression is Stone Dumbbells, Iron Barbell, Steel Dumbbells, Gold Barbell, Titanium Dumbbells, Obsidian Barbell, Neon Alloy Dumbbells, Meteorite Barbell, Crystal Dumbbells, Sapphire Barbell, Amethyst Dumbbells, Voidsteel Barbell, Ruby Dumbbells, Emerald Barbell, and Arcane Godstone Dumbbells;
- the authored Blockbench source is `E:/GitFork/BlockBench/KickLuckyCube/Props/Training/KLC_StrengthTools.bbmodel`; each tier is a separate root and every dumbbell tier contains distinct left/right attachment groups;
- `KickLuckyCubeBlockbenchStrengthToolsImporter` rebuilds the authored FBX, 16 textures, material remaps, and all 15 tier roots; `KickLuckyCubeToolPreviewVisual` selects that model at runtime and only uses its primitive geometry as an explicit missing-resource fallback;
- Strength boost cards buy a one-shot strength increase with soft currency;
- runtime Style shop buys and equips kick styles from the style kiosk press-interaction anchor; every non-default style costs hard currency and adds `+10%` effective kick strength; each card can preview the same motion with style-specific yaw, distance, pitch, FOV, lead-in, safe input locking, and camera restoration without purchasing or launching the cube;
- Elite Mob Shop sells one-time exclusive mobs for hard currency and immediately selects the purchased mob when inventory has space;
- the Shop window receives a prefab-backed `VIP 30 Days` card that sells the privilege pass; if IAP is supported it uses the IAP route, otherwise it spends the configured hard-currency fallback price;
- duplicated shop cards currently point to the same first-pass purchase actions and should become distinct final catalog items later.

Future feature spots:

- Weather Machine opens `KLC_WeatherMachineWindow_Runtime`, requires at least one Epic or Legendary mob in inventory or stable, shows the requirement, duration, active timer, and boost chance, then starts a 10 minute effect that gives landed cubes a chance to boost rarity before the animal roulette; `KLC_FutureFeatureStatusStrip` is hidden by default while the top-right HUD is being reworked;
- Exchange Booth opens a confirmation window for the currently selected mob, shows selected/result cards with an arrow and income delta, locks that preview until exchange or selection change, consumes one of 10 exchange charges after confirmation, restores one charge every 5 minutes, and swaps the selected inventory mob for a nearby-value random mob;
- Elite Mob Shop sells the existing special exclusives plus `Elite Prism Thumper`, `Elite Aurora Penguin`, and `Elite Inferno Jet` as one-time hard-currency purchases;
- Rating Gift Stand grants `Star Review Buddy` once per save file;
- future feature save keys are included in the prototype save reset menu.

Lucky Wheel:

- spin is free when cooldown is ready;
- weighted rewards currently include soft, hard, and strength;
- the wheel visual rotates briefly after a successful spin;
- cooldown persists through PlayerPrefs.

Settings:

- Music and SFX toggles update `KickLuckyCubeAudioSettingsApplier`;
- `KickLuckyCubeAudioChannel` marks an `AudioSource` as `Music` or `Sfx` so prefab/scene audio can be routed without hard-coding arrays in the settings window;
- Language toggle updates `KickLuckyCubeLocalizationController`;
- `KickLuckyCubeLocalizedText` can be added to prefab UI/world text and supports legacy `Text`, TMP, and `TextMesh`;
- static prefab labels in the main windows are now bound with `KickLuckyCubeLocalizedText`; dynamic labels that include prices, counts, timers, generated animal names, or state-specific button text are still controlled by their feature controllers and should be localized in a separate code pass;
- final work should add authored clips/loops, assign channels to real sources, and expand localization coverage to every user-facing text.

Rebirth:

- first rebirth requires 1500 strength;
- each next rebirth requirement is 30x higher;
- multiplier increases from x2 to x3 to x4 and so on;
- rebirth resets strength while keeping speed/tool ownership and increasing money gain.

## First Implementation Order

1. Overview blockout scene with readable landmarks. Done.
2. Kick line interaction and power-to-distance prototype. Done.
3. Cube flight and rarity zone detection. Done.
4. Animal spawn from rarity pools. Done.
5. Animal control and wave chase. Done.
6. Return success/fail flow. First prototype done.
7. Sell/stable placement. Done.
8. Stable income, collect button, and per-slot booster board. Done.
9. Strength tools and training prompts. Bottom-slot training, movement cancel, `x2`, gain FX, and runtime tool shop prototype done.
10. Speed upgrades. Runtime speed shop, kiosk pad opening, uncapped levels, and `+1`/`+5`/`+10` bundle purchases done.
11. Rebirth window and multiplier economy. Done.
12. Tool belt selection. First owned-tier selection prototype done.
13. Rewards window and playtime hard/soft claims. Done.
14. Shop card purchase wiring. First speed/tool/boost purchase prototype done.
15. Player progression and stable slot PlayerPrefs saves. First prototype done.
16. Settings audio/localization backend. First prototype done.
17. Lucky Wheel window and cooldown rewards. Done.
18. Mobile joystick and hold-interact layer. First prototype done.
19. Inventory sell shop. First prototype done.
20. Future feature spots: weather, exchange, epic mob shop, and rating gift. First prototype done.
21. Style shop and live leaderboard board. First prototype done.

## Core Loop Probe

Latest logic probe result:

```text
120 strength -> 45.6 m -> Uncommon -> Boar
Boar placed in stable -> collected 150 soft
Buy Speed -> animal speed 7.8
Buy Tool -> tool level 2
3x Train Strength -> 179 strength
179 strength -> 59.3 m -> Rare
```

This confirms the playable progression loop is now connected: kick, spawn, escape, place animal, collect soft, buy speed/tool, train strength, and kick into a deeper zone.

Latest training bonus probe:

```text
120 strength
Train Strength hold -> 132 strength
x2 prompt pending -> +24 strength
Claim x2 -> 156 strength
Buy Speed level 1 -> animal speed 7.8
```

Latest rebirth probe:

```text
999 strength -> rebirth blocked
1000 strength -> rebirth complete
rebirth count -> 1
soft gain -> x2
strength reset -> 120
animal speed/tool ownership -> kept
100 soft reward after rebirth -> 200 soft
next requirement -> 10000 strength
```

Latest UI/economy probe:

```text
Tool belt selected tier -> 2
Tool selection clamp after selecting tier 99 with 3 owned tiers -> 3
Rewards window -> closed/open/closed
First reward claim -> true, second claim attempt -> false
Shop soft before -> 1000
Buy Speed -> animal speed 7.8, speed level 1
Buy Tool -> tool level 2, selected tool 2
Buy Strength Boost -> strength 195
Shop soft after -> 670
EventSystem -> InputSystemUIInputModule, no StandaloneInputModule
```

Latest Play Mode smoke:

```text
selected tool -> 2
Buy Speed -> true
Buy Tool -> true
Buy Strength Boost -> true
Claim first playtime reward -> true
Place animal in stable -> true
Stable pending before collect -> 42
Stable collected -> 42
wallet after smoke before cleanup -> 400 soft / 5 hard
stats after smoke before cleanup -> 195 strength, 7.8 speed, tool tier 4
Play Mode cleanup reset wallet, stats, rewards, and stable slot prototype state
```

Latest final systems smoke:

```text
Settings mute -> true
Localization RU label -> Колесо
Localization EN label -> Wheel
Wheel spin -> first true, second false due cooldown
Wheel reward changed wallet/stats -> true
Mobile joystick input -> 0.4, -0.9
Mobile hold bridge -> true, then false after clear
```

Latest inventory sell shop smoke:

```text
Sell pad opened shop -> true
Sell button removed one mob -> 2 to 1
Soft currency changed -> 0 to 250
Unity console errors/exceptions -> 0
```

Latest plot/shop/future smoke:

```text
Returned mob -> added through inventory and selected as held mob
Stable slot runtime binder -> 10 player stable modules
Stable booster board -> level saved and income multiplier applied per slot
Speed shop -> opens from speed kiosk pad and buys sequential levels
Training equipment shop -> opens from weights kiosk pad, unlocks next tool tier, and re-equips owned tools
Style shop -> opens from style kiosk hold anchor, spends hard currency, persists ownership/equipment, and performs the selected kick motion
Weather machine -> opens requirement/timer window, requires Epic/Legendary mob, and can boost landed rarity before roulette
Exchange booth -> consumes/restores exchange charges, previews selected/result mob cards, then swaps selected mob
Epic mob shop/rating gift -> add exclusive mobs through inventory save path
Leaderboard -> loads editable world-text prefab and updates local/fake ranking text in Play Mode
```

Latest core-loop feature probe:

```text
Training shop pad -> object true, target true, pad true, prompt Open tools shop, mode Press
Training shop buy tier 2 -> owned 2, selected 2
Future status pills -> weather true, exchange true
Exchange window -> active true, selected/result cards shown, income delta visible, confirms previewed replacement, charges 0
Epic mob shop buy #1 -> owned true, button disabled after purchase, text Owned
Weather status -> active true, label Weather active 10:00, Rare boosted to Epic
Rebirth -> true, count 1, strength reset to 120, tool tier kept at 2, soft x2, +100 gives 200
```

## Latest Agent Test Pass

Date: 2026-08-02

Unity Test Framework:

```text
EditMode: passed, 1 total, 1 passed, 0 failed
PlayMode: no PlayMode tests found
```

Current targeted runtime probes:

```text
Complete feature/prefab contract -> pass
Inventory slot views -> 23
Allocated plots / active fake-online actors -> 4 / 3
Runtime visual source audit -> 25 classified, 0 migration, 0 unclassified
Catalog validator -> 22 entries, 0 errors
Unity Console errors/exceptions -> 0 / 0
```

The strict `118`-object grounding/collider diagnostic is not a release pass: `23` visual-only animated limbs and biome decoration objects do not own colliders. Their gameplay roots stayed grounded, but biome collider coverage must be reviewed during `KLC-PROD-009`.

Editor scene probes:

```text
Active scene -> KickLuckyCubeOverview
Scene dirty -> false
Required roots -> OK
Core systems -> OK
UI/economy/settings/mobile systems -> OK
Side buttons -> Shop, Settings, Rewards, Rebirth, Wheel present
Window controllers -> Shop, Settings, Rewards, Wheel
Window open/close -> closed/open/closed for all four windows
EventSystem -> InputSystemUIInputModule
StandaloneInputModule -> 0
KLC_ProBuilderVisuals -> 270 ProBuilderMesh objects, 0 colliders
KLC_FakeOnline -> 0 colliders
```

Runtime smoke:

```text
Kick distance at starter strength -> 45.6
Landing rarity -> Uncommon
Animal run starts -> true
Animal return/carry -> true
Stable placement -> true
Stable pending/collect -> 42 / 42
Stable save sample -> Boar, 0 pending after collect
Second carried animal sell -> true, +82 soft
Tool belt selected tier -> 2
Shop speed/tool/strength boost buys -> true / true / true
PlayerPrefs progression sample -> strength 195, speed level 1
Rewards claim -> first true, second false
Wheel spin -> first true, second false due cooldown
Rebirth -> true, count 1, multiplier x2
Settings mute -> true
Localization RU label -> Колесо
Mobile simulation visibility -> true then false
Mobile joystick sample -> 0.4, -0.9
Mobile hold bridge -> true then false
```

Latest camera probe:

```text
Start target -> KLC_PrototypePlayer
Start camera position -> 0.0, 4.6, -8.9
Start camera rotation -> 18.0, 0.0, 0.0
After kick active run -> true
Run target -> KLC_Runner_Boar
```

Manual test focus for the next human pass:

1. Feel of the full kick/run/carry/inventory/stable loop with real camera movement.
2. Style shop, speed shop, tool shop, future spots, and leaderboard readability around the hub.
3. Whether stable slot collect/upgrade pads are obvious enough from the placed plot view.
4. UI readability at desktop and narrow/mobile-like aspect ratios.
5. Whether wheel/rewards/shop pacing feels too generous or too slow.
6. Whether mobile joystick and hold button placement is comfortable on a phone viewport.

## Visual Implementation Order

1. First ProBuilder visual pass over the overview blockout. Done.
2. Authored lucky cube prefab with black edges and question marks. Done.
3. ProBuilder-authored zone gates/signs per rarity. First rough pass is disabled as too noisy; next pass should use sparse milestone signs only.
4. ProBuilder-authored stable/sell/training props.
5. Final cleanup pass: remove or hide old visual blockout primitives after gameplay anchors are separated.

## Game-Specific Rules

All unique mechanics stay in:

```text
Assets/Games/KickLuckyCube/
```

Move behavior to `Assets/GameKit` only after it becomes reusable across multiple games.

## 2026-08-01 Shared Checkpoint

Before the current shared checkpoint was committed:

- MCP core was updated to `0.86.3`;
- Animation, ParticleSystem, and ProBuilder extensions were updated to `1.2.30`;
- InputSystem extension was updated to `1.0.16`;
- Unity AssetDatabase refresh completed without compile errors;
- Unity Console returned no Error or Exception entries after compilation;
- EditMode tests passed `1/1`;
- no PlayMode tests were discovered, so the full gameplay route remains a manual verification requirement;
- all current Kick Lucky Cube assets have matching `.meta` files;
- all 18 moved Zoo `.vox` sources preserved their GUIDs under `ArtSource`;
- all 22 animal catalog entries resolved both a runtime visual and icon;
- the catalog produced the expected 60 regular kick variants;
- the active scene and all Kick Lucky Cube prefabs were scanned for missing components; only the unused legacy `Models/Player/Root.prefab` skeleton contains old missing references.

The concise contributor workflow and ownership map are maintained in `Docs/Games/KickLuckyCubeHandoff.md`.

The manual world/UI/animation workflow is maintained in `Docs/Games/KickLuckyCubeAuthoringTools.md`. Open it from Unity through `Tools > Kick Lucky Cube > Authoring Workspace`; the workspace is the supported replacement for broad procedural world rebuilding during the human art pass.
