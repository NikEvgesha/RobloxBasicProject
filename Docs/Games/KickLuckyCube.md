# Kick Lucky Cube

## Game

Name: Kick Lucky Cube

Game id: kick-lucky-cube

Status: playable prototype / overview blockout

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
- lucky cube flight from the kick position selected by the first `E` press;
- flight trail FX on the lucky cube while it is in the air;
- third-person camera target switches to the flying cube during flight;
- distance calculation from current strength and the selected kick power meter value;
- kick power meter with a red/yellow/green strength background, a green fill bar, and moving marker;
- kick power selection cancels if the player steps away after the first `E` press;
- smaller lucky cube scale for the active prototype scene;
- lower landing on the active `Floor_FlatGreenGrass` gameplay floor collider, followed by hiding the cube after impact;
- no persistent landing marker/platform is shown after cube impact;
- rarity zone detection by corridor depth;
- animal roulette phase after landing: shadow silhouettes cycle through the landed rarity pool at runner height, slow down, then select one random animal;
- third-person camera target switches to the roulette preview until selection ends;
- animal spawn from the selected landed-rarity pool result;
- runner phase for the spawned animal, with camera-relative controls matching the prototype player, jump support, side-boundary clamping, and ground snapping to the real `Floor_FlatGreenGrass` play surface;
- third-person camera blends from the roulette preview to the spawned animal when control transfers after selection;
- chasing wave visual behind the animal, started only after the roulette selection and a short wave-rise camera intro;
- wave speed scales up from the kick distance and is shown as a world label above the wave;
- red screen-edge danger vignette that intensifies as the wave approaches the animal;
- return success state respawns the prototype player at the animal's finish point, stores the returned animal through the inventory, and selects it as the active held mob;
- runtime inventory UI with slot 1 reserved for the selected training tool and slots 2-5 reserved for up to four mobs;
- inventory window opened by `I` or the `Bag` button, with drag/drop movement between full inventory slots and bottom mob slots;
- inventory hotbar/storage mobs are saved in PlayerPrefs and restored in Play Mode;
- empty bottom mob slots are hidden in normal play and shown as drop targets only while the inventory window is open;
- the inventory window shows only occupied slots; dropping a bottom-bar mob onto empty inventory-window space creates the next occupied slot there;
- selecting a mob slot stops active tool training and shows a temporary mob preview in the player's `KLC_CarryAnchor`;
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
- HUD/status text with current strength, tool level, animal speed, predicted distance, landing result, run phase state, and economy state;
- progression stations for training strength, buying animal speed, and buying the next strength tool;
- bottom inventory bar that replaces the old tool belt in Play Mode and lets slot 1 toggle active tool training after clearing the active mob only when training can start;
- active tool training shows randomized strength gain bursts around the player, flies them to the strength HUD, moves the held tool with the squat animation, and grants strength once per second;
- moving while tool training is active immediately cancels training and hides the held tool;
- `x2` training bonus prompt is shown as a circle-only `x2` button that stays pending until clicked, pressed with `X`, or until tool training stops;
- runtime Training Equipment shop can unlock the next strength tool tier and equip already-owned tools through the `Tools` button, `T`, or the weights-training kiosk stand pad;
- runtime Speed Upgrades shop opens from the speed kiosk stand pad or `Y` and buys affordable `+1`, `+5`, or `+10` animal speed level bundles;
- runtime Style shop opens from the style kiosk hold-interaction anchor and buys/equips lucky cube color skins;
- runtime Future Feature spots support weather rarity boosts, selected-mob exchange charges with confirmation UI, three exclusive epic-shop mobs, and a one-time rating gift mob;
- runtime Leaderboard board displays a live prototype score list based on strength, soft currency, and owned mobs;
- working Shop card purchases for speed upgrades, strength tools, and one-shot strength boosts;
- working playtime Rewards window with soft/hard claims;
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

Current ProBuilder coverage:

- launch platform trims, gate, kick arrows, and lucky cube dress-up;
- rarity gates and colored zone accents;
- river banks, bridge planks, rails, and posts;
- stable building shell, roof, fences, hay blocks, and coin markers;
- sell kiosk with counter, awning, and coin stacks;
- training rack/tool placeholders;
- stylized wave wall and foam crests;
- corridor fence details.

Current verification:

```text
KLC_ProBuilderVisuals -> 270 ProBuilderMesh objects
Visual bridge MeshColliders -> 32
Main corridor floor guide MeshCollider -> 1
```

Next visual priorities:

1. Replace rough floor labels with diegetic 3D signs.
2. Convert the main lucky cube visual into a single authored ProBuilder prefab.
3. Add zone-specific props and animal silhouettes per rarity.
4. Improve stable/sell/training areas with final scale and interaction-readable silhouettes.
5. Reduce or hide old blockout primitives only after every gameplay reference is moved to dedicated anchors/triggers.

## UI Visual Layer

The first UI visual pass lives in the overview scene under:

```text
KLC_UIBackplates
KLC_UIVisualPass
KLC_UIWindows
```

This pass uses the imported casual Roblox UI toolkit from `Assets/SharedArt/UI` and keeps the existing gameplay HUD objects alive:

- `KLC_KickHud` still receives status text from gameplay scripts;
- `KLC_EconomyHud` still receives economy text from gameplay scripts;
- `KLC_InteractionPrompt` still uses the shared `GameKitInteractionPromptView`.

Current UI coverage:

- top-left kick/status backplate;
- top-center objective strip;
- top-right soft/hard currency panel with toolkit icons;
- left-side square icon buttons for Shop, Settings, Rewards, and Rebirth;
- bottom tool belt with four visual slots;
- restyled hold-`E` interaction prompt;
- runtime home icon above the player's plot, drawn in its own high-order screen-space overlay canvas with a black outline, so it remains visible through walls and clamps to the screen edge when the home is off-camera;
- runtime currency gain numbers burst around the player and fly to the wallet HUD after any soft/hard wallet gain;
- runtime future-feature status pills show current Weather and Exchange charge state;
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
- Settings contents are UI-backed, but the final audio mixer and localization routing are still pending.
- the current audio backend applies Music/SFX toggles to configured sources, and falls back to `AudioListener.volume` while there are no final scene audio sources;
- `KickLuckyCubeSfxController` currently generates short procedural prototype sounds at runtime for money gain, purchases, stable place/take, stable upgrades, and denied clicks; these are placeholder SFX until final authored clips are chosen;
- the first localization backend applies EN/RU text to selected UI labels from the Settings language toggle.

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
- the next spin time is saved in PlayerPrefs;
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
```

The Rebirth side button is wired to a working window:

- the window opens from the left-side `Rebirth` button;
- the window closes through the red close button or backdrop click;
- first rebirth requires 1000 strength;
- each next rebirth requirement is 10x higher;
- rebirth resets strength only while keeping speed upgrades and owned strength tools;
- soft gain multiplier becomes `x2`, then `x3`, then `x4`, and so on.

## Fake Online Ambient Layer

The fake online pass lives in the overview scene under:

```text
KLC_FakeOnline
```

This layer is visual/ambient only:

- no colliders are added under `KLC_FakeOnline`;
- fake bots do not spend player currency;
- fake mobs do not generate player income;
- the layer can be deleted and regenerated without changing the real core loop.

Current fake online coverage:

- 4 neighboring player plots: `Astrozto`, `MiraKit`, `NoobPro77`, `LuckyMax`;
- 12 pre-upgraded stable mobs across Common, Uncommon, Rare, Epic, and Legendary tiers;
- animated bots driven by `KickLuckyCubeFakeOnlineBot`;
- bot activity cycle: patrol, train with a tool, kick a decorative lucky cube.

Latest Play Mode check:

```text
plots=4
bots=4
fake stable mobs=12
colliders=0
```

Default tuning:

```text
distance = 18 + strength * 0.23
minimum distance = 10
maximum distance = 132
starter strength = 120
```

Expected test values:

```text
0 strength -> Common
120 strength -> Uncommon rarity pool; current main flow picks a random pool animal through the roulette
500 strength -> Legendary
Boar sell value -> 82 soft
Boar stable income -> 3 soft/s
```

To test manually, open `KickLuckyCubeOverview`, enter Play Mode, and click bottom slot 1 or press `1` to start/stop tool training. The player should hold the tool, squat, gain strength each second, show strength gain text bursting from different points around the player and flying to the strength HUD, and show at most one pending `x2` circle every 5 seconds. Walking while training should immediately cancel training. Then stand near the kick interaction area; the lucky cube should appear in the player's hands.
Press `E` once to start the power meter, then press `E` again to kick with the current meter value. If the player moves after starting the meter, the meter is cancelled.
The camera follows the cube during flight; the cube leaves a trail, lands on the lower corridor floor, then disappears.
After landing, the animal roulette cycles through shadow silhouettes from the landed rarity pool and slows down on one selected animal.
Only after that selection does the wave rise intro play: the camera focuses close to the front of the wave, the speed label is shown, then the camera blends back behind the selected animal and the chase starts. Run back toward the kick start point before the wave reaches it; the red vignette should intensify as the wave gets close.
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
E: press once to start kick power selection, press again to kick; press near sell/speed kiosks to open their shops; press near stable collect spots to place/take mobs; hold for collect-all and progression stations
Mouse left click: upgrade an occupied stable slot from its `UpgradeBoard`
Hold E near style kiosk: open cube-style shop
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
- `KLC_PrototypePlayer` uses `KickLuckyCubePlayerController` and a blocky child visual named `KLC_PlayerVisual`;
- `KLC_PlayerVisual` has `KickLuckyCubePlayer.controller` with idle, move, walk, and run clips copied from the MechanicsTestbed prototype and stored under `Assets/Games/KickLuckyCube/Animations`;
- during animal run phase it follows the current spawned animal;
- after run/fail/sell/stable flow it falls back to the prototype player target.

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
- `KLC_PlotInstance_MOVE_PlotSlot_01` is currently auto-bound in Play Mode by `KickLuckyCubeStablePlotRuntimeBinder`: its ten direct `Template_StableSlot_*` children receive stable logic, a trigger on `CollectSpot`, an `E` interaction for place/take, automatic income collection while standing on the collect spot, and a mouse-click `UpgradeBoard` for per-slot boosters;
- the player's current plot also gets a runtime home icon through `KickLuckyCubeHomeIconMarker`; it follows the plot in a dedicated UI overlay canvas, stays visible through walls and other 3D blockers, and clamps to the screen edge when the home is behind the camera;
- empty stable collect spots show `Поставить` only while the player is carrying/selecting a mob; occupied stable collect spots show `Забрать` and return the mob to inventory, preferring free bottom hotbar slots;
- standing on an occupied stable `CollectSpot` automatically claims pending soft currency without pressing `E`;
- each occupied `CollectSpot` shows the pending soft amount as a short white label with black outline fixed inside the green pad; it does not billboard toward the camera and shows `0` while the slot has a mob but no pending income;
- stable `UpgradeBoard` objects are mouse-click targets, not `E` prompts, and their runtime text is limited to level plus compact price; the runtime label size is derived from the board mesh height so it stays proportional to the stable board;
- spawned and stabled mob capsules use a black inverse-hull outline for clearer Roblox-style readability;
- stable mob anchor placeholder visuals are hidden at runtime; only actual placed mobs should be visible on slots;
- occupied stable slots normalize placed mobs to a target world height from renderer bounds, independent of the non-uniform `MobAnchor` scale, and show a white TextMesh with black outline above the mob; the label contains only mob name, stable level, and income per second, faces the camera from an unscaled runtime label root, and hides at distance to reduce visual clutter;
- `Template_FloorExpansion` contains visual-only posts, ladder, and outline beams for future upper-floor expansion;
- `MOVE_PlotSlot_01..04` define placed plot locations;
- `KLC_PlotSlot_StaticInstances` contains edit-mode plot copies placed from the reusable template on every current `MOVE_PlotSlot`;
- `KLC_PlotTemplate_EditSource` is kept inactive as the reusable edit source and should be enabled only when editing the template shape;
- runtime plot auto-allocation is currently disabled to avoid duplicate plots while static scene placement is being tuned;
- `KLC_HubKiosks_Blockout/KLC_ImmediateKiosks_LeftToRight` holds the current left-to-right hub kiosks: animal sell, style shop, speed upgrade, weights training, and leaderboard;
- sell, speed, and weights kiosks have visible stand pads; style shop uses a hold-interaction anchor and opens the runtime style shop;
- `KLC_HubKiosks_Blockout/KLC_FutureFeatureSpots` holds runtime-bound prototype spots for weather machine, animal exchange, epic mob shop, and rating gift stand;
- plot template, placed plot copies, and hub kiosk blockout meshes are converted to `ProBuilderMesh`; `TextMesh` labels remain regular text objects;
- `00_BaseEnvelope_DoNotMoveAsAGroup` holds the flat green grass floor and tan boundary walls;
- `02_ZoneAndRiverGuides` holds non-final zone, corridor, and river guides;
- older noisy prototype visual groups are disabled while this layout pass is being placed.

Mobile controls:

```text
Virtual joystick: move animal runner
Jump button: queue a prototype player jump
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

Initial zone direction:

- Zone 1: Common;
- Zone 2: Uncommon;
- Zone 3: Rare;
- Zone 4: Epic;
- Zone 5: Legendary.

Each zone is separated by a river gap. The deeper the cube lands, the better the animal pool.

## Progression

Strength:

- current prototype trains from bottom slot 1: the selected named tool appears in-hand, the player squats, the tool moves with the squat, and tier-based strength is added every second;
- the green `Train Strength` station remains as an older prototype fallback;
- each tool level increases strength gained per hold;
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
- each next single level costs more than the previous level;
- each bought level increases animal runner speed by the shop's configured gain and affects the animal while escaping the wave;
- the speed shop closes automatically when the player leaves the speed kiosk stand pad.

Stable income:

- stable animals generate soft currency per second;
- every stable slot starts at booster level 1; each upgrade increases only that slot's income multiplier by 20%;
- slot upgrade levels are uncapped; upgrade costs start at 150 soft and scale by 1.65x per level;
- each occupied stable slot is claimed automatically by standing on its own green `CollectSpot`;
- each occupied stable slot has a larger placed mob, a black outline, and a distance-gated billboard label above the mob;
- stable upgrade boards show only the current level and next price, and are bought by mouse click;
- the green collect-all board resolves runtime stable slots dynamically and claims the combined pending income;
- placed stable animals, pending income, upgrade level, and last save time are stored in PlayerPrefs in Play Mode with a plot-instance-specific slot id;
- offline stable income is calculated from the saved UTC timestamp by real elapsed seconds; the default prototype setting has no offline cap, so next-day returns still accumulate income.
- prototype saves, including inventory, can be cleared in the Unity Editor through `Tools/Kick Lucky Cube/Clear Prototype Save`.

Inventory / tool bar:

- bottom slot 1 shows the active named training tool, its owned tier/status, and toggles tool training;
- bottom slots 2-5 hold up to four returned mobs;
- unused bottom mob slots are hidden unless the inventory window is open;
- overflow mobs go into the temporary inventory window but can still become the active held mob;
- mobs can be dragged between inventory-window slots and bottom-bar mob slots, or dropped onto empty window space to create the next inventory slot;
- selected mobs are shown in `KLC_CarryAnchor`; selecting the same mob again deselects it, and selecting any mob stops active tool training;
- the sell kiosk opens a runtime sell window with 3-column square mob cards and removes sold mobs from the bottom bar/inventory while adding their sell value to the wallet;
- selected inventory mobs can be placed into an empty stable `CollectSpot`; if placement fails, the mob is returned to the hotbar/inventory;
- inventory persistence is active, and stable slot persistence stores the full inventory animal payload.

Shop:

- Speed cards buy the next uncapped animal speed level with soft currency;
- runtime Speed Upgrades shop exposes uncapped `+1`, `+5`, and `+10` speed bundles through `Y` / the speed kiosk pad;
- Tool cards buy the next strength tool tier with soft currency;
- runtime Training Equipment shop also exposes strength tool unlock/equip flow through `T` / `Tools` / the weights-training kiosk pad;
- Strength boost cards buy a one-shot strength increase with soft currency;
- runtime Style shop buys and equips lucky cube color skins from the style kiosk hold-interaction anchor;
- Epic Mob Shop sells three one-time exclusive mobs and immediately selects the purchased mob when inventory has space;
- duplicated shop cards currently point to the same first-pass purchase actions and should become distinct final catalog items later.

Future feature spots:

- Weather Machine requires at least one Epic or Legendary mob in inventory or stable, lasts 10 minutes, gives landed cubes a chance to boost rarity before the animal roulette, and is shown in the future-feature status strip while active;
- Exchange Booth opens a confirmation window for the currently selected mob, consumes one of 10 exchange charges after confirmation, restores one charge every 5 minutes, and swaps the selected inventory mob for a nearby-value random mob;
- Epic Mob Shop sells `Crystal Griffin`, `Neon Hydra`, and `Sun Kaiju` as one-time exclusive purchases;
- Rating Gift Stand grants `Star Review Buddy` once per save file;
- future feature save keys are included in the prototype save reset menu.

Lucky Wheel:

- spin is free when cooldown is ready;
- weighted rewards currently include soft, hard, and strength;
- the wheel visual rotates briefly after a successful spin;
- cooldown persists through PlayerPrefs.

Settings:

- Music and SFX toggles update `KickLuckyCubeAudioSettingsApplier`;
- Language toggle updates `KickLuckyCubeLocalizationController`;
- final work should replace the fallback listener mute with real music/SFX source groups and expand localization coverage to every user-facing text.

Rebirth:

- first rebirth requires 1000 strength;
- each next rebirth requirement is 10x higher;
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
Stable slot runtime binder -> 10 stable modules on KLC_PlotInstance_MOVE_PlotSlot_01
Stable booster board -> level saved and income multiplier applied per slot
Speed shop -> opens from speed kiosk pad and buys sequential levels
Training equipment shop -> opens from weights kiosk pad, unlocks next tool tier, and re-equips owned tools
Style shop -> opens from style kiosk hold anchor and applies selected cube color
Weather machine -> requires Epic/Legendary mob and can boost landed rarity before roulette
Exchange booth -> consumes/restores exchange charges and swaps selected mob
Epic mob shop/rating gift -> add exclusive mobs through inventory save path
Leaderboard -> updates local/fake ranking text in Play Mode
```

Latest core-loop feature probe:

```text
Training shop pad -> object true, target true, pad true, prompt Open tools shop, mode Press
Training shop buy tier 2 -> owned 2, selected 2
Future status pills -> weather true, exchange true
Exchange window -> active true, selected mob shown, confirms replacement, charges 0
Epic mob shop buy #1 -> owned true, button disabled after purchase, text Owned
Weather status -> active true, label Weather active 10:00, Rare boosted to Epic
Rebirth -> true, count 1, strength reset to 120, tool tier kept at 2, soft x2, +100 gives 200
```

## Latest Agent Test Pass

Date: 2026-05-31

Unity Test Framework:

```text
EditMode: passed, 1 total, 1 passed, 0 failed
PlayMode: no PlayMode tests found
```

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
2. ProBuilder-authored lucky cube prefab.
3. ProBuilder-authored zone gates/signs per rarity.
4. ProBuilder-authored stable/sell/training props.
5. Final cleanup pass: remove or hide old visual blockout primitives after gameplay anchors are separated.

## Game-Specific Rules

All unique mechanics stay in:

```text
Assets/Games/KickLuckyCube/
```

Move behavior to `Assets/GameKit` only after it becomes reusable across multiple games.
